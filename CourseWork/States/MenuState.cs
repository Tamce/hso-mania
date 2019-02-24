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

        public override void OnDraw() {
            // 绘制菜单，只用绘制一次
            if (redraw) {
                cv.Clear();
                ((Brush)resources["img.bg"]).Opacity = 0;
                ((Brush)resources["img.start"]).Opacity = 0;
                cv.Image(0, 0, 640, 480, (Brush)resources["img.bg"]);
                cv.Image(220, 140, 200, 200, (Brush)resources["img.start"]);
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
