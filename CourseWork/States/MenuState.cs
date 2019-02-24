using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace CourseWork.States
{
    class MenuState :  StateBase
    {
        public MenuState(CanvasHelper _cv, Dictionary<string, object> res, Game.PlayerWraper playing) : base(_cv, res, playing) {
        }

        double op = 1;
        double dop = -0.04;
        public override void OnDraw() {

            // 绘制菜单，初次绘制的时候设置淡入属性和播放背景音乐
            cv.Clear();
            if (redraw) {
                ((Brush)resources["img.bg"]).Opacity = 0;
                ((Brush)resources["img.start"]).Opacity = 0;
                playing.player = (MediaPlayer)resources["wav.startup"];
                playing.player.Play();
                redraw = false;
            }
            // 处理淡入
            if (((Brush)resources["img.bg"]).Opacity < 0.8) {
                ((Brush)resources["img.bg"]).Opacity += 0.04;
            }
            if (((Brush)resources["img.start"]).Opacity < 1) {
                ((Brush)resources["img.start"]).Opacity += 0.04;
            }

            cv.Image(0, 0, 640, 480, (Brush)resources["img.bg"]);
            cv.Image(220, 100, 200, 200, (Brush)resources["img.start"]);

            // 闪烁文字
            cv.Rectangle(0, 340, 640, 53, 0, null, Helper.ColorBrush("#000", 0.5));
            cv.Text(200, 350, 25, "按下 [Space] 或 [Enter] 开始", Helper.ColorBrush("#fff", op), 240);
            cv.Text(200, 370, 18, "按下 [F1] 来查看操作说明", null, 240);
            if (op < 0 || op > 1) {
                dop *= -1;
            }
            op = op + dop;

            // 循环播放
            if (playing.player != null && playing.player.NaturalDuration.HasTimeSpan && playing.player.Position.TotalSeconds >= playing.player.NaturalDuration.TimeSpan.TotalSeconds - 5) {
                playing.player.Position = TimeSpan.Zero;
            }
        }

        public override void OnMouseLeftButtonDown(object sender, CanvasHelper.PointEventArg e) {
            PushState(State.Selecting);
        }

        public override void OnKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter || e.Key == Key.Space) {
                PushState(State.Selecting);
            }
        }
    }
}
