using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CourseWork
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private CanvasHelper cv;
        protected Random random;
        private delegate void UpdateDelegate();
        private Thread loop;
        private Game game;
        // 每帧允许绘制的时间
        private int mpf = 1000 / 40;
        public MainWindow() {
            InitializeComponent();
            random = new Random();
        }
        private void Canvas_Loaded(object sender, EventArgs e) {
            canvas.Focus();
            cv = new CanvasHelper(canvas);
            game = new Game(cv);
            // 异步加载各种文件
            Dispatcher.Invoke(() => {
                game.Initialize();
            });
            loop = new Thread(new ThreadStart(Loop));
            loop.Start();
        }
        private void Loop() {
            while (true) {
                // 同步调用
                Dispatcher.Invoke(new UpdateDelegate(game.OnUpdate));
                // 用这种方式来保持帧率稳定
                Thread.Sleep(mpf - DateTime.Now.Millisecond % mpf);
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            loop.Abort();
        }
    }
}
