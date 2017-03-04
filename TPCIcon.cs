namespace TouchpadPeaceFree
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Runtime.InteropServices;
    using TouchpadPeaceFree.Properties;

    public enum HookType : int
    {
        WH_JOURNALRECORD = 0,
        WH_JOURNALPLAYBACK = 1,
        WH_KEYBOARD = 2,
        WH_GETMESSAGE = 3,
        WH_CALLWNDPROC = 4,
        WH_CBT = 5,
        WH_SYSMSGFILTER = 6,
        WH_MOUSE = 7,
        WH_HARDWARE = 8,
        WH_DEBUG = 9,
        WH_SHELL = 10,
        WH_FOREGROUNDIDLE = 11,
        WH_CALLWNDPROCRET = 12,
        WH_KEYBOARD_LL = 13,
        WH_MOUSE_LL = 14
    }

    internal class TPCIcon : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        public class KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public KBDLLHOOKSTRUCTFlags flags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        [Flags]
        public enum KBDLLHOOKSTRUCTFlags : uint
        {
            LLKHF_EXTENDED = 0x01,
            LLKHF_INJECTED = 0x10,
            LLKHF_ALTDOWN = 0x20,
            LLKHF_UP = 0x80,
        }

        public const int WM_POINTERUPDATE = 0x0245;
        public static uint GET_POINTERID_WPARAM(uint wParam) { return wParam & 0xFFFF; }

        public enum POINTER_INPUT_TYPE : int
        {
            PT_POINTER = 0x00000001,
            PT_TOUCH = 0x00000002,
            PT_PEN = 0x00000003,
            PT_MOUSE = 0x00000004,
            PT_TOUCHPAD = 0x00000005
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public int mouseData; // must be int, not uint
            public int flags;
            public int time;
            public UIntPtr dwExtraInfo;
        }

        private static TPCIcon that;

        internal NotifyIcon TrayIcon { get; set; }
        internal ContextMenus ContextMenus;
        internal bool TouchScreenSupport = false;

        protected internal int delayInMilliSeconds;

        protected internal bool mouseOff;
        protected internal long mouseMoveStartTicks;
        protected internal long lastMouseMoveTicks;

        // firstButtonMessage will track the case when the user performed a 
        // control-click, and the upmousebutton event arrives.   This message
        // should be allowed to pass to avoid unwanted object selection.
        protected internal bool firstButtonMessage;

        protected static long MOUSE_MOVE_TIME_TO_ENABLE_CLICKS = 200 * TimeDefs.MILLISECONDS;
        protected static long MOUSE_MOVE_DELAY_BETWEEN_MOVES = 120 * TimeDefs.MILLISECONDS;

        public delegate int HookProc(int code, int wParam, IntPtr lparam);

        static IntPtr hMouseHook = IntPtr.Zero;  // mouse hook handle
        static IntPtr hKeyboardHook = IntPtr.Zero; // keyboard hook handle

        public const int WM_LBUTTONDOWN = 0X0201;
        public const int WM_LBUTTONUP = 0X0202;
        public const int WM_RBUTTONDOWN = 0X0204;
        public const int WM_RBUTTONUP = 0X0205;

        protected static long MOUSEEVENTF_FROMTOUCH = 0xFF515700;
        protected static long MOUSE_INFO_MASK = ~((long)0xFF);

        protected int keyClickCounter;

        private static VirtualKeys[] ignoreKeys = new VirtualKeys[] {
            VirtualKeys.Kana,
            VirtualKeys.Kanji,
            VirtualKeys.Junja,
            VirtualKeys.Shift,
            VirtualKeys.Control,
            VirtualKeys.Menu,
            VirtualKeys.LeftShift,
            VirtualKeys.RightShift,
            VirtualKeys.LeftControl,
            VirtualKeys.RightControl,
            VirtualKeys.LeftMenu,
            VirtualKeys.RightMenu };

        const int TICKS_TO_MILLISECONDS = 10000;  // convertion from ticks (100 nanosec) to milliseconds.

        HookProc mouseHookProc;
        HookProc keyboardHookProc;

        [DllImport("user32.dll", SetLastError = false)]
        static extern IntPtr GetMessageExtraInfo();
        
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetWindowsHookEx(HookType hookType, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool UnhookWindowsHookEx(IntPtr hhk);

        // overload for use with LowLevelKeyboardProc
        [DllImport("user32.dll")]
        static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, uint wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(out POINT lpPoint);

        //// overload for use with LowLevelMouseProc
        //[DllImport("user32.dll")]
        //static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, uint wParam, [In]MSLLHOOKSTRUCT lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, int wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        [DllImport("User32.dll", SetLastError=true)]
        public static extern bool GetPointerType(uint pPointerID, out POINTER_INPUT_TYPE pPointerType);

        //// overload for use with LowLevelKeyboardProc
        //[DllImport("user32.dll")]
        //static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, [In]KBDLLHOOKSTRUCT lParam);

        //// overload for use with LowLevelMouseProc
        //[DllImport("user32.dll")]
        //static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, [In]MSLLHOOKSTRUCT lParam);
        
        public TPCIcon()
        {
            Version version = Environment.OSVersion.Version;
            int versionMajor = version.Major;
            int versionMinor = version.Minor;
            keyClickCounter = 0;
            TouchScreenSupport = (versionMajor >= 7) || (versionMajor == 6 && versionMinor >= 2);
            Point pt = System.Windows.Forms.Cursor.Position;
            Array.Sort(ignoreKeys);
            Program.LoadProgramSettings();
            TrayIcon = new NotifyIcon();
            delayInMilliSeconds = 500;  // DEFAULT_VALUE
            that = this;
        }

        /// <summary>
        /// Displays the icon in the system tray.
        /// </summary>
        public void Display()
        {
            TrayIcon.Icon = Resources.TPPIcon;
            TrayIcon.Text = string.Format(Strings.ReportTemplate, Program.ProgramSettings.FilterCount);
            TrayIcon.Visible = true;

            // Attach a context menu.
            ContextMenus = new ContextMenus(this); ;
            TrayIcon.ContextMenuStrip = ContextMenus.Create();

            mouseHookProc = new HookProc(onMouseAction);
            keyboardHookProc = new HookProc(onKeyboardAction);
            
            using (Process process = Process.GetCurrentProcess())
            using (ProcessModule module = process.MainModule)
            {
                IntPtr hModule = GetModuleHandle(module.ModuleName);
                hKeyboardHook = SetWindowsHookEx(HookType.WH_KEYBOARD_LL, keyboardHookProc, hModule, 0);
                hMouseHook = SetWindowsHookEx(HookType.WH_MOUSE_LL, mouseHookProc, hModule, 0);
            }

            TooltipAlwaysAnnounce(Strings.TouchpadPeaceStarted);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        public void Dispose()
        {
            // When the application closes, this will remove the icon from the system tray immediately.
            TrayIcon.Dispose();
        }

        public static int onMouseAction(int nCode, int wParam, IntPtr lParam)
        {
            int returnValue;
            WM mouseAction = (WM)((uint)wParam);

            MSLLHOOKSTRUCT mouseInfo = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(
                lParam, typeof(MSLLHOOKSTRUCT));

            long mouseExtraInfo = (long)mouseInfo.dwExtraInfo;
            bool buttonUpAction = mouseAction == WM.LBUTTONUP ||
               mouseAction == WM.RBUTTONUP ||
               mouseAction == WM.NCLBUTTONUP ||
               mouseAction == WM.NCRBUTTONUP;

            if ((mouseExtraInfo & MOUSE_INFO_MASK) == MOUSEEVENTF_FROMTOUCH)
            {
                that.mouseOff = false;
            }

            long currentTicks = DateTime.Now.Ticks;
            bool firstMessageOKToPass = that.firstButtonMessage && buttonUpAction;

            if (nCode >= 0 && (buttonUpAction || mouseAction == WM.LBUTTONDOWN ||
               mouseAction == WM.RBUTTONDOWN || mouseAction == WM.RBUTTONDBLCLK ||
               mouseAction == WM.LBUTTONDBLCLK || mouseAction == WM.NCLBUTTONDOWN ||
               mouseAction == WM.NCRBUTTONDOWN || mouseAction == WM.NCRBUTTONDBLCLK ||
               mouseAction == WM.NCLBUTTONDBLCLK))
            {
                if (that.mouseOff && !firstMessageOKToPass)
                {
                    if (((++Program.ProgramSettings.FilterCount) & 0x07) == 0x07)
                        Program.SaveProgramSettings();

                    that.TrayIcon.Text = string.Format(Strings.ReportTemplate, Program.ProgramSettings.FilterCount);
                    return -1;  // eat the event.  Do not pass it to the application
                }

                returnValue = (int)CallNextHookEx(hMouseHook, nCode, (uint)mouseAction, lParam);
                return returnValue;
            }

            if (mouseAction == WM.MOUSEMOVE)
            {
                if (that.mouseOff && that.mouseMoveStartTicks > 0)
                {
                    long ticksSinceLastMove = currentTicks - that.lastMouseMoveTicks;
                    long msecSinceLastMove = (long)TimeSpan.FromTicks(ticksSinceLastMove).TotalMilliseconds;
                    if (msecSinceLastMove > MOUSE_MOVE_DELAY_BETWEEN_MOVES)
                    {
                        that.mouseMoveStartTicks = currentTicks;
                    }
                    else
                    {
                        long mouseMoveTicks = currentTicks - that.mouseMoveStartTicks;
                        long mouseMoveMSec = (long)TimeSpan.FromTicks(mouseMoveTicks).TotalMilliseconds;
                        that.mouseOff = mouseMoveMSec > MOUSE_MOVE_TIME_TO_ENABLE_CLICKS;
                    }
                }
                else if (that.mouseOff && that.mouseMoveStartTicks == 0)
                {
                    that.mouseMoveStartTicks = currentTicks;
                }

                that.lastMouseMoveTicks = currentTicks;
            }

            if (buttonUpAction)
            {
                that.firstButtonMessage = false;
            }

            returnValue = (int)CallNextHookEx(hMouseHook, nCode, (uint)mouseAction, lParam);
            return returnValue;
        }

        internal static void TooltipAlwaysAnnounce(string formatString, params Object[] args)
         {
             string outString = string.Format(formatString, args);
             that.TrayIcon.ShowBalloonTip(1 * TimeDefs.SECONDS, "TouchpadPeace", outString, ToolTipIcon.Info);
         }

        public static int onKeyboardAction(int nCode, int wParam, IntPtr lParam)
        {
            that.keyClickCounter = (that.keyClickCounter + 1) & (0xFFFF);

            if (!that.mouseOff)
            {
                // disable warning on variable not used.  These definition are present mostly
                // for information and possible future use.
#pragma warning disable 219
                const int MOUSEEVENTF_LEFTDOWN = 0x02;
                const int MOUSEEVENTF_LEFTUP = 0x04;
                const int MOUSEEVENTF_RIGHTDOWN = 0x08;
                const int MOUSEEVENTF_RIGHTUP = 0x10;
#pragma warning restore 219

                KBDLLHOOKSTRUCT kbData = new KBDLLHOOKSTRUCT();
                Marshal.PtrToStructure(lParam, kbData);
                VirtualKeys vkCode = (VirtualKeys)kbData.vkCode;
                long currentTicks = DateTime.Now.Ticks;
                int indexKey = Array.BinarySearch(ignoreKeys, vkCode);
                if (indexKey < 0)
                {
                    that.mouseOff = true;
                    that.mouseMoveStartTicks = 0L;
                }
            }

            int returnValue = (int)CallNextHookEx(hKeyboardHook, nCode, wParam, lParam);
            return returnValue;
        }

        public void RemoveHooks() 
        {
            if (hMouseHook != IntPtr.Zero)
            {
                UnhookWindowsHookEx(hMouseHook);
            }

            if (hKeyboardHook != IntPtr.Zero)
            {
                UnhookWindowsHookEx(hKeyboardHook);
            }

            hMouseHook = hKeyboardHook = IntPtr.Zero;
        }
     }
}

