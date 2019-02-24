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
            Helper.errorHandler += Helper_errorHandler;
        }

        private void Helper_errorHandler(object sender, System.IO.ErrorEventArgs e) {
            MessageBox.Show("在执行过程中发生错误！\n\n详细信息: \n" + e.GetException().Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
        }

        private void Canvas_Loaded(object sender, EventArgs e) {
            canvas.Focus();
            cv = new CanvasHelper(canvas);
            game = new Game(cv);

            try {
                game.Initialize();
            } catch (Exception ex) {
                MessageBox.Show("在初始化时发生错误！\n\n详细信息: \n" + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            loop = new Thread(new ThreadStart(Loop));
            looping = true;
            loop.Start();
        }

        bool looping = false;
        private void Loop() {
            try {
                while (looping) {
                    // 同步调用
                    Dispatcher.Invoke(new UpdateDelegate(game.OnUpdate));
                    // 用这种方式来保持帧率稳定
                    Thread.Sleep(mpf - DateTime.Now.Millisecond % mpf);
                }
            } catch (Exception e) {
                MessageBox.Show("在执行过程中发生错误！\n\n详细信息: \n" + e.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            // 如果使用 Abort 来终止线程的话，还会抛出一个 ThreadAbortException 并且 Close 操作将会无法执行
            // 所以干脆用一个变量控制让线程主动退出
            looping = false;
        }
    }
}
