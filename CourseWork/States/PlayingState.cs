using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace CourseWork.States
{
    class PlayingState : StateBase
    {
        SongResources song = null;
        int combo = 0;
        int score = 0;
        decimal percent = 0;
        double factor = 0.7;
        const double stageOffset = 100;
        readonly double[] segments = new double[] { 28, 64, 95, 126, 162 };

        int judgeUITimeout = 0;
        string judgeUI = null;
        int keyPressed = 0;

        public PlayingState(CanvasHelper _cv, Dictionary<string, object> res, Game.PlayerWraper playing) : base(_cv, res, playing) {
            for (int i = 0; i < segments.Length; ++i) segments[i] += stageOffset;
        }

        // 接受 SongResources，实际就是重置状态
        public override void OnStateEnter(object args) {
            base.OnStateEnter(args);
            // 默认重置状态
            song = (SongResources)args;
            playing.player = song.bgm;
            Restart();
        }
        public override void OnMouseLeftButtonDown(object sender, CanvasHelper.PointEventArg e) {
            PushState(State.Result, new object[] { song, combo, score, percent });
        }
        public override void OnKeyDown(object sender, KeyEventArgs e) {
            // TODO 重新整理这些按键的逻辑
            if (e.Key == Key.Escape) {
                song.bgm.Pause();
                //ChangeState(State.Selecting);
            } else if (e.Key == Key.Oem3) {
                Restart();
            } else if (e.Key == Key.F3) {
                if (factor > 0.2) factor -= 0.04;
            } else if (e.Key == Key.F4) {
                if (factor < 3) factor += 0.04;
            } else {
                if (e.Key == Key.D) keyPressed |= 1;
                else if (e.Key == Key.F) keyPressed |= 2;
                else if (e.Key == Key.J) keyPressed |= 4;
                else if (e.Key == Key.K) keyPressed |= 8;
                else return;
                decimal t = Convert.ToDecimal(song.bgm.Position.TotalMilliseconds);
                // Key effect
                ((MediaPlayer)resources["wav.se"]).Position = TimeSpan.Zero;
                ((MediaPlayer)resources["wav.se"]).Play();
                Note note;
                int dt = FindClosestFreeNote(song.notes, e.Key, t, false, out note);
                if (note != null) {
                    if (null != note.Judge(t, false, ref combo, ref score, ref percent, song.totalNoteCount)) {
                        // 按键判定有效，显示判定 UI
                        ShowJudgeUI(note.status);
                    }
                }
            }
        }

        public override void OnKeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.D) keyPressed &= ~1;
            else if (e.Key == Key.F) keyPressed &= ~2;
            else if (e.Key == Key.J) keyPressed &= ~4;
            else if (e.Key == Key.K) keyPressed &= ~8;
            else return;
            // 尾判处理
            decimal t = Convert.ToDecimal(song.bgm.Position.TotalMilliseconds);
            Note note;
            int dt = FindClosestFreeNote(song.notes, e.Key, t, true, out note);
            if (note != null) {
                if (null != note.Judge(t, true, ref combo, ref score, ref percent, song.totalNoteCount)) {
                    // 按键判定有效，显示判定 UI
                    ShowJudgeUI(note.endStatus);
                }
            }
        }

        public override void OnDraw() {
            cv.Clear();

            decimal t = Convert.ToDecimal(song.bgm.Position.TotalMilliseconds);

            // 歌曲结束判定
            if (song.bgm.Position.TotalMilliseconds > song.bgm.NaturalDuration.TimeSpan.TotalMilliseconds - 100) {
                PushState(State.Result, new object[] { song, combo, score, percent });
                return;
            }

            // 绘制背景图片先
            Brush bg = song.bg.Clone();
            bg.Opacity = 0.5;
            cv.Image(0, 0, 640, 480, bg);

            // 绘制轨道和 Note
            cv.Image(stageOffset, 0, 203, 480, (Brush)resources["img.stage"]);
            foreach (double x in segments) {
                cv.Line(x, 0, x, 406);
            }
            for (int i = 0; i < song.notes.Count; ++i) {
                song.notes[i].Draw(cv, t, segments, resources, factor);
            }

            // 绘制轨道光
            for (int i = 0; i < 4; ++i) {
                if ((keyPressed & (1 << i)) > 0) {
                    cv.Image(segments[i], 406 - 350, segments[i + 1] - segments[i], 350, (Brush)resources["img.light"]);
                    if (i == 0 || i == 3)
                        cv.Image(segments[i], 413, segments[i + 1] - segments[i], 480 - 413, (Brush)resources["img.key1D"]);
                    else
                        cv.Image(segments[i], 413, segments[i + 1] - segments[i], 480 - 413, (Brush)resources["img.key2D"]);
                } else {
                    if (i == 0 || i == 3)
                        cv.Image(segments[i], 413, segments[i + 1] - segments[i], 480 - 413, (Brush)resources["img.key1"]);
                    else
                        cv.Image(segments[i], 413, segments[i + 1] - segments[i], 480 - 413, (Brush)resources["img.key2"]);
                }
            }

            // 绘制分数和完成率
            {
                int s = score;
                // 一共七位数字
                for (int i = 0; i < 7; ++i) {
                    cv.Image(640 - 35 - 25 * i, 8, 25, 25, (Brush)resources["img.score-" + s % 10]);
                    s /= 10;
                }
                // 小数部分
                s = (int)((percent - (int)percent) * 100);
                for (int i = 0; i < 2; ++i) {
                    cv.Image(640 - 50 - 20 * i, 41, 20, 20, (Brush)resources["img.score-" + s % 10]);
                    s /= 10;
                }
                // 小数点
                cv.Text(640 - 50 - 20 - 10, 48, 20, "●", Helper.ColorBrush("#fff", 0.8));
                // 百分号
                cv.Text(640 - 25, 35, 45, "%");
                // 整数部分
                s = (int)percent;
                for (int i = 0; i < 2; ++i) {
                    cv.Image(640 - 50 - 40 - 8 - 20 * i, 41, 20, 20, (Brush)resources["img.score-" + s % 10]);
                    s /= 10;
                }
            }

            // 绘制 combo
            {
                int c = combo;
                // 求总位数算位置
                int n = 0;
                do {
                    c /= 10;
                    n++;
                } while (c != 0);

                c = combo;
                int i = 0;
                do {
                    cv.Image(segments[2] - 22 + 12 * n - 25 * i, 70, 25, 25, (Brush)resources["img.score-" + c % 10]);
                    ++i;
                    c /= 10;
                } while (c != 0);
            }

            // Miss 判定逻辑
            foreach (Note note in song.notes) {
                if (note.status == Note.Status.Free) {
                    if (note.time < t - 200) {
                        note.status = Note.Status.Miss;
                        // 头判 Miss 情况下尾判直接 Miss
                        note.endStatus = Note.Status.Miss;
                        combo = 0;
                        ShowJudgeUI(Note.Status.Miss);
                    }
                } else if (note.type == Note.Type.Hold && note.endStatus == Note.Status.Free) {
                    if (note.endtime < t - 200) {
                        note.endStatus = Note.Status.Miss;
                        combo = 0;
                        ShowJudgeUI(Note.Status.Miss);
                    }
                }
            }

            // 绘制判定 UI
            if (judgeUI != null) {
                double xl = segments[1] - (segments[2] - segments[1]) / 2;
                Brush b = ((Brush)resources["img.hit-" + judgeUI]).Clone();
                b.Opacity = (double)judgeUITimeout / 10;
                double scale = judgeUITimeout;
                cv.Image(xl - scale, 300 - scale, (segments[4] + segments[3]) / 2 - xl + 2 * scale, 20 + 2 * scale, b);
                if (--judgeUITimeout < 5) {
                    ResetJudgeUI();
                }
            }
        }

        // 停止播放并重置所有变量
        void Restart() {
            playing.player.Stop();
            combo = score = 0;
            percent = 0;
            judgeUITimeout = 0;
            foreach (Note n in song.notes) {
                n.status = Note.Status.Free;
                n.endStatus = Note.Status.Free;
            }
            playing.player.Position = TimeSpan.Zero;
            playing.player.Play();
        }

        void ResetJudgeUI() {
            judgeUITimeout = 13;
            judgeUI = null;
        }

        void ShowJudgeUI(Note.Status status) {
            ResetJudgeUI();
            switch (status) {
                case Note.Status.PGreat:
                    judgeUI = "300g";
                    break;
                case Note.Status.Great:
                    judgeUI = "300";
                    break;
                case Note.Status.Good:
                    judgeUI = "200";
                    break;
                case Note.Status.Bad:
                    judgeUI = "50";
                    break;
                case Note.Status.Miss:
                    judgeUI = "0";
                    break;
            }
        }

        int FindClosestFreeNote(List<Note> notes, Key key, decimal t, bool isReleasedEvent, out Note note) {
            int dt = int.MaxValue;
            note = null;
            foreach (Note n in song.notes) {
                if (!n.IsKeyValid(key, isReleasedEvent)) continue;
                if (Math.Abs((isReleasedEvent ? n.endtime : n.time) - t) < Math.Abs(dt)) {
                    note = n;
                    dt = Convert.ToInt32(t - (isReleasedEvent ? n.endtime : n.time));
                }
            }
            return dt;
        }
    }
}
