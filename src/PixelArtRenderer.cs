using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace CozyCityMayor
{
    public class PixelArtRenderer
    {
        public const int VirtualWidth = 640;
        public const int VirtualHeight = 360;

        private readonly Bitmap _pixelBuffer;
        private readonly Font _fontTiny;
        private readonly Font _fontSmall;
        private readonly Font _fontBold;
        private readonly Font _fontTitle;
        private readonly Font _fontHeader;

        public List<ClickRegion> ClickTargets { get; private set; }

        public PixelArtRenderer()
        {
            ClickTargets = new List<ClickRegion>();
            _pixelBuffer = new Bitmap(VirtualWidth, VirtualHeight);
            _fontTiny = new Font("Consolas", 6.5f, FontStyle.Regular);
            _fontSmall = new Font("Consolas", 7.5f, FontStyle.Regular);
            _fontBold = new Font("Consolas", 8.0f, FontStyle.Bold);
            _fontHeader = new Font("Consolas", 10.0f, FontStyle.Bold);
            _fontTitle = new Font("Consolas", 14.0f, FontStyle.Bold);
        }

        public Bitmap GetPixelBuffer() { return _pixelBuffer; }

        public void RenderFrame(Graphics windowGraphics, int clientWidth, int clientHeight, GameStateEngine game, WorldController controller)
        {
            ClickTargets.Clear();

            using (Graphics g = Graphics.FromImage(_pixelBuffer))
            {
                g.SmoothingMode = SmoothingMode.None;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;

                if (game.CurrentScene == GameScene.TitleScreen)
                {
                    DrawTitleScreen(g, game);
                }
                else if (game.CurrentScene == GameScene.ElectionCutscene)
                {
                    DrawElectionScene(g, game);
                }
                else
                {
                    DrawActiveWorldScene(g, game, controller);
                }

                if (game.ActiveDialogue != null)
                {
                    DrawDialogueBox(g, game);
                }
            }

            windowGraphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            windowGraphics.PixelOffsetMode = PixelOffsetMode.Half;
            windowGraphics.SmoothingMode = SmoothingMode.None;
            windowGraphics.DrawImage(_pixelBuffer, new Rectangle(0, 0, clientWidth, clientHeight));
        }

        private void DrawTitleScreen(Graphics g, GameStateEngine game)
        {
            using (var skyBrush = new LinearGradientBrush(new Rectangle(0, 0, VirtualWidth, VirtualHeight),
                Color.FromArgb(42, 34, 78), Color.FromArgb(195, 105, 85), LinearGradientMode.Vertical))
            {
                g.FillRectangle(skyBrush, 0, 0, VirtualWidth, VirtualHeight);
            }

            using (var hillBrush = new SolidBrush(Color.FromArgb(48, 72, 68)))
            {
                g.FillRectangle(hillBrush, 0, 230, VirtualWidth, 130);
            }

            DrawBuildingSprite(g, 275, 165, 90, 70, Color.FromArgb(185, 95, 75), Color.FromArgb(75, 55, 65), "CITY HALL", true);
            DrawBuildingSprite(g, 155, 185, 75, 50, Color.FromArgb(65, 95, 145), Color.FromArgb(45, 55, 85), "POLICE", false);
            DrawBuildingSprite(g, 410, 185, 75, 50, Color.FromArgb(135, 85, 140), Color.FromArgb(65, 45, 75), "LIBRARY", false);

            DrawWeatherParticles(g, game);

            DrawPixelPanel(g, 110, 24, 420, 92, Color.FromArgb(235, 42, 30, 28), Color.FromArgb(255, 215, 105));
            DrawCenteredText(g, "COZY CITY MAYOR", _fontTitle, Color.FromArgb(255, 230, 120), 34);
            DrawCenteredText(g, "2D Pixel RPG • City Builder • Tax & Police Sim • AI Advisor", _fontBold, Color.FromArgb(255, 200, 165), 60);
            DrawCenteredText(g, "Single-Player Edition | Native XInput & DirectInput Gamepad Support", _fontTiny, Color.FromArgb(170, 230, 195), 82);
            DrawCenteredText(g, game.Input.GetControllerStatusSummary(), _fontTiny, Color.FromArgb(220, 220, 240), 96);

            string[] options = new[]
            {
                "1. Start New Mayoral Term (Election Day Cutscene!)",
                game.HasSaveFile() ? "2. Continue Saved City (Load Mayor Term)" : "2. Quick-Start Directly at City Hall Computer",
                "3. Exit to Desktop"
            };

            for (int i = 0; i < options.Length; i++)
            {
                int by = 242 + i * 30;
                bool sel = game.TitleMenuIndex == i;
                Rectangle btnRect = new Rectangle(155, by, 330, 24);
                Color bg = sel ? Color.FromArgb(235, 155, 65) : Color.FromArgb(58, 44, 42);
                Color border = sel ? Color.FromArgb(255, 245, 160) : Color.FromArgb(140, 110, 95);
                DrawPixelPanel(g, btnRect.X, btnRect.Y, btnRect.Width, btnRect.Height, bg, border);
                DrawCenteredText(g, (sel ? "► " : "  ") + options[i], _fontBold, Color.White, by + 5);

                int captured = i;
                ClickTargets.Add(new ClickRegion(btnRect, () =>
                {
                    game.TitleMenuIndex = captured;
                    if (captured == 0) game.StartNewGameWithElection();
                    else if (captured == 1)
                    {
                        if (!game.LoadGame()) game.InitializeGameData();
                        game.TransitionFromElectionToCityHallDesk();
                    }
                    else Environment.Exit(0);
                }));
            }

            DrawCenteredText(g, "Controls: WASD/Arrows or Gamepad Stick • [E]/Enter/(A) Confirm • [P]/(Y) Phone • [C] Computer", _fontTiny, Color.FromArgb(235, 225, 200), 342);
        }

        private void DrawElectionScene(Graphics g, GameStateEngine game)
        {
            g.Clear(Color.FromArgb(105, 175, 225));
            using (var grassBrush = new SolidBrush(Color.FromArgb(92, 168, 88)))
                g.FillRectangle(grassBrush, 0, 120, VirtualWidth, 240);

            using (var stoneBrush = new SolidBrush(Color.FromArgb(185, 178, 165)))
                g.FillRectangle(stoneBrush, 80, 145, 480, 195);

            DrawPixelPanel(g, 130, 62, 380, 105, Color.FromArgb(135, 82, 48), Color.FromArgb(245, 200, 90));
            DrawPixelPanel(g, 140, 20, 360, 44, Color.FromArgb(165, 42, 52), Color.FromArgb(255, 220, 105));
            DrawCenteredText(g, "★ MAPLEWOOD VALLEY MAYORAL ELECTION ★", _fontBold, Color.FromArgb(255, 235, 130), 25);

            string tallyText = "YOU (" + game.MayorName + "): " + game.Election.PlayerVotes + " VOTES   |   CORNELIUS VON MONOCLE: " + game.Election.RivalVotes + " VOTES";
            DrawCenteredText(g, tallyText, _fontBold, Color.FromArgb(140, 255, 160), 42);

            DrawPixelPanel(g, 205, 118, 46, 36, Color.FromArgb(70, 135, 95), Color.FromArgb(255, 225, 110));
            DrawCharacterSprite(g, 228, 105, Color.FromArgb(65, 155, 95), Color.FromArgb(95, 55, 30), true, 0, "YOU (MAYOR)");

            DrawPixelPanel(g, 302, 112, 36, 38, Color.FromArgb(230, 195, 85), Color.White);
            g.DrawString("VOTE", _fontTiny, Brushes.Black, 307, 124);

            DrawPixelPanel(g, 390, 118, 46, 36, Color.FromArgb(95, 55, 125), Color.FromArgb(180, 180, 195));
            DrawCharacterSprite(g, 413, 105, Color.FromArgb(95, 55, 125), Color.FromArgb(70, 70, 80), true, 0, "CORNELIUS (0)");

            for (int i = 0; i < 12; i++)
            {
                int row = i / 6;
                int col = i % 6;
                float cx = 145 + col * 68 + (row % 2 == 1 ? 18 : 0);
                float cy = 195 + row * 42;
                bool hasVoted = i < game.Election.PlayerVotes;
                Color shirt = Color.FromArgb(90 + (i * 29) % 140, 80 + (i * 53) % 140, 110 + (i * 37) % 120);
                DrawCharacterSprite(g, cx, cy, shirt, Color.FromArgb(90, 60, 40), i % 2 == 0, 1, null);
                if (hasVoted)
                {
                    g.DrawString("✓YOU!", _fontTiny, Brushes.Gold, cx - 14, cy - 28);
                }
            }

            DrawPixelPanel(g, 90, 260, 460, 26, Color.FromArgb(220, 30, 40, 55), Color.FromArgb(255, 220, 110));
            DrawCenteredText(g, game.Election.LastVoterAnnouncement, _fontSmall, Color.FromArgb(255, 240, 150), 266);

            foreach (var c in game.Election.Confetti)
            {
                using (var b = new SolidBrush(c.Tint))
                    g.FillRectangle(b, c.X, c.Y, c.Size, c.Size);
            }
        }

        private void DrawActiveWorldScene(Graphics g, GameStateEngine game, WorldController controller)
        {
            if (game.CurrentScene == GameScene.TownExterior)
                DrawTownExterior(g, game);
            else if (game.CurrentScene == GameScene.CityHallInterior)
                DrawCityHallInterior(g, game);
            else if (game.CurrentScene == GameScene.PoliceStationInterior)
                DrawPoliceStationInterior(g, game);
            else if (game.CurrentScene == GameScene.LibraryInterior)
                DrawLibraryInterior(g, game);

            foreach (var npc in game.Citizens)
            {
                if (npc.Scene != game.CurrentScene) continue;
                DrawCharacterSprite(g, npc.X, npc.Y, npc.PrimaryColor, npc.HairColor, npc.HasHat, npc.Facing, npc.Name);

                if (npc.HasTaxBillPending)
                {
                    float bounce = (float)Math.Sin(Environment.TickCount * 0.008) * 3f;
                    DrawPixelPanel(g, (int)npc.X - 22, (int)(npc.Y - 36 + bounce), 44, 13, Color.FromArgb(245, 195, 45), Color.White);
                    g.DrawString("TAX BILL $", _fontTiny, Brushes.Black, npc.X - 20, npc.Y - 35 + bounce);
                }
            }

            DrawCharacterSprite(g, game.PlayerX, game.PlayerY, Color.FromArgb(55, 165, 95), Color.FromArgb(95, 55, 30), true, game.PlayerFacing, game.MayorName + " (YOU)");

            DrawWeatherLightingAndParticles(g, game);

            foreach (var ft in game.FloatingTexts)
            {
                using (var b = new SolidBrush(ft.Color))
                {
                    g.DrawString(ft.Text, _fontBold, Brushes.Black, ft.X - 39, ft.Y + 1);
                    g.DrawString(ft.Text, _fontBold, b, ft.X - 40, ft.Y);
                }
            }

            DrawTopMayorHud(g, game);
            DrawOnScreenPhone(g, game, controller);

            if (game.ActiveOverlay == OverlayMode.MayorComputer)
                DrawMayorComputerOverlay(g, game, controller);
            else if (game.ActiveOverlay == OverlayMode.LibraryReader)
                DrawLibraryReaderOverlay(g, game);
            else if (game.ActiveOverlay == OverlayMode.BuildMenu)
                DrawBuildMenuOverlay(g, game);
            else if (game.ActiveOverlay == OverlayMode.PauseSettings)
                DrawPauseOverlay(g, game);
        }

        private void DrawTownExterior(Graphics g, GameStateEngine game)
        {
            Color grassColor = Color.FromArgb(96, 166, 88);
            if (game.CurrentWeather == WeatherType.AutumnBreeze) grassColor = Color.FromArgb(128, 152, 76);
            else if (game.CurrentWeather == WeatherType.GentleSnow) grassColor = Color.FromArgb(205, 222, 232);
            else if (game.CurrentWeather == WeatherType.StarlightGlow) grassColor = Color.FromArgb(58, 104, 78);

            g.Clear(grassColor);

            using (var riverBrush = new SolidBrush(Color.FromArgb(68, 145, 205)))
                g.FillRectangle(riverBrush, 604, 30, 36, 330);

            using (var roadBrush = new SolidBrush(Color.FromArgb(182, 172, 156)))
            {
                g.FillRectangle(roadBrush, 240, 60, 160, 270);
                g.FillRectangle(roadBrush, 30, 145, 580, 56);
                g.FillRectangle(roadBrush, 30, 225, 580, 26);
            }

            using (var rimBrush = new SolidBrush(Color.FromArgb(140, 145, 155)))
                g.FillEllipse(rimBrush, 294, 152, 52, 42);
            using (var waterBrush = new SolidBrush(Color.FromArgb(85, 185, 235)))
                g.FillEllipse(waterBrush, 300, 157, 40, 32);

            if (game.CurrentDayType == DayType.CozyFestivalDay)
            {
                for (int x = 60; x < 580; x += 45)
                {
                    using (var lantern = new SolidBrush(Color.FromArgb(255, 190, 65)))
                        g.FillEllipse(lantern, x, 140, 8, 8);
                }
            }

            DrawBuildingSprite(g, 255, 28, 130, 44, Color.FromArgb(190, 88, 72), Color.FromArgb(75, 52, 62), "★ CITY HALL (DOOR) ★", true);
            DrawBuildingSprite(g, 235, 314, 82, 42, Color.FromArgb(55, 90, 150), Color.FromArgb(38, 52, 85), "POLICE DEPT", false);
            DrawBuildingSprite(g, 330, 314, 88, 42, Color.FromArgb(135, 80, 145), Color.FromArgb(68, 42, 78), "PUBLIC LIBRARY", false);

            foreach (var plot in game.BuildingPlots)
            {
                Rectangle r = plot.WorldRect;
                if (plot.CurrentBuilding == BuildingType.EmptyLot)
                {
                    using (var pen = new Pen(Color.FromArgb(220, 195, 135), 1))
                    {
                        pen.DashStyle = DashStyle.Dash;
                        g.DrawRectangle(pen, r);
                    }
                    DrawPixelPanel(g, r.X + 8, r.Y + 14, r.Width - 16, 26, Color.FromArgb(135, 95, 55), Color.FromArgb(235, 200, 120));
                    g.DrawString("BUILD LOT", _fontTiny, Brushes.Gold, r.X + 12, r.Y + 17);
                    g.DrawString("[E] / [B]", _fontTiny, Brushes.White, r.X + 14, r.Y + 27);
                }
                else
                {
                    Color wall = GetBuildingColor(plot.CurrentBuilding);
                    Color roof = Color.FromArgb(Math.Max(20, wall.R - 45), Math.Max(20, wall.G - 45), Math.Max(20, wall.B - 45));
                    DrawBuildingSprite(g, r.X, r.Y, r.Width, r.Height, wall, roof, plot.CurrentBuilding + " L" + plot.Level, false);
                }
            }
        }

        private void DrawCityHallInterior(Graphics g, GameStateEngine game)
        {
            g.Clear(Color.FromArgb(52, 38, 34));
            using (var floorBrush = new SolidBrush(Color.FromArgb(158, 112, 74)))
                g.FillRectangle(floorBrush, 40, 40, 560, 290);

            using (var carpetBrush = new SolidBrush(Color.FromArgb(175, 48, 55)))
                g.FillRectangle(carpetBrush, 285, 95, 70, 235);

            using (var wallBrush = new SolidBrush(Color.FromArgb(95, 68, 52)))
                g.FillRectangle(wallBrush, 40, 32, 560, 42);

            DrawPixelPanel(g, 260, 74, 120, 36, Color.FromArgb(110, 65, 38), Color.FromArgb(245, 200, 95));
            DrawPixelPanel(g, 302, 66, 36, 24, Color.FromArgb(40, 195, 235), Color.White);
            g.DrawString("PC", _fontBold, Brushes.Black, 311, 72);
            DrawCenteredText(g, "▼ MAYOR'S DESK COMPUTER [Press E or C] ▼", _fontBold, Color.FromArgb(255, 240, 120), 48);

            DrawPixelPanel(g, 440, 82, 110, 36, Color.FromArgb(65, 110, 135), Color.FromArgb(170, 230, 255));
            g.DrawString("WEATHER & DAY TOWER", _fontTiny, Brushes.White, 446, 88);
            g.DrawString(game.CurrentWeather.ToString(), _fontTiny, Brushes.Gold, 446, 100);

            DrawPixelPanel(g, 90, 82, 110, 36, Color.FromArgb(135, 85, 65), Color.FromArgb(255, 215, 125));
            g.DrawString("TAX ASSESSOR DESK", _fontTiny, Brushes.White, 98, 88);
            g.DrawString("Bills Left: " + game.GetPendingTaxBillCount(), _fontTiny, Brushes.Gold, 98, 100);

            DrawPixelPanel(g, 275, 315, 90, 18, Color.FromArgb(90, 145, 85), Color.White);
            DrawCenteredText(g, "▼ EXIT TO TOWN SQUARE ▼", _fontTiny, Color.White, 319);
        }

        private void DrawPoliceStationInterior(Graphics g, GameStateEngine game)
        {
            g.Clear(Color.FromArgb(32, 42, 62));
            using (var floorBrush = new SolidBrush(Color.FromArgb(125, 140, 162)))
                g.FillRectangle(floorBrush, 40, 40, 560, 290);

            DrawPixelPanel(g, 160, 48, 320, 52, Color.FromArgb(35, 58, 95), Color.FromArgb(110, 205, 255));
            DrawCenteredText(g, "★ MAPLEWOOD POLICE PRECINCT DISPATCH BOARD [Press E] ★", _fontBold, Color.FromArgb(140, 225, 255), 55);
            DrawCenteredText(g, "Active Incidents: " + game.GetOpenIncidentCount() + "  |  Resolved: " + game.IncidentsResolvedTotal + "  |  Safety Rating: " + game.SafetyRating + "%", _fontSmall, Color.White, 74);

            DrawPixelPanel(g, 80, 160, 95, 48, Color.FromArgb(145, 95, 65), Color.FromArgb(255, 210, 140));
            g.DrawString("PRECINCT TEA", _fontTiny, Brushes.White, 92, 170);
            g.DrawString("& PASTRIES", _fontTiny, Brushes.Gold, 96, 184);

            DrawPixelPanel(g, 275, 315, 90, 18, Color.FromArgb(65, 115, 175), Color.White);
            DrawCenteredText(g, "▼ EXIT TO TOWN SQUARE ▼", _fontTiny, Color.White, 319);
        }

        private void DrawLibraryInterior(Graphics g, GameStateEngine game)
        {
            g.Clear(Color.FromArgb(48, 32, 46));
            using (var floorBrush = new SolidBrush(Color.FromArgb(148, 104, 78)))
                g.FillRectangle(floorBrush, 40, 40, 560, 290);

            using (var rugBrush = new SolidBrush(Color.FromArgb(135, 62, 95)))
                g.FillRectangle(rugBrush, 160, 135, 320, 130);

            for (int i = 0; i < 4; i++)
            {
                int bx = 75 + i * 125;
                DrawPixelPanel(g, bx, 48, 105, 52, Color.FromArgb(105, 62, 36), Color.FromArgb(235, 190, 110));
                Color[] spines = new[] { Color.Crimson, Color.RoyalBlue, Color.ForestGreen, Color.Goldenrod, Color.MediumOrchid };
                for (int s = 0; s < 12; s++)
                {
                    using (var sb = new SolidBrush(spines[s % spines.Length]))
                        g.FillRectangle(sb, bx + 6 + s * 8, 56, 6, 16);
                    using (var sb = new SolidBrush(spines[(s + 2) % spines.Length]))
                        g.FillRectangle(sb, bx + 6 + s * 8, 76, 6, 16);
                }
            }
            DrawCenteredText(g, "★ MAPLEWOOD PUBLIC LIBRARY — WALK UP & PRESS [E] TO READ BOOKS ★", _fontBold, Color.FromArgb(255, 235, 145), 106);

            DrawPixelPanel(g, 275, 315, 90, 18, Color.FromArgb(135, 80, 145), Color.White);
            DrawCenteredText(g, "▼ EXIT TO TOWN SQUARE ▼", _fontTiny, Color.White, 319);
        }

        private void DrawTopMayorHud(Graphics g, GameStateEngine game)
        {
            DrawPixelPanel(g, 4, 3, 632, 24, Color.FromArgb(230, 28, 34, 46), Color.FromArgb(235, 195, 95));

            int hours = (int)(game.DayClockMinutes / 60f) % 24;
            int mins = ((int)game.DayClockMinutes % 60) / 15 * 15;
            string ampm = hours >= 12 ? "PM" : "AM";
            int dispHour = hours > 12 ? hours - 12 : (hours == 0 ? 12 : hours);

            string leftHud = string.Format("DAY {0} ({1:00}:{2:00} {3}) | ${4} | Happy:{5}% | Safe:{6}% | Cozy:{7}",
                game.CurrentDay, dispHour, mins, ampm, game.CityTreasury, game.Happiness, game.SafetyRating, game.CozyScore);
            g.DrawString(leftHud, _fontSmall, Brushes.White, 10, 8);

            string rightHud = "[" + game.CurrentDayType + "] • [" + game.CurrentWeather + "]";
            g.DrawString(rightHud, _fontBold, Brushes.Gold, 415, 8);

            Rectangle compBtn = new Rectangle(580, 5, 52, 18);
            DrawPixelPanel(g, compBtn.X, compBtn.Y, compBtn.Width, compBtn.Height, Color.FromArgb(55, 125, 165), Color.White);
            g.DrawString("PC [C]", _fontTiny, Brushes.White, compBtn.X + 6, compBtn.Y + 4);
            ClickTargets.Add(new ClickRegion(compBtn, () =>
            {
                game.ActiveOverlay = game.ActiveOverlay == OverlayMode.MayorComputer ? OverlayMode.None : OverlayMode.MayorComputer;
                game.CompleteScheduleTask(0);
            }));
        }

        private void DrawOnScreenPhone(Graphics g, GameStateEngine game, WorldController controller)
        {
            if (!game.PhoneExpanded || game.ActiveOverlay != OverlayMode.None)
            {
                Rectangle miniRect = new Rectangle(520, 334, 114, 22);
                DrawPixelPanel(g, miniRect.X, miniRect.Y, miniRect.Width, miniRect.Height, Color.FromArgb(235, 32, 44, 68), Color.FromArgb(120, 215, 255));
                g.DrawString("MayorPhone [P]", _fontBold, Brushes.Cyan, miniRect.X + 8, miniRect.Y + 4);
                ClickTargets.Add(new ClickRegion(miniRect, () => { game.ActiveOverlay = OverlayMode.None; game.PhoneExpanded = true; }));
                return;
            }

            int px = 446;
            int py = 178;
            int pw = 188;
            int ph = 176;
            DrawPixelPanel(g, px, py, pw, ph, Color.FromArgb(242, 24, 32, 48), Color.FromArgb(110, 215, 255));

            DrawPixelPanel(g, px + 4, py + 4, pw - 8, 18, Color.FromArgb(42, 62, 92), Color.FromArgb(90, 150, 210));
            g.DrawString("CozyPhone 5G • AI", _fontTiny, Brushes.Cyan, px + 8, py + 7);
            Rectangle minBtn = new Rectangle(px + pw - 24, py + 5, 18, 14);
            g.DrawString("_", _fontBold, Brushes.White, minBtn.X + 4, minBtn.Y - 2);
            ClickTargets.Add(new ClickRegion(minBtn, () => game.PhoneExpanded = false));

            string[] appTabs = new[] { "Sched", "Sky", "Tax", "AI", "Pad" };
            for (int i = 0; i < appTabs.Length; i++)
            {
                Rectangle tabR = new Rectangle(px + 5 + i * 36, py + 24, 34, 15);
                bool active = game.PhoneAppIndex == i;
                DrawPixelPanel(g, tabR.X, tabR.Y, tabR.Width, tabR.Height, active ? Color.FromArgb(65, 155, 215) : Color.FromArgb(40, 50, 70), Color.FromArgb(130, 180, 220));
                g.DrawString(appTabs[i], _fontTiny, Brushes.White, tabR.X + 3, tabR.Y + 2);
                int cap = i;
                ClickTargets.Add(new ClickRegion(tabR, () => { game.PhoneAppIndex = cap; game.Audio.Play("click"); }));
            }

            int contentY = py + 43;
            if (game.PhoneAppIndex == 0)
            {
                g.DrawString("DAILY MAYOR SCHEDULE:", _fontTiny, Brushes.Gold, px + 8, contentY);
                for (int i = 0; i < game.DailySchedule.Count; i++)
                {
                    var s = game.DailySchedule[i];
                    string mark = s.Completed ? "[v]" : "[ ]";
                    Color c = s.Completed ? Color.FromArgb(120, 245, 140) : Color.FromArgb(230, 235, 245);
                    string line = mark + " " + s.Title;
                    if (line.Length > 27) line = line.Substring(0, 27);
                    using (var b = new SolidBrush(c))
                        g.DrawString(line, _fontTiny, b, px + 6, contentY + 13 + i * 15);
                }
            }
            else if (game.PhoneAppIndex == 1)
            {
                g.DrawString("TAP TO SET WEATHER / DAY:", _fontTiny, Brushes.Gold, px + 8, contentY);
                Rectangle wBtn = new Rectangle(px + 8, contentY + 15, pw - 16, 22);
                DrawPixelPanel(g, wBtn.X, wBtn.Y, wBtn.Width, wBtn.Height, Color.FromArgb(55, 115, 165), Color.White);
                g.DrawString("Weather: " + game.CurrentWeather + " [F]", _fontTiny, Brushes.White, wBtn.X + 6, wBtn.Y + 6);
                ClickTargets.Add(new ClickRegion(wBtn, () => game.SetWeather((WeatherType)(((int)game.CurrentWeather + 1) % 5))));

                Rectangle dBtn = new Rectangle(px + 8, contentY + 42, pw - 16, 22);
                DrawPixelPanel(g, dBtn.X, dBtn.Y, dBtn.Width, dBtn.Height, Color.FromArgb(165, 110, 45), Color.White);
                g.DrawString("Day: " + game.CurrentDayType + " [G]", _fontTiny, Brushes.White, dBtn.X + 6, dBtn.Y + 6);
                ClickTargets.Add(new ClickRegion(dBtn, () => game.SetDayType((DayType)(((int)game.CurrentDayType + 1) % 5))));

                Rectangle bBtn = new Rectangle(px + 8, contentY + 69, pw - 16, 22);
                DrawPixelPanel(g, bBtn.X, bBtn.Y, bBtn.Width, bBtn.Height, Color.FromArgb(75, 145, 85), Color.White);
                g.DrawString("Open City Builder [B]", _fontTiny, Brushes.White, bBtn.X + 6, bBtn.Y + 6);
                ClickTargets.Add(new ClickRegion(bBtn, () => game.ActiveOverlay = OverlayMode.BuildMenu));
            }
            else if (game.PhoneAppIndex == 2)
            {
                g.DrawString("UNPAID PROPERTY TAX BILLS:", _fontTiny, Brushes.Gold, px + 8, contentY);
                int yOff = contentY + 14;
                foreach (var b in game.TaxBills)
                {
                    string st = b.Delivered ? "PAID" : ("$" + b.AmountDue);
                    Color col = b.Delivered ? Color.LightGreen : Color.Orange;
                    using (var br = new SolidBrush(col))
                        g.DrawString("• " + b.OwnerName + ": " + st, _fontTiny, br, px + 6, yOff);
                    yOff += 14;
                }
                g.DrawString("Open Police Cases: " + game.GetOpenIncidentCount(), _fontTiny, Brushes.Cyan, px + 8, yOff + 4);
            }
            else if (game.PhoneAppIndex == 3)
            {
                g.DrawString("COZY-AI CHIEF OF STAFF:", _fontTiny, Brushes.Gold, px + 8, contentY);
                string lastMsg = game.AI.ChatHistory.Count > 0 ? game.AI.ChatHistory[game.AI.ChatHistory.Count - 1] : "Ready!";
                g.DrawString(lastMsg, _fontTiny, Brushes.White, new RectangleF(px + 6, contentY + 14, pw - 12, 66));

                Rectangle askBtn = new Rectangle(px + 8, contentY + 82, pw - 16, 20);
                DrawPixelPanel(g, askBtn.X, askBtn.Y, askBtn.Width, askBtn.Height, Color.FromArgb(115, 70, 155), Color.White);
                g.DrawString("Ask CozyAI Advice", _fontTiny, Brushes.White, askBtn.X + 26, askBtn.Y + 5);
                ClickTargets.Add(new ClickRegion(askBtn, () =>
                {
                    game.AI.AskAdvisor("General City Pulse", game.CurrentDay, game.CityTreasury, game.Happiness, game.SafetyRating, game.PropertyTaxRate, game.CurrentWeather, game.CurrentDayType, game.Employees, game.Incidents, game.TaxBills);
                    game.Audio.Play("blip");
                }));
            }
            else
            {
                g.DrawString("CONTROLLER & SAVE:", _fontTiny, Brushes.Gold, px + 8, contentY);
                g.DrawString("XInput: " + (game.Input.XInputAvailable ? "Active" : "N/A"), _fontTiny, Brushes.LightGreen, px + 8, contentY + 14);
                g.DrawString("DirectInput: " + (game.Input.DirectInputAvailable ? "Active" : "N/A"), _fontTiny, Brushes.LightGreen, px + 8, contentY + 27);
                g.DrawString("Now: " + game.Input.ActiveInputSource, _fontTiny, Brushes.White, new RectangleF(px + 8, contentY + 40, pw - 14, 26));

                Rectangle saveBtn = new Rectangle(px + 8, contentY + 72, pw - 16, 22);
                DrawPixelPanel(g, saveBtn.X, saveBtn.Y, saveBtn.Width, saveBtn.Height, Color.FromArgb(65, 145, 95), Color.White);
                g.DrawString("Save Mayoral Progress", _fontTiny, Brushes.White, saveBtn.X + 16, saveBtn.Y + 6);
                ClickTargets.Add(new ClickRegion(saveBtn, () => game.SaveGame()));
            }

            string notif = game.PhoneNotification ?? "";
            if (notif.Length > 29) notif = notif.Substring(0, 29) + "..";
            g.DrawString(notif, _fontTiny, Brushes.Khaki, px + 6, py + ph - 14);
        }

        private void DrawMayorComputerOverlay(Graphics g, GameStateEngine game, WorldController controller)
        {
            DrawPixelPanel(g, 28, 30, 584, 300, Color.FromArgb(246, 24, 34, 48), Color.FromArgb(110, 220, 255));
            DrawPixelPanel(g, 34, 36, 572, 24, Color.FromArgb(42, 78, 112), Color.FromArgb(140, 220, 255));
            g.DrawString("CITY HALL MAYOR'S COMPUTER — CozyOS v2.6 (Use Q/R or Left/Right or Click Tabs, [Esc] Exit)", _fontSmall, Brushes.White, 42, 42);

            string[] tabs = new[]
            {
                "1.Overview",
                "2.Employees",
                "3.Police Dept",
                "4.Tax & Sky",
                "5.City Build",
                "6.CozyAI"
            };

            for (int i = 0; i < tabs.Length; i++)
            {
                Rectangle tr = new Rectangle(36 + i * 94, 64, 90, 20);
                bool sel = game.ComputerTabIndex == i;
                DrawPixelPanel(g, tr.X, tr.Y, tr.Width, tr.Height, sel ? Color.FromArgb(65, 155, 205) : Color.FromArgb(45, 58, 78), sel ? Color.Gold : Color.Gray);
                g.DrawString(tabs[i], _fontSmall, sel ? Brushes.Yellow : Brushes.White, tr.X + 6, tr.Y + 4);
                int cap = i;
                ClickTargets.Add(new ClickRegion(tr, () => { game.ComputerTabIndex = cap; game.ComputerSelectedRow = 0; game.Audio.Play("click"); }));
            }

            int bodyX = 40;
            int bodyY = 90;

            if (game.ComputerTabIndex == 0)
            {
                g.DrawString(game.AI.LatestPolicyBriefing, _fontSmall, Brushes.LightCyan, new RectangleF(bodyX, bodyY, 550, 95));

                string[] actions = new[]
                {
                    "Adjust Property Tax Rate (Currently " + game.PropertyTaxRate + "%) — Click or Press [Enter]",
                    "Print Fresh Property Tax Bills to Mayor Bag (" + game.GetPendingTaxBillCount() + " Undelivered)",
                    "Conclude Day " + game.CurrentDay + " & Collect Daily Building Revenue -> Advance to Day " + (game.CurrentDay + 1),
                    "Log Off Computer & Return to City Hall Floor [Esc]"
                };

                for (int i = 0; i < actions.Length; i++)
                {
                    Rectangle ar = new Rectangle(bodyX, bodyY + 104 + i * 28, 550, 24);
                    bool rowSel = game.ComputerSelectedRow == i;
                    DrawPixelPanel(g, ar.X, ar.Y, ar.Width, ar.Height, rowSel ? Color.FromArgb(65, 130, 95) : Color.FromArgb(42, 52, 68), rowSel ? Color.Gold : Color.Silver);
                    g.DrawString((rowSel ? "► " : "  ") + actions[i], _fontBold, Brushes.White, ar.X + 8, ar.Y + 5);
                    int cap = i;
                    ClickTargets.Add(new ClickRegion(ar, () => { game.ComputerSelectedRow = cap; controller.ExecuteComputerAction(0, cap); }));
                }
            }
            else if (game.ComputerTabIndex == 1)
            {
                g.DrawString("MUNICIPAL EMPLOYEES DATABASE (Select Employee & Press [Enter]/Click to Award $75 Cozy Bonus):", _fontSmall, Brushes.Gold, bodyX, bodyY);
                for (int i = 0; i < game.Employees.Count; i++)
                {
                    var emp = game.Employees[i];
                    Rectangle er = new Rectangle(bodyX, bodyY + 18 + i * 32, 550, 29);
                    bool sel = game.ComputerSelectedRow == i;
                    DrawPixelPanel(g, er.X, er.Y, er.Width, er.Height, sel ? Color.FromArgb(60, 105, 145) : Color.FromArgb(38, 48, 64), sel ? Color.Gold : Color.Gray);
                    g.DrawString(emp.Name + " (" + emp.Role + ") | Morale: " + emp.Morale + "% | Eff: " + emp.Efficiency + "% | [+$75 Bonus]", _fontBold, Brushes.White, er.X + 6, er.Y + 3);
                    g.DrawString("  AI HR Note: " + emp.AIInsight, _fontTiny, Brushes.LightCyan, er.X + 6, er.Y + 16);
                    int cap = i;
                    ClickTargets.Add(new ClickRegion(er, () => { game.ComputerSelectedRow = cap; controller.ExecuteComputerAction(1, cap); }));
                }
            }
            else if (game.ComputerTabIndex == 2)
            {
                g.DrawString("MAPLEWOOD POLICE BLOTTER & INCIDENT DISPATCH (Select Incident to Dispatch/Resolve):", _fontSmall, Brushes.Gold, bodyX, bodyY);
                for (int i = 0; i < game.Incidents.Count; i++)
                {
                    var inc = game.Incidents[i];
                    Rectangle ir = new Rectangle(bodyX, bodyY + 20 + i * 62, 550, 56);
                    bool sel = game.ComputerSelectedRow == i;
                    Color bg = inc.Resolved ? Color.FromArgb(38, 68, 52) : (sel ? Color.FromArgb(85, 65, 115) : Color.FromArgb(52, 42, 68));
                    DrawPixelPanel(g, ir.X, ir.Y, ir.Width, ir.Height, bg, sel ? Color.Gold : Color.Silver);

                    string status = inc.Resolved ? ("[RESOLVED: " + inc.ResolutionSummary + "]") : ("[OPEN • Dispatch: " + inc.AssignedOfficer + "]");
                    g.DrawString("* " + inc.Title + " @ " + inc.Location + "  " + status, _fontSmall, inc.Resolved ? Brushes.LightGreen : Brushes.Yellow, ir.X + 8, ir.Y + 5);
                    g.DrawString(inc.Description, _fontTiny, Brushes.White, new RectangleF(ir.X + 8, ir.Y + 22, 534, 30));

                    int cap = i;
                    ClickTargets.Add(new ClickRegion(ir, () => { game.ComputerSelectedRow = cap; controller.ExecuteComputerAction(2, cap); }));
                }
            }
            else if (game.ComputerTabIndex == 3)
            {
                g.DrawString("PROCLAIM OFFICIAL DAY TYPE:", _fontBold, Brushes.Gold, bodyX, bodyY);
                for (int i = 0; i < 5; i++)
                {
                    DayType dt = (DayType)i;
                    Rectangle dr = new Rectangle(bodyX, bodyY + 18 + i * 24, 265, 21);
                    bool active = game.CurrentDayType == dt;
                    bool sel = game.ComputerSelectedRow == i;
                    DrawPixelPanel(g, dr.X, dr.Y, dr.Width, dr.Height, active ? Color.FromArgb(175, 120, 40) : Color.FromArgb(45, 55, 72), sel ? Color.Yellow : Color.Gray);
                    g.DrawString((active ? "★ " : "  ") + dt, _fontSmall, Brushes.White, dr.X + 6, dr.Y + 4);
                    int cap = i;
                    ClickTargets.Add(new ClickRegion(dr, () => { game.ComputerSelectedRow = cap; controller.ExecuteComputerAction(3, cap); }));
                }

                g.DrawString("ATMOSPHERIC WEATHER HARMONIZER:", _fontBold, Brushes.Cyan, bodyX + 285, bodyY);
                for (int i = 0; i < 5; i++)
                {
                    WeatherType wt = (WeatherType)i;
                    Rectangle wr = new Rectangle(bodyX + 285, bodyY + 18 + i * 24, 265, 21);
                    bool active = game.CurrentWeather == wt;
                    bool sel = game.ComputerSelectedRow == (i + 5);
                    DrawPixelPanel(g, wr.X, wr.Y, wr.Width, wr.Height, active ? Color.FromArgb(45, 135, 185) : Color.FromArgb(45, 55, 72), sel ? Color.Yellow : Color.Gray);
                    g.DrawString((active ? "☀ " : "  ") + wt, _fontSmall, Brushes.White, wr.X + 6, wr.Y + 4);
                    int cap = i + 5;
                    ClickTargets.Add(new ClickRegion(wr, () => { game.ComputerSelectedRow = cap; controller.ExecuteComputerAction(3, cap); }));
                }

                g.DrawString("TODAY'S PROPERTY TAX BILL ROLL (Walk up to Citizens in Town Square & Press [E] to Deliver):", _fontBold, Brushes.LightGreen, bodyX, bodyY + 146);
                for (int i = 0; i < game.TaxBills.Count; i++)
                {
                    var b = game.TaxBills[i];
                    string st = b.Delivered ? "DELIVERED & PAID" : ("PENDING DELIVERY ($" + b.AmountDue + ")");
                    g.DrawString("• " + b.PropertyName + " (" + b.OwnerName + "): " + st, _fontTiny, b.Delivered ? Brushes.LightGreen : Brushes.Khaki, bodyX + (i % 2) * 280, bodyY + 164 + (i / 2) * 16);
                }
            }
            else if (game.ComputerTabIndex == 4)
            {
                g.DrawString("CITY ZONING & CONSTRUCTION BLUEPRINT (Select a Lot to Build or Upgrade):", _fontBold, Brushes.Gold, bodyX, bodyY);
                for (int i = 0; i < game.BuildingPlots.Count; i++)
                {
                    var p = game.BuildingPlots[i];
                    Rectangle pr = new Rectangle(bodyX, bodyY + 20 + i * 32, 550, 28);
                    bool sel = game.ComputerSelectedRow == i;
                    DrawPixelPanel(g, pr.X, pr.Y, pr.Width, pr.Height, sel ? Color.FromArgb(65, 135, 95) : Color.FromArgb(42, 54, 70), sel ? Color.Gold : Color.Silver);
                    string desc = p.CurrentBuilding == BuildingType.EmptyLot
                        ? "EMPTY LOT — Click or Press [Enter] to Construct New Building!"
                        : (p.CurrentBuilding + " (Level " + p.Level + ") | Daily Tax Revenue: +$" + p.DailyTaxRevenue + "/day | Cozy: +" + p.CozyBonus);
                    g.DrawString(p.PlotName + ": " + desc, _fontSmall, Brushes.White, pr.X + 8, pr.Y + 7);
                    int cap = i;
                    ClickTargets.Add(new ClickRegion(pr, () => { game.ComputerSelectedRow = cap; controller.ExecuteComputerAction(4, cap); }));
                }
            }
            else if (game.ComputerTabIndex == 5)
            {
                g.DrawString("COZY-AI EXECUTIVE INTELLIGENCE TERMINAL:", _fontBold, Brushes.Cyan, bodyX, bodyY);
                DrawPixelPanel(g, bodyX, bodyY + 18, 550, 115, Color.FromArgb(18, 26, 38), Color.FromArgb(80, 175, 220));

                int startIdx = Math.Max(0, game.AI.ChatHistory.Count - 5);
                int yLine = bodyY + 24;
                for (int i = startIdx; i < game.AI.ChatHistory.Count; i++)
                {
                    g.DrawString(game.AI.ChatHistory[i], _fontTiny, i % 2 == 0 ? Brushes.LightCyan : Brushes.Khaki, new RectangleF(bodyX + 6, yLine, 538, 22));
                    yLine += 21;
                }

                string[] prompts = new[]
                {
                    "Ask AI: Property Tax & Budget Forecast",
                    "Ask AI: Police Incidents & Safety Analysis",
                    "Ask AI: Employee Morale & HR Recommendations",
                    "Ask AI: Weather & Day-Type Synergy Guide"
                };
                for (int i = 0; i < prompts.Length; i++)
                {
                    Rectangle qr = new Rectangle(bodyX + (i % 2) * 278, bodyY + 142 + (i / 2) * 32, 268, 26);
                    bool sel = game.ComputerSelectedRow == i;
                    DrawPixelPanel(g, qr.X, qr.Y, qr.Width, qr.Height, sel ? Color.FromArgb(95, 70, 155) : Color.FromArgb(45, 52, 74), sel ? Color.Gold : Color.Silver);
                    g.DrawString(prompts[i], _fontTiny, Brushes.White, qr.X + 8, qr.Y + 8);
                    int cap = i;
                    ClickTargets.Add(new ClickRegion(qr, () => { game.ComputerSelectedRow = cap; controller.ExecuteComputerAction(5, cap); }));
                }
            }
        }

        private void DrawLibraryReaderOverlay(Graphics g, GameStateEngine game)
        {
            DrawPixelPanel(g, 34, 28, 572, 300, Color.FromArgb(245, 48, 34, 28), Color.FromArgb(235, 190, 110));
            g.DrawString("MAPLEWOOD PUBLIC LIBRARY — COZY BOOK READER (Up/Down Select Book, Left/Right/Enter Turn Page, Esc Exit)", _fontTiny, Brushes.Gold, 44, 35);

            for (int i = 0; i < game.Library.Books.Count; i++)
            {
                var b = game.Library.Books[i];
                Rectangle br = new Rectangle(44, 54 + i * 32, 215, 28);
                bool sel = game.Library.SelectedBookIndex == i;
                DrawPixelPanel(g, br.X, br.Y, br.Width, br.Height, sel ? b.CoverColor : Color.FromArgb(65, 48, 40), sel ? Color.Gold : Color.RosyBrown);
                string title = b.Title.Length > 30 ? b.Title.Substring(0, 30) + ".." : b.Title;
                g.DrawString((b.HasBeenRead ? "✓ " : "") + title, _fontTiny, Brushes.White, br.X + 5, br.Y + 4);
                g.DrawString("by " + b.Author, _fontTiny, Brushes.Bisque, br.X + 5, br.Y + 15);
                int cap = i;
                ClickTargets.Add(new ClickRegion(br, () => { game.Library.SelectedBookIndex = cap; game.Library.CurrentPageIndex = 0; game.Audio.Play("page"); }));
            }

            var activeBook = game.Library.Books[game.Library.SelectedBookIndex];
            DrawPixelPanel(g, 270, 54, 324, 264, Color.FromArgb(245, 235, 210), Color.FromArgb(145, 95, 55));
            g.DrawString(activeBook.Title, _fontBold, Brushes.SaddleBrown, new RectangleF(282, 64, 300, 24));
            g.DrawString("Author: " + activeBook.Author + "  |  Page " + (game.Library.CurrentPageIndex + 1) + " of " + activeBook.Pages.Count, _fontTiny, Brushes.DimGray, 282, 86);

            string pageText = activeBook.Pages[Math.Min(game.Library.CurrentPageIndex, activeBook.Pages.Count - 1)];
            using (var inkBrush = new SolidBrush(Color.FromArgb(45, 32, 24)))
            {
                g.DrawString(pageText, _fontSmall, inkBrush, new RectangleF(282, 108, 298, 175));
            }

            Rectangle nextBtn = new Rectangle(450, 290, 132, 22);
            DrawPixelPanel(g, nextBtn.X, nextBtn.Y, nextBtn.Width, nextBtn.Height, Color.FromArgb(135, 85, 50), Color.Gold);
            g.DrawString("Turn Page [Enter/►]", _fontTiny, Brushes.White, nextBtn.X + 10, nextBtn.Y + 6);
            ClickTargets.Add(new ClickRegion(nextBtn, () =>
            {
                game.Library.CurrentPageIndex = (game.Library.CurrentPageIndex + 1) % activeBook.Pages.Count;
                game.Audio.Play("page");
            }));
        }

        private void DrawBuildMenuOverlay(Graphics g, GameStateEngine game)
        {
            DrawPixelPanel(g, 50, 34, 540, 292, Color.FromArgb(245, 30, 44, 38), Color.FromArgb(245, 210, 110));
            var plot = game.BuildingPlots[game.SelectedPlotIndex];
            string discountNote = game.CurrentDayType == DayType.CivicConstructionDay ? " (CIVIC CONSTRUCTION DAY: 20% OFF!)" : "";
            g.DrawString("CITY BUILDER BLUEPRINT — " + plot.PlotName + discountNote, _fontBold, Brushes.Gold, 62, 42);
            g.DrawString("Current Status: " + plot.CurrentBuilding + " (Lv." + plot.Level + ") | Treasury: $" + game.CityTreasury + " | Left/Right Change Plot, Up/Down Choose Building", _fontTiny, Brushes.LightCyan, 62, 58);

            for (int i = 0; i < 8; i++)
            {
                BuildingType bt = (BuildingType)(i + 1);
                int cost = game.GetBuildingBaseCost(bt);
                if (game.CurrentDayType == DayType.CivicConstructionDay) cost = (int)(cost * 0.8f);

                Rectangle br = new Rectangle(62 + (i % 2) * 258, 78 + (i / 2) * 52, 248, 44);
                bool sel = game.SelectedBuildOption == i;
                DrawPixelPanel(g, br.X, br.Y, br.Width, br.Height, sel ? Color.FromArgb(65, 140, 95) : Color.FromArgb(42, 60, 52), sel ? Color.Gold : Color.Silver);
                g.DrawString(bt + " — Cost: $" + cost, _fontBold, Brushes.White, br.X + 8, br.Y + 6);
                g.DrawString("Daily Tax Revenue: +$" + (45 + (i + 1) * 15) + "/day | Cozy: +" + (12 + (i + 1) * 4), _fontTiny, Brushes.Khaki, br.X + 8, br.Y + 23);

                int cap = i;
                ClickTargets.Add(new ClickRegion(br, () =>
                {
                    game.SelectedBuildOption = cap;
                    game.ConstructOrUpgradePlot(plot, (BuildingType)(cap + 1));
                }));
            }

            DrawCenteredText(g, "Press [Enter] / (A) or Click to Build/Upgrade • [Q]/[R] Switch Lot • [Esc] / (B) Close", _fontTiny, Color.White, 306);
        }

        private void DrawPauseOverlay(Graphics g, GameStateEngine game)
        {
            DrawPixelPanel(g, 130, 68, 380, 220, Color.FromArgb(245, 28, 34, 48), Color.Gold);
            DrawCenteredText(g, "PAUSED — MAYORAL SETTINGS & INPUT DIAGNOSTICS", _fontBold, Color.Gold, 78);
            DrawCenteredText(g, game.Input.GetControllerStatusSummary(), _fontTiny, Color.LightCyan, 102);
            DrawCenteredText(g, "✓ XInput (xinput1_4 / Rumble) & DirectInput8 (dinput8 / WinMM) Active", _fontTiny, Color.LightGreen, 118);

            Rectangle saveBtn = new Rectangle(180, 142, 280, 26);
            DrawPixelPanel(g, saveBtn.X, saveBtn.Y, saveBtn.Width, saveBtn.Height, Color.FromArgb(65, 145, 95), Color.White);
            DrawCenteredText(g, "Save Game & Resume [Enter / A]", _fontBold, Color.White, 148);
            ClickTargets.Add(new ClickRegion(saveBtn, () => { game.SaveGame(); game.ActiveOverlay = OverlayMode.None; }));

            Rectangle rumbleBtn = new Rectangle(180, 176, 280, 26);
            DrawPixelPanel(g, rumbleBtn.X, rumbleBtn.Y, rumbleBtn.Width, rumbleBtn.Height, Color.FromArgb(85, 110, 165), Color.White);
            DrawCenteredText(g, "Test Controller Vibration & Audio Chime", _fontBold, Color.White, 182);
            ClickTargets.Add(new ClickRegion(rumbleBtn, () => { game.Input.TriggerRumble(40000, 40000, 0.35f); game.Audio.Play("fanfare"); }));

            Rectangle muteBtn = new Rectangle(180, 210, 280, 26);
            DrawPixelPanel(g, muteBtn.X, muteBtn.Y, muteBtn.Width, muteBtn.Height, Color.FromArgb(125, 85, 135), Color.White);
            DrawCenteredText(g, "Audio Synthesizer: " + (game.Audio.Muted ? "MUTED" : "ENABLED"), _fontBold, Color.White, 216);
            ClickTargets.Add(new ClickRegion(muteBtn, () => { game.Audio.Muted = !game.Audio.Muted; }));

            DrawCenteredText(g, "Press [Esc] or (B) to Return to Maplewood Valley", _fontTiny, Color.Silver, 258);
        }

        private void DrawDialogueBox(Graphics g, GameStateEngine game)
        {
            var dlg = game.ActiveDialogue;
            if (dlg == null) return;

            bool phoneBeside = game.PhoneExpanded && game.ActiveOverlay == OverlayMode.None && game.CurrentScene != GameScene.ElectionCutscene;
            int dx = 12;
            int dy = 216;
            int dw = phoneBeside ? 428 : 612;
            int dh = 138;
            DrawPixelPanel(g, dx, dy, dw, dh, Color.FromArgb(245, 28, 24, 36), Color.FromArgb(255, 215, 105));

            DrawPixelPanel(g, dx + 8, dy + 10, 48, 52, dlg.PortraitColor, Color.White);
            DrawCharacterSprite(g, dx + 32, dy + 44, dlg.PortraitColor, Color.FromArgb(90, 55, 35), true, 0, null);

            g.DrawString(dlg.Speaker, _fontBold, Brushes.Gold, dx + 62, dy + 8);
            g.DrawString(dlg.Title, _fontTiny, Brushes.LightCyan, dx + 195, dy + 10);

            int len = Math.Min(dlg.Text.Length, (int)game.DialogueCharIndex);
            string visibleText = dlg.Text.Substring(0, len);
            g.DrawString(visibleText, _fontTiny, Brushes.White, new RectangleF(dx + 62, dy + 24, dw - 70, 46));

            if (len >= dlg.Text.Length && dlg.Choices.Count > 0)
            {
                for (int i = 0; i < dlg.Choices.Count; i++)
                {
                    Rectangle cr = new Rectangle(dx + 62, dy + 72 + i * 19, dw - 70, 17);
                    bool sel = game.SelectedChoiceIndex == i;
                    DrawPixelPanel(g, cr.X, cr.Y, cr.Width, cr.Height, sel ? Color.FromArgb(75, 145, 105) : Color.FromArgb(48, 44, 62), sel ? Color.Yellow : Color.Gray);
                    g.DrawString((sel ? "► " : "  ") + dlg.Choices[i].Text, _fontTiny, Brushes.White, cr.X + 4, cr.Y + 3);
                    int cap = i;
                    ClickTargets.Add(new ClickRegion(cr, () =>
                    {
                        var ch = dlg.Choices[cap];
                        game.AdvanceDialogue();
                        if (ch.OnSelect != null) ch.OnSelect();
                    }));
                }
            }
            else
            {
                Rectangle contRect = new Rectangle(dx + dw - 135, dy + dh - 20, 128, 16);
                g.DrawString("Press [E]/Enter/(A) ►", _fontTiny, Brushes.Gold, contRect.X, contRect.Y);
                ClickTargets.Add(new ClickRegion(new Rectangle(dx, dy, dw, dh), () =>
                {
                    if (game.DialogueCharIndex < dlg.Text.Length) game.DialogueCharIndex = dlg.Text.Length;
                    else game.AdvanceDialogue();
                }));
            }
        }

        private void DrawWeatherLightingAndParticles(Graphics g, GameStateEngine game)
        {
            if (game.CurrentWeather == WeatherType.CozyRain)
            {
                using (var tint = new SolidBrush(Color.FromArgb(35, 30, 70, 120)))
                    g.FillRectangle(tint, 0, 0, VirtualWidth, VirtualHeight);
            }
            else if (game.CurrentWeather == WeatherType.StarlightGlow)
            {
                using (var tint = new SolidBrush(Color.FromArgb(55, 20, 18, 55)))
                    g.FillRectangle(tint, 0, 0, VirtualWidth, VirtualHeight);
            }
            else if (game.CurrentWeather == WeatherType.AutumnBreeze)
            {
                using (var tint = new SolidBrush(Color.FromArgb(22, 215, 120, 30)))
                    g.FillRectangle(tint, 0, 0, VirtualWidth, VirtualHeight);
            }

            DrawWeatherParticles(g, game);
        }

        private void DrawWeatherParticles(Graphics g, GameStateEngine game)
        {
            foreach (var p in game.WeatherParticles)
            {
                using (var b = new SolidBrush(p.Tint))
                {
                    if (game.CurrentWeather == WeatherType.CozyRain)
                        g.FillRectangle(b, p.X, p.Y, 1, p.Size);
                    else
                        g.FillRectangle(b, p.X, p.Y, p.Size, p.Size);
                }
            }
        }

        private void DrawBuildingSprite(Graphics g, int x, int y, int w, int h, Color wallColor, Color roofColor, string label, bool hasClockTower)
        {
            using (var sh = new SolidBrush(Color.FromArgb(65, 0, 0, 0)))
                g.FillRectangle(sh, x + 3, y + 3, w, h);

            using (var wb = new SolidBrush(wallColor))
                g.FillRectangle(wb, x, y + 12, w, h - 12);
            g.DrawRectangle(Pens.Black, x, y + 12, w, h - 12);

            using (var rb = new SolidBrush(roofColor))
                g.FillRectangle(rb, x - 2, y, w + 4, 15);
            g.DrawRectangle(Pens.Black, x - 2, y, w + 4, 15);

            if (hasClockTower)
            {
                using (var rb = new SolidBrush(roofColor))
                    g.FillRectangle(rb, x + w / 2 - 14, y - 14, 28, 16);
                using (var cb = new SolidBrush(Color.FromArgb(255, 240, 185)))
                    g.FillEllipse(cb, x + w / 2 - 6, y - 11, 12, 12);
            }

            using (var winBrush = new SolidBrush(Color.FromArgb(255, 235, 145)))
            {
                g.FillRectangle(winBrush, x + 8, y + 20, 10, 10);
                g.FillRectangle(winBrush, x + w - 18, y + 20, 10, 10);
            }

            using (var db = new SolidBrush(Color.FromArgb(95, 55, 32)))
                g.FillRectangle(db, x + w / 2 - 7, y + h - 16, 14, 16);

            if (!string.IsNullOrEmpty(label))
            {
                SizeF sz = g.MeasureString(label, _fontTiny);
                int lx = x + (int)((w - sz.Width) / 2);
                using (var sb = new SolidBrush(Color.FromArgb(190, 20, 20, 28)))
                    g.FillRectangle(sb, lx - 2, y + 2, sz.Width + 4, 11);
                g.DrawString(label, _fontTiny, Brushes.Gold, lx, y + 2);
            }
        }

        private void DrawCharacterSprite(Graphics g, float x, float y, Color clothesColor, Color hairColor, bool hasHat, int facing, string label)
        {
            int ix = (int)x;
            int iy = (int)y;

            using (var sh = new SolidBrush(Color.FromArgb(75, 0, 0, 0)))
                g.FillEllipse(sh, ix - 7, iy + 5, 14, 5);

            g.FillRectangle(Brushes.Black, ix - 4, iy + 2, 3, 5);
            g.FillRectangle(Brushes.Black, ix + 1, iy + 2, 3, 5);

            using (var cb = new SolidBrush(clothesColor))
                g.FillRectangle(cb, ix - 6, iy - 7, 12, 10);
            g.DrawRectangle(Pens.Black, ix - 6, iy - 7, 12, 10);

            using (var skin = new SolidBrush(Color.FromArgb(255, 218, 185)))
                g.FillRectangle(skin, ix - 5, iy - 16, 10, 9);
            g.DrawRectangle(Pens.Black, ix - 5, iy - 16, 10, 9);

            using (var hb = new SolidBrush(hairColor))
                g.FillRectangle(hb, ix - 5, iy - 17, 10, 3);

            if (hasHat)
            {
                using (var hatB = new SolidBrush(clothesColor))
                    g.FillRectangle(hatB, ix - 7, iy - 19, 14, 3);
            }

            if (facing != 1)
            {
                g.FillRectangle(Brushes.Black, ix - 3, iy - 13, 2, 2);
                g.FillRectangle(Brushes.Black, ix + 1, iy - 13, 2, 2);
            }

            if (!string.IsNullOrEmpty(label))
            {
                SizeF sz = g.MeasureString(label, _fontTiny);
                g.DrawString(label, _fontTiny, Brushes.Black, ix - sz.Width / 2 + 1, iy - 28 + 1);
                g.DrawString(label, _fontTiny, Brushes.White, ix - sz.Width / 2, iy - 28);
            }
        }

        private Color GetBuildingColor(BuildingType bt)
        {
            switch (bt)
            {
                case BuildingType.CozyBakery: return Color.FromArgb(215, 135, 85);
                case BuildingType.MapleCoffeeShop: return Color.FromArgb(145, 95, 68);
                case BuildingType.CommunityPark: return Color.FromArgb(75, 165, 95);
                case BuildingType.SolarGarden: return Color.FromArgb(65, 140, 185);
                case BuildingType.TownClinic: return Color.FromArgb(215, 225, 235);
                case BuildingType.ArtisanBookstore: return Color.FromArgb(145, 90, 155);
                case BuildingType.CozyApartments: return Color.FromArgb(185, 115, 95);
                case BuildingType.RetroArcade: return Color.FromArgb(95, 75, 175);
                default: return Color.Gray;
            }
        }

        private void DrawPixelPanel(Graphics g, int x, int y, int w, int h, Color bg, Color border)
        {
            using (var b = new SolidBrush(bg))
                g.FillRectangle(b, x, y, w, h);
            using (var p = new Pen(border, 1))
                g.DrawRectangle(p, x, y, w, h);
        }

        private void DrawCenteredText(Graphics g, string text, Font font, Color color, int y)
        {
            SizeF sz = g.MeasureString(text, font);
            float x = (VirtualWidth - sz.Width) / 2f;
            g.DrawString(text, font, Brushes.Black, x + 1, y + 1);
            using (var b = new SolidBrush(color))
                g.DrawString(text, font, b, x, y);
        }
    }
}
