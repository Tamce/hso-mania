using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace CourseWork.States
{
    class ResultState : StateBase
    {
        SongResources song;
        int combo, score;
        decimal percent;
        const double stageOffset = 100;
        int resultBtnIdx = 0;
        public ResultState(CanvasHelper _cv, Dictionary<string, object> res, Game.PlayerWraper playing) : base(_cv, res, playing) {
        }

        public override void OnStateEnter(object args) {
            base.OnStateEnter(args);
            resultBtnIdx = 0;
            song = (SongResources)((object[])args)[0];
            combo = (int)((object[])args)[1];
            score = (int)((object[])args)[2];
            percent = (decimal)((object[])args)[3];
            fadeout = true;

            playing.player = (MediaPlayer)resources["wav.result"];
            playing.player.Play();
        }
        public override void OnKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter || e.Key == Key.Space) {
                if (resultBtnIdx == 0)
                    PushState(State.Selecting, true);
                else {
                    PushState(State.Playing, song);
                }
            } else if (e.Key == Key.Right || e.Key == Key.Left || e.Key == Key.Tab) {
                resultBtnIdx = 1 - resultBtnIdx;
                redraw = true;
            }
        }

        bool fadeout = true;
        public override void OnDraw() {
            if (fadeout) {
                if (cv.cv.Opacity > 0.1) {
                    cv.cv.Opacity -= 0.1;
                    return;
                }
                fadeout = false;
            }
            if (!fadeout && cv.cv.Opacity < 1) {
                cv.cv.Opacity += 0.1;
            }

            if (redraw) {
                redraw = false;
                Brush bg = (Brush)resources["img.bg-black"];
                cv.Clear();
                cv.Image(0, 0, 640, 480, bg);
                // 顶部遮罩
                cv.Rectangle(0, 0, 640, 50, 0, null, Helper.ColorBrush("#000", 0.4));
                cv.Text(20, 12, 40, "游玩结果");
                // 歌曲名和难度名
                cv.Text(120, 10, 25, song.name);
                cv.Text(120, 30, 20, song.difficuties[song.loadedDifficultyIndex].Name.Substring(0, song.difficuties[song.loadedDifficultyIndex].Name.Length - 4), Helper.ColorBrush("#bbb"));

                // 统计各类音符的数量并绘制
                {
                    int[] count = new int[] { 0, 0, 0, 0, 0, 0, 0 };
                    foreach (Note n in song.notes) {
                        count[(int)n.status]++;
                        if (n.type == Note.Type.Hold) {
                            count[(int)n.endStatus]++;
                        }
                    }

                    int margin = 35;
                    cv.Image(15, 185, 81, 27, (Brush)resources["img.hit-300g"]);
                    cv.Image(15 + 200, 185, 81, 27, (Brush)resources["img.hit-300"]);
                    cv.Image(15, 185 + margin + 30, 81, 27, (Brush)resources["img.hit-200"]);
                    cv.Image(15 + 200, 185 + margin + 30, 81, 27, (Brush)resources["img.hit-50"]);
                    cv.Image(15, 185 + 2 * (margin + 30), 81, 27, (Brush)resources["img.hit-0"]);

                    DrawSpriteNumber(count[(int)Note.Status.PGreat], -1, 105, 187, 25, 0);
                    DrawSpriteNumber(count[(int)Note.Status.Great], -1, 305, 187, 25, 0);
                    DrawSpriteNumber(count[(int)Note.Status.Good], -1, 105, 185 + margin + 30 + 2, 25, 0);
                    DrawSpriteNumber(count[(int)Note.Status.Bad], -1, 305, 185 + margin + 30 + 2, 25, 0);
                    DrawSpriteNumber(count[(int)Note.Status.Miss], -1, 105, 185 + 2 * (margin + 30) + 2, 25, 0);
                }

                // 绘制按钮
                cv.Rectangle(370, 390, 120, 35, 3, Helper.ColorBrush("#aef", resultBtnIdx == 0 ? 1 : 0.3), Helper.ColorBrush("#000", 0.3));
                cv.Text(370, 395, 35, "返回", null, 120);
                cv.Rectangle(370 + 130, 390, 120, 35, 3, Helper.ColorBrush("#aef", resultBtnIdx == 1 ? 1 : 0.3), Helper.ColorBrush("#000", 0.3));
                cv.Text(370 + 130, 395, 35, "重试", null, 120);

                // 绘制 rank
                {
                    Brush ranking = null;
                    if (percent >= 99) ranking = (Brush)resources["img.rank-ss"];
                    else if (percent >= 97) ranking = (Brush)resources["img.rank-s"];
                    else if (percent >= 90) ranking = (Brush)resources["img.rank-a"];
                    else if (percent >= 80) ranking = (Brush)resources["img.rank-b"];
                    else if (percent >= 60) ranking = (Brush)resources["img.rank-c"];
                    else ranking = (Brush)resources["img.rank-d"];
                    cv.Image(380, 130, 240, 200, ranking);
                }

                // 绘制分数和完成率
                {
                    // 分数
                    DrawSpriteNumber(score, 7, 75, 100, 25, 5);
                    // 整数部分
                    DrawSpriteNumber((int)percent, 2, 100, 400, 30, 3);
                    // 小数部分
                    DrawSpriteNumber((int)((percent - (int)percent) * 100), 2, 100 + 2 * 33 + 10, 400, 30, 3);
                    // 小数点和百分号
                    cv.Text(100 + 2 * 30, 400 + 10, 30, "●", Helper.ColorBrush("#fff", 0.8));
                    cv.Text(100 + 2 * 33 + 10 + 2 * 33 + 10, 400 - 10, 70, "%");
                }
            }
        }
        public override void OnMouseDown(object sender, CanvasHelper.PointEventArg e) {
            base.OnMouseDown(sender, e);
            if (Helper.PointIn(e.point, 370, 390, 370 + 120, 390 + 35)) {
                resultBtnIdx = 0;
                PushState(State.Selecting, true);
            } else if (Helper.PointIn(e.point, 370 + 130, 390, 370 + 130 + 120, 390 + 35)) {
                resultBtnIdx = 1;
                PushState(State.Playing, song);
            }
        }
        void DrawSpriteNumber(int n, int nDigit, double x, double y, double size, double margin = 0) {
            if (nDigit < 0) {
                int temp = n;
                nDigit = 0;
                do {
                    nDigit++;
                    temp /= 10;
                } while (temp != 0);
            }
            x = x + (nDigit - 1) * (size + margin);
            for (int i = 0; i < nDigit; ++i) {
                cv.Image(x - (margin + size) * i, y, size, size, (Brush)resources["img.score-" + n % 10]);
                n /= 10;
            }
        }
    }
}
