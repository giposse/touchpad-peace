namespace TouchpadPeaceFree
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    class MouseHookStruct
    {
        public POINT pt;
        public int hwnd;
        public int wHitTestCode;
        public int dwExtraInfo;
    }
}
