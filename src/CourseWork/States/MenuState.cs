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

        // 第一个 State 在Enter的时候
        public override void OnStateEnter(object args) {
            base.OnStateEnter(args);
            playing.player = (MediaPlayer)resources["wav.startup"];
            playing.player.Play();
        }

        double op = 1;
        double dop = -0.04;
        bool showInstruction = false;
        public override void OnDraw() {

            cv.Clear();
            if (redraw) {
                // 绘制菜单，redraw 的时候处理淡入属性
                cv.cv.Opacity = 0;
                redraw = false;
            }
            // 处理淡入
            if (cv.cv.Opacity < 1) {
                cv.cv.Opacity += 0.1;
            }

            if (showInstruction) {
                cv.Image(0, 0, 640, 480, (Brush)resources["img.instruction"]);
            } else {
                ((Brush)resources["img.bg"]).Opacity = 0.8;
                cv.Image(0, 0, 640, 480, (Brush)resources["img.bg"]);
                cv.Image(220, 100, 200, 200, (Brush)resources["img.start"]);
            }

            // 闪烁文字
            if (!showInstruction) {
                cv.Rectangle(0, 340, 640, 53, 0, null, Helper.ColorBrush("#000", 0.5));
                cv.Text(200, 350, 25, "按下 [Space] 或 [Enter] 开始", Helper.ColorBrush("#fff", op), 240);
                cv.Text(200, 370, 18, "按下 [F1] 来查看操作说明", null, 240);
                if (op < 0 || op > 1) {
                    dop *= -1;
                }
                op = op + dop;
            }

            // 循环播放
            if (playing.player != null && playing.player.NaturalDuration.HasTimeSpan && playing.player.Position.TotalSeconds >= playing.player.NaturalDuration.TimeSpan.TotalSeconds - 5) {
                playing.player.Position = TimeSpan.Zero;
            }
        }

        public override void OnMouseDown(object sender, CanvasHelper.PointEventArg e) {
            if (showInstruction) {
                showInstruction = false;
                cv.cv.Opacity = 0.3;
                return;
            }
            PushState(State.Selecting);
        }

        public override void OnKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter || e.Key == Key.Space) {
                if (showInstruction) {
                    showInstruction = false;
                    cv.cv.Opacity = 0.3;
                    return;
                }
                PushState(State.Selecting);
            } else if (e.Key == Key.F1) {
                showInstruction = true;
                cv.cv.Opacity = 0.3;
            } else if (e.Key == Key.Escape && showInstruction) {
                showInstruction = false;
                cv.cv.Opacity = 0.3;
            }
        }
    }
}
