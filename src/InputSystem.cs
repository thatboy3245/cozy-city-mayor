using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CozyCityMayor
{
    public class InputSystem
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct XINPUT_GAMEPAD
        {
            public ushort wButtons;
            public byte bLeftTrigger;
            public byte bRightTrigger;
            public short sThumbLX;
            public short sThumbLY;
            public short sThumbRX;
            public short sThumbRY;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct XINPUT_STATE
        {
            public uint dwPacketNumber;
            public XINPUT_GAMEPAD Gamepad;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct XINPUT_VIBRATION
        {
            public ushort wLeftMotorSpeed;
            public ushort wRightMotorSpeed;
        }

        private const ushort XINPUT_GAMEPAD_DPAD_UP = 0x0001;
        private const ushort XINPUT_GAMEPAD_DPAD_DOWN = 0x0002;
        private const ushort XINPUT_GAMEPAD_DPAD_LEFT = 0x0004;
        private const ushort XINPUT_GAMEPAD_DPAD_RIGHT = 0x0008;
        private const ushort XINPUT_GAMEPAD_START = 0x0010;
        private const ushort XINPUT_GAMEPAD_BACK = 0x0020;
        private const ushort XINPUT_GAMEPAD_LEFT_SHOULDER = 0x0100;
        private const ushort XINPUT_GAMEPAD_RIGHT_SHOULDER = 0x0200;
        private const ushort XINPUT_GAMEPAD_A = 0x1000;
        private const ushort XINPUT_GAMEPAD_B = 0x2000;
        private const ushort XINPUT_GAMEPAD_X = 0x4000;
        private const ushort XINPUT_GAMEPAD_Y = 0x8000;

        [DllImport("xinput1_4.dll", EntryPoint = "XInputGetState")]
        private static extern uint XInputGetState_14(uint dwUserIndex, ref XINPUT_STATE pState);

        [DllImport("xinput1_4.dll", EntryPoint = "XInputSetState")]
        private static extern uint XInputSetState_14(uint dwUserIndex, ref XINPUT_VIBRATION pVibration);

        [DllImport("xinput9_1_0.dll", EntryPoint = "XInputGetState")]
        private static extern uint XInputGetState_910(uint dwUserIndex, ref XINPUT_STATE pState);

        [DllImport("xinput9_1_0.dll", EntryPoint = "XInputSetState")]
        private static extern uint XInputSetState_910(uint dwUserIndex, ref XINPUT_VIBRATION pVibration);

        [DllImport("dinput8.dll", EntryPoint = "DirectInput8Create")]
        private static extern int DirectInput8Create(
            IntPtr hinst,
            uint dwVersion,
            ref Guid riidltf,
            out IntPtr ppvOut,
            IntPtr punkOuter);

        [StructLayout(LayoutKind.Sequential)]
        public struct JOYINFOEX
        {
            public uint dwSize;
            public uint dwFlags;
            public uint dwXpos;
            public uint dwYpos;
            public uint dwZpos;
            public uint dwRpos;
            public uint dwUpos;
            public uint dwVpos;
            public uint dwButtons;
            public uint dwButtonNumber;
            public uint dwPOV;
            public uint dwReserved1;
            public uint dwReserved2;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct JOYCAPSW
        {
            public ushort wMid;
            public ushort wPid;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szPname;
            public uint wXmin;
            public uint wXmax;
            public uint wYmin;
            public uint wYmax;
            public uint wZmin;
            public uint wZmax;
            public uint wNumButtons;
            public uint wPeriodMin;
            public uint wPeriodMax;
            public uint wRmin;
            public uint wRmax;
            public uint wUmin;
            public uint wUmax;
            public uint wVmin;
            public uint wVmax;
            public uint wCaps;
            public uint wMaxAxes;
            public uint wNumAxes;
            public uint wMaxButtons;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szRegKey;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szOEMVxD;
        }

        private const uint JOY_RETURNALL = 0x000000FF;

        [DllImport("winmm.dll")]
        private static extern uint joyGetNumDevs();

        [DllImport("winmm.dll")]
        private static extern uint joyGetPosEx(uint uJoyID, ref JOYINFOEX pji);

        [DllImport("winmm.dll", CharSet = CharSet.Unicode)]
        private static extern uint joyGetDevCapsW(UIntPtr uJoyID, ref JOYCAPSW pjc, uint cbjc);

        private readonly HashSet<Keys> _keysDown = new HashSet<Keys>();
        private readonly HashSet<Keys> _keysPressedThisFrame = new HashSet<Keys>();

        public bool XInputAvailable { get; private set; }
        public bool DirectInputAvailable { get; private set; }
        public bool XInputConnected { get; private set; }
        public bool DirectInputConnected { get; private set; }
        public string ActiveInputSource { get; private set; }
        public string DirectInputDeviceName { get; private set; }
        public int ActiveXInputSlot { get; private set; }
        public int ActiveDirectInputSlot { get; private set; }

        private ushort _prevXButtons;
        private uint _prevDButtons;
        private int _prevDpadDir = -1;
        private float _rumbleTimer;
        private bool _useXInput14 = true;

        public float MoveX { get; private set; }
        public float MoveY { get; private set; }

        public bool ConfirmPressed { get; private set; }
        public bool CancelPressed { get; private set; }
        public bool PhoneTogglePressed { get; private set; }
        public bool ComputerQuickPressed { get; private set; }
        public bool WeatherCyclePressed { get; private set; }
        public bool DayTypeCyclePressed { get; private set; }
        public bool TabPrevPressed { get; private set; }
        public bool TabNextPressed { get; private set; }
        public bool UpPressed { get; private set; }
        public bool DownPressed { get; private set; }
        public bool LeftPressed { get; private set; }
        public bool RightPressed { get; private set; }
        public bool BuildMenuPressed { get; private set; }
        public bool PausePressed { get; private set; }

        public InputSystem()
        {
            ActiveInputSource = "Keyboard / Mouse (XInput & DirectInput Ready)";
            DirectInputDeviceName = "DirectInput8 Standard HID";
            ActiveXInputSlot = -1;
            ActiveDirectInputSlot = -1;
            InitializeSubsystems();
        }

        private void InitializeSubsystems()
        {
            try
            {
                XINPUT_STATE st = new XINPUT_STATE();
                XInputGetState_14(0, ref st);
                XInputAvailable = true;
                _useXInput14 = true;
            }
            catch
            {
                try
                {
                    XINPUT_STATE st = new XINPUT_STATE();
                    XInputGetState_910(0, ref st);
                    XInputAvailable = true;
                    _useXInput14 = false;
                }
                catch
                {
                    XInputAvailable = false;
                }
            }

            try
            {
                Guid iidDirectInput8W = new Guid("BF798031-483A-4DA2-AA99-5D64ED369700");
                IntPtr pDI = IntPtr.Zero;
                int hr = DirectInput8Create(
                    Marshal.GetHINSTANCE(typeof(InputSystem).Module),
                    0x0800,
                    ref iidDirectInput8W,
                    out pDI,
                    IntPtr.Zero);
                if (hr >= 0 && pDI != IntPtr.Zero)
                {
                    DirectInputAvailable = true;
                    Marshal.Release(pDI);
                }
                else
                {
                    DirectInputAvailable = joyGetNumDevs() > 0;
                }
            }
            catch
            {
                try
                {
                    DirectInputAvailable = joyGetNumDevs() > 0;
                }
                catch
                {
                    DirectInputAvailable = false;
                }
            }
        }

        public void OnKeyDown(Keys k)
        {
            if (!_keysDown.Contains(k))
            {
                _keysPressedThisFrame.Add(k);
            }
            _keysDown.Add(k);
            ActiveInputSource = "Keyboard & Mouse";
        }

        public void OnKeyUp(Keys k)
        {
            _keysDown.Remove(k);
        }

        public bool IsKeyDown(Keys k) { return _keysDown.Contains(k); }
        public bool WasKeyPressed(Keys k) { return _keysPressedThisFrame.Contains(k); }

        public void TriggerRumble(ushort leftMotor, ushort rightMotor, float durationSeconds)
        {
            if (!XInputAvailable || ActiveXInputSlot < 0) return;
            try
            {
                XINPUT_VIBRATION vib = new XINPUT_VIBRATION
                {
                    wLeftMotorSpeed = leftMotor,
                    wRightMotorSpeed = rightMotor
                };
                if (_useXInput14)
                    XInputSetState_14((uint)ActiveXInputSlot, ref vib);
                else
                    XInputSetState_910((uint)ActiveXInputSlot, ref vib);
                _rumbleTimer = durationSeconds;
            }
            catch { }
        }

        public void Update(float dt)
        {
            MoveX = 0f;
            MoveY = 0f;

            ConfirmPressed = WasKeyPressed(Keys.E) || WasKeyPressed(Keys.Enter) || WasKeyPressed(Keys.Space) || WasKeyPressed(Keys.Z);
            CancelPressed = WasKeyPressed(Keys.Escape) || WasKeyPressed(Keys.Back) || WasKeyPressed(Keys.X);
            PhoneTogglePressed = WasKeyPressed(Keys.P) || WasKeyPressed(Keys.Tab);
            ComputerQuickPressed = WasKeyPressed(Keys.C);
            WeatherCyclePressed = WasKeyPressed(Keys.F) || WasKeyPressed(Keys.D2);
            DayTypeCyclePressed = WasKeyPressed(Keys.G) || WasKeyPressed(Keys.D1);
            BuildMenuPressed = WasKeyPressed(Keys.B);
            TabPrevPressed = WasKeyPressed(Keys.Q) || WasKeyPressed(Keys.PageUp);
            TabNextPressed = WasKeyPressed(Keys.R) || WasKeyPressed(Keys.PageDown);
            PausePressed = WasKeyPressed(Keys.Escape) || WasKeyPressed(Keys.F1);

            UpPressed = WasKeyPressed(Keys.W) || WasKeyPressed(Keys.Up);
            DownPressed = WasKeyPressed(Keys.S) || WasKeyPressed(Keys.Down);
            LeftPressed = WasKeyPressed(Keys.A) || WasKeyPressed(Keys.Left);
            RightPressed = WasKeyPressed(Keys.D) || WasKeyPressed(Keys.Right);

            if (IsKeyDown(Keys.Left) || IsKeyDown(Keys.A)) MoveX -= 1f;
            if (IsKeyDown(Keys.Right) || IsKeyDown(Keys.D)) MoveX += 1f;
            if (IsKeyDown(Keys.Up) || IsKeyDown(Keys.W)) MoveY -= 1f;
            if (IsKeyDown(Keys.Down) || IsKeyDown(Keys.S)) MoveY += 1f;

            if (_rumbleTimer > 0f)
            {
                _rumbleTimer -= dt;
                if (_rumbleTimer <= 0f && ActiveXInputSlot >= 0)
                {
                    XINPUT_VIBRATION zero = new XINPUT_VIBRATION();
                    try
                    {
                        if (_useXInput14) XInputSetState_14((uint)ActiveXInputSlot, ref zero);
                        else XInputSetState_910((uint)ActiveXInputSlot, ref zero);
                    }
                    catch { }
                }
            }

            PollXInput();
            PollDirectInput();

            float lenSq = MoveX * MoveX + MoveY * MoveY;
            if (lenSq > 1.0f)
            {
                float invLen = 1.0f / (float)Math.Sqrt(lenSq);
                MoveX *= invLen;
                MoveY *= invLen;
            }

            _keysPressedThisFrame.Clear();
        }

        private void PollXInput()
        {
            if (!XInputAvailable) return;

            XInputConnected = false;
            ActiveXInputSlot = -1;

            for (uint i = 0; i < 4; i++)
            {
                XINPUT_STATE state = new XINPUT_STATE();
                uint res;
                try
                {
                    res = _useXInput14 ? XInputGetState_14(i, ref state) : XInputGetState_910(i, ref state);
                }
                catch
                {
                    break;
                }

                if (res == 0)
                {
                    XInputConnected = true;
                    ActiveXInputSlot = (int)i;
                    var gp = state.Gamepad;

                    const short DEADZONE = 7849;
                    float lx = 0f, ly = 0f;
                    if (Math.Abs((int)gp.sThumbLX) > DEADZONE)
                        lx = gp.sThumbLX / 32767f;
                    if (Math.Abs((int)gp.sThumbLY) > DEADZONE)
                        ly = -gp.sThumbLY / 32767f;

                    if ((gp.wButtons & XINPUT_GAMEPAD_DPAD_LEFT) != 0) lx = -1f;
                    if ((gp.wButtons & XINPUT_GAMEPAD_DPAD_RIGHT) != 0) lx = 1f;
                    if ((gp.wButtons & XINPUT_GAMEPAD_DPAD_UP) != 0) ly = -1f;
                    if ((gp.wButtons & XINPUT_GAMEPAD_DPAD_DOWN) != 0) ly = 1f;

                    if (Math.Abs(lx) > 0.05f || Math.Abs(ly) > 0.05f || gp.wButtons != 0)
                    {
                        ActiveInputSource = "XInput Controller #" + (i + 1);
                    }

                    if (Math.Abs(lx) > Math.Abs(MoveX)) MoveX = lx;
                    if (Math.Abs(ly) > Math.Abs(MoveY)) MoveY = ly;

                    ushort newlyPressed = (ushort)(gp.wButtons & ~_prevXButtons);
                    if ((newlyPressed & XINPUT_GAMEPAD_A) != 0) ConfirmPressed = true;
                    if ((newlyPressed & XINPUT_GAMEPAD_B) != 0) CancelPressed = true;
                    if ((newlyPressed & XINPUT_GAMEPAD_Y) != 0 || (newlyPressed & XINPUT_GAMEPAD_BACK) != 0) PhoneTogglePressed = true;
                    if ((newlyPressed & XINPUT_GAMEPAD_X) != 0) BuildMenuPressed = true;
                    if ((newlyPressed & XINPUT_GAMEPAD_LEFT_SHOULDER) != 0) TabPrevPressed = true;
                    if ((newlyPressed & XINPUT_GAMEPAD_RIGHT_SHOULDER) != 0) TabNextPressed = true;
                    if ((newlyPressed & XINPUT_GAMEPAD_START) != 0) PausePressed = true;
                    if ((newlyPressed & XINPUT_GAMEPAD_DPAD_UP) != 0) UpPressed = true;
                    if ((newlyPressed & XINPUT_GAMEPAD_DPAD_DOWN) != 0) DownPressed = true;
                    if ((newlyPressed & XINPUT_GAMEPAD_DPAD_LEFT) != 0) LeftPressed = true;
                    if ((newlyPressed & XINPUT_GAMEPAD_DPAD_RIGHT) != 0) RightPressed = true;

                    _prevXButtons = gp.wButtons;
                    break;
                }
            }
        }

        private void PollDirectInput()
        {
            if (!DirectInputAvailable) return;

            DirectInputConnected = false;
            ActiveDirectInputSlot = -1;

            try
            {
                uint numDevs = joyGetNumDevs();
                for (uint i = 0; i < Math.Min(numDevs, 4u); i++)
                {
                    JOYINFOEX info = new JOYINFOEX();
                    info.dwSize = (uint)Marshal.SizeOf(typeof(JOYINFOEX));
                    info.dwFlags = JOY_RETURNALL;

                    if (joyGetPosEx(i, ref info) == 0)
                    {
                        DirectInputConnected = true;
                        ActiveDirectInputSlot = (int)i;

                        JOYCAPSW caps = new JOYCAPSW();
                        if (joyGetDevCapsW((UIntPtr)i, ref caps, (uint)Marshal.SizeOf(typeof(JOYCAPSW))) == 0 &&
                            !string.IsNullOrWhiteSpace(caps.szPname))
                        {
                            DirectInputDeviceName = caps.szPname.Trim();
                        }

                        if (XInputConnected && ActiveXInputSlot == (int)i)
                            break;

                        float dx = ((int)info.dwXpos - 32767) / 32768f;
                        float dy = ((int)info.dwYpos - 32767) / 32768f;
                        if (Math.Abs(dx) < 0.22f) dx = 0f;
                        if (Math.Abs(dy) < 0.22f) dy = 0f;

                        int dpadDir = -1;
                        if (info.dwPOV < 36000)
                        {
                            if (info.dwPOV >= 31500 || info.dwPOV <= 4500) { dy = -1f; dpadDir = 0; }
                            else if (info.dwPOV >= 4500 && info.dwPOV <= 13500) { dx = 1f; dpadDir = 1; }
                            else if (info.dwPOV >= 13500 && info.dwPOV <= 22500) { dy = 1f; dpadDir = 2; }
                            else if (info.dwPOV >= 22500 && info.dwPOV <= 31500) { dx = -1f; dpadDir = 3; }
                        }

                        if (dpadDir != _prevDpadDir)
                        {
                            if (dpadDir == 0) UpPressed = true;
                            if (dpadDir == 1) RightPressed = true;
                            if (dpadDir == 2) DownPressed = true;
                            if (dpadDir == 3) LeftPressed = true;
                            _prevDpadDir = dpadDir;
                        }

                        if (Math.Abs(dx) > 0.05f || Math.Abs(dy) > 0.05f || info.dwButtons != 0)
                        {
                            ActiveInputSource = "DirectInput (" + DirectInputDeviceName + ")";
                        }

                        if (Math.Abs(dx) > Math.Abs(MoveX)) MoveX = dx;
                        if (Math.Abs(dy) > Math.Abs(MoveY)) MoveY = dy;

                        uint newlyPressed = info.dwButtons & ~_prevDButtons;
                        if ((newlyPressed & 0x0001) != 0) ConfirmPressed = true;
                        if ((newlyPressed & 0x0002) != 0) CancelPressed = true;
                        if ((newlyPressed & 0x0004) != 0) BuildMenuPressed = true;
                        if ((newlyPressed & 0x0008) != 0) PhoneTogglePressed = true;
                        if ((newlyPressed & 0x0010) != 0) TabPrevPressed = true;
                        if ((newlyPressed & 0x0020) != 0) TabNextPressed = true;
                        if ((newlyPressed & 0x0040) != 0) WeatherCyclePressed = true;
                        if ((newlyPressed & 0x0080) != 0) DayTypeCyclePressed = true;
                        if ((newlyPressed & 0x0200) != 0) PausePressed = true;

                        _prevDButtons = info.dwButtons;
                        break;
                    }
                }
            }
            catch { }
        }

        public string GetControllerStatusSummary()
        {
            string xi = XInputConnected ? ("Connected (Slot " + (ActiveXInputSlot + 1) + ")") : (XInputAvailable ? "Ready (Standby)" : "Unavailable");
            string di = DirectInputConnected ? ("Connected (" + DirectInputDeviceName + ")") : (DirectInputAvailable ? "Ready (DirectInput8)" : "Unavailable");
            return "XInput: " + xi + "  |  DirectInput: " + di + "  |  Active: " + ActiveInputSource;
        }
    }
}
