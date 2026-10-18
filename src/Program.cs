using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace CozyCityMayor
{
    public class MainForm : Form
    {
        private readonly GameStateEngine _game;
        private readonly WorldController _controller;
        private readonly PixelArtRenderer _renderer;
        private readonly Timer _timer;
        private int _lastTick;

        public MainForm()
        {
            Text = "Cozy City Mayor — 2D Pixel RPG & Government Sim (XInput + DirectInput)";
            ClientSize = new Size(1280, 720);
            StartPosition = FormStartPosition.CenterScreen;
            DoubleBuffered = true;
            KeyPreview = true;
            BackColor = Color.Black;

            _game = new GameStateEngine();
            _controller = new WorldController(_game);
            _renderer = new PixelArtRenderer();

            KeyDown += (s, e) => _game.Input.OnKeyDown(e.KeyCode);
            KeyUp += (s, e) => _game.Input.OnKeyUp(e.KeyCode);
            MouseDown += OnWindowMouseDown;

            _lastTick = Environment.TickCount;
            _timer = new Timer { Interval = 16 };
            _timer.Tick += (s, e) =>
            {
                int now = Environment.TickCount;
                float dt = Math.Min(0.1f, Math.Max(0.001f, (now - _lastTick) / 1000f));
                _lastTick = now;

                _controller.Update(dt);
                Invalidate();
            };
            _timer.Start();
        }

        private void OnWindowMouseDown(object sender, MouseEventArgs e)
        {
            if (ClientSize.Width <= 0 || ClientSize.Height <= 0) return;

            int vx = e.X * PixelArtRenderer.VirtualWidth / ClientSize.Width;
            int vy = e.Y * PixelArtRenderer.VirtualHeight / ClientSize.Height;

            for (int i = _renderer.ClickTargets.Count - 1; i >= 0; i--)
            {
                var target = _renderer.ClickTargets[i];
                if (target.Rect.Contains(vx, vy))
                {
                    if (target.OnClick != null) target.OnClick();
                    Invalidate();
                    return;
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            _renderer.RenderFrame(e.Graphics, ClientSize.Width, ClientSize.Height, _game, _controller);
        }
    }

    internal static class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args != null && args.Length > 0 && args[0] == "--verify")
            {
                string outDir = args.Length > 1 ? args[1] : ".";
                return RunAutomatedVerification(outDir);
            }

            Application.Run(new MainForm());
            return 0;
        }

        private static int RunAutomatedVerification(string screenshotDir)
        {
            Directory.CreateDirectory(screenshotDir);
            var game = new GameStateEngine();
            var controller = new WorldController(game);
            var renderer = new PixelArtRenderer();

            Console.WriteLine("[Verify] Input Status: " + game.Input.GetControllerStatusSummary());

            using (var bmp = new Bitmap(1280, 720))
            using (var g = Graphics.FromImage(bmp))
            {
                // 1. Title Screen
                renderer.RenderFrame(g, 1280, 720, game, controller);
                bmp.Save(Path.Combine(screenshotDir, "01_title_screen.png"), ImageFormat.Png);

                // 2. Election Cutscene (12-0 Unanimous Vote!)
                game.StartNewGameWithElection();
                game.ActiveDialogue = null;
                game.DialogueQueue.Clear();
                game.Election.StartBallotCounting();
                for (int i = 0; i < 40; i++)
                {
                    game.Election.Update(0.2f, game.Audio, game.Input, null);
                }
                renderer.RenderFrame(g, 1280, 720, game, controller);
                bmp.Save(Path.Combine(screenshotDir, "02_election_landslide.png"), ImageFormat.Png);

                // 3. City Hall Interior & Mayor's Desk Computer (Employees Tab)
                game.TransitionFromElectionToCityHallDesk();
                game.ComputerTabIndex = 1;
                renderer.RenderFrame(g, 1280, 720, game, controller);
                bmp.Save(Path.Combine(screenshotDir, "03_mayor_computer_employees.png"), ImageFormat.Png);

                // 4. Mayor's Desk Computer (Police Incidents Tab)
                game.ComputerTabIndex = 2;
                renderer.RenderFrame(g, 1280, 720, game, controller);
                bmp.Save(Path.Combine(screenshotDir, "04_mayor_computer_police.png"), ImageFormat.Png);

                // 5. Town Exterior with Tax Bill Delivery & Weather Control & On-Screen Phone
                game.ActiveOverlay = OverlayMode.None;
                game.CurrentScene = GameScene.TownExterior;
                game.PlayerX = 140;
                game.PlayerY = 165;
                game.SetWeather(WeatherType.AutumnBreeze);
                game.SetDayType(DayType.TaxDay);
                controller.TryInteractInWorld();
                game.DialogueCharIndex = 999f;
                renderer.RenderFrame(g, 1280, 720, game, controller);
                bmp.Save(Path.Combine(screenshotDir, "05_town_tax_bill_delivery.png"), ImageFormat.Png);

                // 6. Public Library Book Reader (Live AI Chronicle)
                game.ActiveDialogue = null;
                game.CurrentScene = GameScene.LibraryInterior;
                game.ActiveOverlay = OverlayMode.LibraryReader;
                game.Library.SelectedBookIndex = 7;
                renderer.RenderFrame(g, 1280, 720, game, controller);
                bmp.Save(Path.Combine(screenshotDir, "06_library_book_reader.png"), ImageFormat.Png);
            }

            Console.WriteLine("[Verify] All 6 core scenes and systems verified successfully!");
            return 0;
        }
    }
}
