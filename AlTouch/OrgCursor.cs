using System;
using System.Runtime.InteropServices;


namespace AlTouch {
    class OrgCursor {
        private const int IDC_ARROW = 32512;

        public void Change(int no) {

            var cur = IntPtr.Zero;
            switch (no) {
                case 0: //default
                    cur = LoadCursorFromFile(@"C:\WINDOWS\Cursors\arrow_m.cur");
                    break;
                case 1:
                    cur = LoadCursorFromFile("AlTouch.cur");
                    break;
                case 2:
                    cur = LoadCursorFromFile("Zoom.cur");
                    break;
                case 3:
                    cur = LoadCursorFromFile("Pinch.cur");
                    break;

            }
            SetSystemCursor(cur, IDC_ARROW);
        }

        [DllImport("user32", EntryPoint = "LoadCursorFromFile")]
        static extern IntPtr LoadCursorFromFile(string lpFileName);

        [DllImport("user32", EntryPoint = "SetSystemCursor")]
        static extern bool SetSystemCursor(IntPtr hcur, uint id);

    }

}
