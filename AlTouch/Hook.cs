using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using WindowsInput;

namespace AlTouch {
    //*******************************************************************
    // SetWindowsHookExによりマウス及びキーの入力をグローバルフックし
    // Altキーを押している間だけ、マウス入力をタッチ入力に変換する
    //*******************************************************************
    public class Hook : IDisposable {
        private readonly ListBox _listBox;

        HookProcDelegate _mouseProc;
        HookProcDelegate _keyProc;
        readonly IntPtr _mouseHook;
        readonly IntPtr _keyHook;
        private int _touchMode; //0=通常 1=スライド　2=ズーム　3=ピンチ

        bool _enable = true;


        //Zoomの際にX座標をずらす為に使用する
        private readonly int _screenWodth;

        //スライド操作
        Touch _touch;
        //スワイプ操作
        readonly Swipe _swipe;
        //オリジナルカーソル
        private readonly OrgCursor _cur;

        public Hook(ListBox listBox, int screenWidth, int screenHeight) {
            _listBox = listBox;
            _screenWodth = screenWidth;
            _cur = new OrgCursor();

            _swipe = new Swipe(screenWidth, screenHeight);

            //WH_MOUSE_LL = 14
            _mouseHook = SetWindowsHookEx(14, _mouseProc = MouseHookProc, Marshal.GetHINSTANCE(typeof(Hook).Module), 0);
            //WH_KEYBOARD_LL = 13
            _keyHook = SetWindowsHookEx(13, _keyProc = KeyHookProc, Marshal.GetHINSTANCE(typeof(Hook).Module), 0);

            AppDomain.CurrentDomain.DomainUnload += delegate {
                if (_mouseHook != IntPtr.Zero)
                    UnhookWindowsHookEx(_mouseHook);
                if (_keyHook != IntPtr.Zero)
                    UnhookWindowsHookEx(_keyHook);
            };
        }

