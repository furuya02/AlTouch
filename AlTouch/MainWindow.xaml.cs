using System.Windows;
using System.Windows.Forms;

namespace AlTouch {
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window{
        private Hook _hook;
        public MainWindow() {
            InitializeComponent();

            var screenHeight = Screen.PrimaryScreen.WorkingArea.Height;
            var screenWidth = Screen.PrimaryScreen.WorkingArea.Width;

            _hook = new Hook(listBoxLog, screenWidth, screenHeight);
        }
    }
}
