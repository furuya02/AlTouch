using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace AlTouch {
    internal class Touch {
        private readonly POINTER_TOUCH_INFO[] contacts = new POINTER_TOUCH_INFO[10];

        public void Down(int x, int y) {
            InitializeTouchInjection(10, TOUCH_FEEDBACK_NONE);

            contacts[0].pointerInfo.pointerType = POINTER_INPUT_TYPE.PT_TOUCH;
            contacts[0].touchFlags = TouchFlags.NONE;

            contacts[0].orientation = 90;
            contacts[0].pressure = 32000;
            contacts[0].pointerInfo.pointerFlags = POINTER_FLAG.DOWN | POINTER_FLAG.INRANGE | POINTER_FLAG.INCONTACT;
            contacts[0].touchMask = TouchMask.CONTACTAREA | TouchMask.ORIENTATION | TouchMask.PRESSURE;
            contacts[0].pointerInfo.ptPixelLocation.x = x;
            contacts[0].pointerInfo.ptPixelLocation.y = y;
            contacts[0].pointerInfo.pointerId = 0;

            const int marge = 2;
            contacts[0].rcContact.left = x - marge;
            contacts[0].rcContact.right = x + marge;
            contacts[0].rcContact.top = y - marge;
            contacts[0].rcContact.bottom = y - marge;

//            InitContact(0, x, y);


            InjectTouchInput(1, contacts);
        }

        public void Hold() {

            contacts[0].pointerInfo.pointerFlags = POINTER_FLAG.UPDATE | POINTER_FLAG.INRANGE | POINTER_FLAG.INCONTACT;
            for (int i = 0; i < 100000; i++) {
                InjectTouchInput(1, contacts);
            }
        }

        public void Drag(int x, int y) {

            contacts[0].pointerInfo.pointerFlags = POINTER_FLAG.UPDATE | POINTER_FLAG.INRANGE | POINTER_FLAG.INCONTACT;

            contacts[0].pointerInfo.ptPixelLocation.x = x;
            contacts[0].pointerInfo.ptPixelLocation.y = y;

            InjectTouchInput(1, contacts);
        }

        public void Up() {
            contacts[0].pointerInfo.pointerFlags = POINTER_FLAG.UP;
            InjectTouchInput(1, contacts);
        }

        public void Zoom(bool pinch, int x, int y) {

            InitializeTouchInjection(10, TOUCH_FEEDBACK_NONE);

            InitContact(0, x - 150, y);
            InitContact(1, x + 150, y);

            InjectTouchInput(2, contacts);

            contacts[0].pointerInfo.pointerFlags = POINTER_FLAG.UPDATE | POINTER_FLAG.INRANGE | POINTER_FLAG.INCONTACT;
            contacts[1].pointerInfo.pointerFlags = POINTER_FLAG.UPDATE | POINTER_FLAG.INRANGE | POINTER_FLAG.INCONTACT;

            for (int i = 0; i < 150; i++) {
                if (pinch) {
                    contacts[0].pointerInfo.ptPixelLocation.x += 1;
                    contacts[1].pointerInfo.ptPixelLocation.x -= 1;
                } else {
                    contacts[0].pointerInfo.ptPixelLocation.x -= 1;
                    contacts[1].pointerInfo.ptPixelLocation.x += 1;
                }
                InjectTouchInput(2, contacts);
                Thread.Sleep(2);
            }

            contacts[0].pointerInfo.pointerFlags = POINTER_FLAG.UP;
            contacts[1].pointerInfo.pointerFlags = POINTER_FLAG.UP;

            InjectTouchInput(2, contacts);
        }

        void InitContact(int n, int x, int y) {
            contacts[n].pointerInfo.pointerType = POINTER_INPUT_TYPE.PT_TOUCH;
            contacts[n].touchFlags = TouchFlags.NONE;

            contacts[n].orientation = 90;
            contacts[n].pressure = 32000;
            contacts[n].pointerInfo.pointerFlags = POINTER_FLAG.DOWN | POINTER_FLAG.INRANGE | POINTER_FLAG.INCONTACT;
            contacts[n].touchMask = TouchMask.CONTACTAREA | TouchMask.ORIENTATION | TouchMask.PRESSURE;
            contacts[n].pointerInfo.ptPixelLocation.x = x;
            contacts[n].pointerInfo.ptPixelLocation.y = y;
            contacts[n].pointerInfo.pointerId = (uint)n+1;

            const int marge = 2;
            contacts[n].rcContact.left = x - marge;
            contacts[n].rcContact.right = x + marge;
            contacts[n].rcContact.top = y - marge;
            contacts[n].rcContact.bottom = y - marge;
        }


        [DllImport("User32.dll")]
        private static extern bool InitializeTouchInjection(uint maxCount, int dwMode);

        [DllImport("User32.dll")]
        private static extern bool InjectTouchInput(uint count, [MarshalAs(UnmanagedType.LPArray), In] POINTER_TOUCH_INFO[] contacts);


        public enum POINTER_FLAG {
            //            NONE = 0x00000000,
            //            NEW = 0x00000001,
            INRANGE = 0x00000002,
            INCONTACT = 0x00000004,
            //            FIRSTBUTTON = 0x00000010,
            //            SECONDBUTTON = 0x00000020,
            //            THIRDBUTTON = 0x00000040,
            //            OTHERBUTTON = 0x00000080,
            //            PRIMARY = 0x00000100,
            //            CONFIDENCE = 0x00000200,
            //            CANCELLED = 0x00000400,
            DOWN = 0x00010000,
            UPDATE = 0x00020000,
            UP = 0x00040000,
            //            WHEEL = 0x00080000,
            //            HWHEEL = 0x00100000
        }

        //        int TOUCH_FEEDBACK_DEFAULT = 0x1;
        //        int TOUCH_FEEDBACK_INDIRECT = 0x2;
        private int TOUCH_FEEDBACK_NONE = 0x03;


        [StructLayout(LayoutKind.Sequential)]
        public struct RECT {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }


        public enum TouchFlags {
            NONE = 0x00000000
        }

        public enum TouchMask {
            //            NONE = 0x00000000,
            CONTACTAREA = 0x00000001,
            ORIENTATION = 0x00000002,
            PRESSURE = 0x00000004
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct TouchPoint {
            public int x;
            public int y;
        }

        public enum POINTER_INPUT_TYPE {
            //PT_POINTER = 0x00000001,
            PT_TOUCH = 0x00000002,
            //PT_PEN = 0x00000003,
            //PT_MOUSE = 0x00000004
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct POINTER_INFO {
            public POINTER_INPUT_TYPE pointerType;
            public uint pointerId;
            public uint frameId;
            public POINTER_FLAG pointerFlags;
            public IntPtr sourceDevice;
            public IntPtr hwndTarget;
            public TouchPoint ptPixelLocation;
            public TouchPoint ptPixelLocationRaw;
            public TouchPoint ptHimetricLocation;
            public TouchPoint ptHimetricLocationRaw;
            public uint dwTime;
            public uint historyCount;
            public uint inputData;
            public uint dwKeyStates;
            public ulong PerformanceCount;
            public int ButtonChangeType;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINTER_TOUCH_INFO {
            public POINTER_INFO pointerInfo;
            public TouchFlags touchFlags;
            public TouchMask touchMask;
            public RECT rcContact;
            public RECT rcContactRaw;
            public uint orientation;
            public uint pressure;
        }
    }
}