        public void Dispose() {
            _cur.Change(0);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSLLHOOKSTRUCT {
            public POINT pt;
            public int mouseData;
            public int flags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KBDLLHOOKSTRUCT {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        delegate IntPtr HookProcDelegate(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern IntPtr SetWindowsHookEx(int idHook, HookProcDelegate lpfn, IntPtr hMod, int dwThreadId);

        [DllImport("user32.dll")]
        static extern IntPtr CallNextHookEx(IntPtr hHook, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern bool UnhookWindowsHookEx(IntPtr hHook);



        IntPtr MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam) {
            if (nCode != 0) { //HC_ACTION=0
                return CallNextHookEx(_mouseHook, nCode, wParam, lParam);
            }

            if (_touchMode == 0) {
                return CallNextHookEx(_mouseHook, nCode, wParam, lParam);
            }

            if (nCode == 0) { //HC_ACTION=0

                var w = wParam.ToInt32();
                var m = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

                if (_touchMode == 1) {

                    _cur.Change(_touchMode);

                    if (w == 513) {
                        if (SwipeResult.Non == _swipe.Start(m.pt.x, m.pt.y)){
                            //スワイプの対象外のみスライドを初期化する
                            _touch = new Touch();
                            _touch.Down(m.pt.x, m.pt.y);

                        } else{
//                            _altEnable = false;//スワイプが有効になった時点でALTを無効化
                        }

                    } else if (w == 514) {
                        _swipe.Stop();
                        //_altEnable = true;//スワイプが無効なった時点でALTを有効化
                        if (_touch != null) {
                            _touch.Up();
                            _touch = null;
                        }
                    }
                    //ドラッグ
                    SwipeResult swipeResult = _swipe.Move(m.pt.x, m.pt.y);
                    switch (swipeResult) {
                        case SwipeResult.Right:
                            _listBox.Items.Add("RIGHT");
                            InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.LWIN, VirtualKeyCode.VK_C); //チャームメニュー
                            break;
                        case SwipeResult.Left:
                            _listBox.Items.Add("LIGHT");
                            var keys = new List<VirtualKeyCode>{
                                VirtualKeyCode.LWIN,
                                VirtualKeyCode.CONTROL};
                            InputSimulator.SimulateModifiedKeyStroke(keys, VirtualKeyCode.TAB);
                            break;
                        case SwipeResult.Top:
                            _listBox.Items.Add("TOP");
                            InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.LWIN, VirtualKeyCode.VK_Z); //アプリバー
                            break;
                        case SwipeResult.Bottom:
                            _listBox.Items.Add("BOTTOM");
                            InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.LWIN, VirtualKeyCode.VK_Z); //アプリバー
                            break;
                    }
                    if (swipeResult == SwipeResult.Non) {
                        if (_touch != null) {
                            _touch.Drag(m.pt.x, m.pt.y);
                        }
                    } else if (swipeResult == SwipeResult.Busy) {
                        //Busyの場合は、処理なし
                    } else {
                        _swipe.Stop();
                        if (_touch != null) {
                            _touch.Up();
                            _touch = null;
                        }
                    }

                    //タッチモードの間は、マウスのUP/DOWNは無効化する
                    if (w == 513 || w == 514) {
                        //入力は無効化する
                        if (_touch != null) {
                            return (IntPtr)1;
                        }
                    }

                } else if (_touchMode == 2 || _touchMode == 3) { //Zoom

                    if (w == 513) {
                        //画面サイズを超えないように修正
                        var x = m.pt.x;
                        if (x < 150) {
                            x = 150; //最低150は必要
                        } else if (_screenWodth - 150 < x)
                            x = _screenWodth - 150; //最大値

                        _touch = new Touch();
                        _touch.Zoom((_touchMode == 3), x, m.pt.y);
                        _touch = null;
                    }

                    //タッチモードの間は、マウスのUP/DOWNは無効化する
                    if (w == 513 || w == 514) {
                        //入力は無効化する
                        return (IntPtr)1;
                    }
                    _cur.Change(_touchMode);

                }

            }
            return CallNextHookEx(_mouseHook, nCode, wParam, lParam);
        }

        //**********************************************************************************
        // ALT押下中のみONFする場合
        //**********************************************************************************
        //ALT押下状態
        private bool _alt = false;

        IntPtr KeyHookProc(int nCode, IntPtr wParam, IntPtr lParam) {
            var k = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
            if (nCode == 0) { //HC_ACTION=0
                //_cur.Change(_touchMode ? 1 : 0);

                //WM_KEYDOWN = 0x0100;
                //WM_KEYUP = 0x0101;
                //WM_SYSKEYDOWN = 0x0104;
                //ALT ScanCode==56
                //CTRL ScanCode==29
                //SHIFT ScanCode==42

                if (wParam.ToInt32() == 0x0104 && k.scanCode == 56){
                    _alt = true;
                } else if (wParam.ToInt32() == 0x0101 && k.scanCode == 56){
                    _alt = false;
                }



                if (_alt && wParam.ToInt32() == 0x0100 && k.scanCode == 29) {
                    _enable = !_enable;
                    if (!_enable) {
                        _touchMode = 0;
                    }
                    _listBox.Items.Add(_enable);
                    _cur.Change(_touchMode);
                }


                if (_alt && wParam.ToInt32() == 0x0104 && k.scanCode == 42) {
                    //_shift = true;//ALT+SHIFT DOWN
                    if (_touchMode == 1) {
                        _touchMode = 2; //Zoom
                    } else if (_touchMode == 2) {
                        _touchMode = 3; //Pinch
                    } else if (_touchMode == 3) {
                        _touchMode = 1;
                    }
                    _cur.Change(_touchMode);
                    _listBox.Items.Add(_touchMode);
                }



                if (_enable && wParam.ToInt32() == 0x0104 && k.scanCode == 56) {
                    _touchMode = 1;
                    _cur.Change(_touchMode);//カーソル変更
                    //通常モードで実行中の処理をキャンセルするため
                    //ここでマウスUPを疑似的に送信する必要がある（未実装）
                } else if (wParam.ToInt32() == 0x0101 && k.scanCode == 56) {
                    _touchMode = 0;
                    _cur.Change(_touchMode); //カーソル復帰
                    //もし、スライド操作中の場合は、破棄する
                    if (_touch != null) {
                        _touch.Up();
                        _touch = null;
                    }
                    //もし、スワイプ操作中の場合は、破棄する
                    _swipe.Stop();

                }


                //スワイプ検出中の場合、ALTキーに関する情報をカットする
                //if (!_altEnable){
                if (_enable) {
                    if(_touchMode != 0){
                        if (k.scanCode == 56){
                            //入力は無効化する
                            return (IntPtr) 1;
                        }
                    }
                }

                //if (_touchMode != 0) {
                    _listBox.Items.Add(String.Format("{0:X} Code={1} Scan={2}", wParam.ToInt32(), k.vkCode, k.scanCode));
                //}

            }
            return CallNextHookEx(_keyHook, nCode, wParam, lParam);
        }


        //**********************************************************************************
        // CTRL+ALTでON/OFFする場合
        //**********************************************************************************
        /*
        //CTRL押下状態
        private bool _ctrl = false;

        IntPtr KeyHookProc(int nCode, IntPtr wParam, IntPtr lParam) {
            var k = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
            if (nCode == 0) { //HC_ACTION=0
                //_cur.Change(_touchMode ? 1 : 0);

                //WM_KEYDOWN = 0x0100;
                //WM_KEYUP = 0x0101;
                //WM_SYSKEYDOWN = 0x0104;
                //ALT ScanCode==56
                //CTRL ScanCode==29

                //if (wParam.ToInt32() == 0x0104 && k.scanCode == 56) {//ALT DOWN
                if (wParam.ToInt32() == 0x0101 && k.scanCode == 56) {
                    //CTRL+ALTキーでタッチモードをON/OFFする
                    if (_ctrl) {
                        _touchMode = !_touchMode;
                        _listBox.Items.Add(_touchMode);
                        _cur.Change(_touchMode ? 1 : 0);
                    }
                } else if (wParam.ToInt32() == 0x0100 && k.scanCode == 29) {
                    _ctrl = true;//CTRL DOWN
                } else if (wParam.ToInt32() == 0x0100 && k.scanCode == 29) {
                    _ctrl = false;//CTRL UP
                }
                _listBox.Items.Add(String.Format("{0:X} Code={1} Scan={2}", wParam.ToInt32(), k.vkCode, k.scanCode));

            }
            return CallNextHookEx(_keyHook, nCode, wParam, lParam);
        }
        */
    }
}
