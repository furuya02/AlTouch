using System;
using System.Windows;
using System.Windows.Forms;

namespace AlTouch {
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window{
        readonly NotifyIcon _notifyIcon; //タスクトレイに格納
        private Hook _hook;

        public MainWindow() {
            InitializeComponent();

            var screenHeight = Screen.PrimaryScreen.WorkingArea.Height;
            var screenWidth = Screen.PrimaryScreen.WorkingArea.Width;

            _hook = new Hook(ListBoxLog, screenWidth, screenHeight);

            //タスクトレイアイコンの初期化
            _notifyIcon = new NotifyIcon();
            _notifyIcon.Text = "AlTouch マウスでタッチ操作";
            _notifyIcon.Icon = new System.Drawing.Icon("AlTouch.ico");

            //タスクトレイへの表示
            _notifyIcon.Visible = true;

            //メニュー「終了」追加
            var menuStrip = new ContextMenuStrip();
            var exitItem = new ToolStripMenuItem();
            exitItem.Text = "終了";
            menuStrip.Items.Add(exitItem);
            exitItem.Click += ExitItemClick;
            _notifyIcon.ContextMenuStrip = menuStrip;
            _notifyIcon.MouseClick += NotifyIconMouseClick;

        }

        private void NotifyIconMouseClick(object sender, MouseEventArgs e) {
            try {
                if (e.Button == MouseButtons.Left) {
                    //ウィンドウを可視化
                    Visibility = Visibility.Visible;
                    WindowState = WindowState.Normal;
                }
            } catch { }
        }


        //終了メニューのイベントハンドラ
        private void ExitItemClick(object sender, EventArgs e) {
            try {
                _notifyIcon.Dispose();
                System.Windows.Application.Current.Shutdown();
            } catch { }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            try {
                e.Cancel = true;
                Visibility = Visibility.Collapsed;
            } catch { }
        }

    }
}
