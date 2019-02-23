using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Collections;
using System.Windows.Media;

namespace CourseWork
{
    class Note
    {
        public enum Type { Tap, Hold };
        public enum Status { Free, PGreat, Great, Good, Bad, Miss };
        public Status status = Status.Free;
        // 只对长押有效，表示尾判状态
        public Status endStatus = Status.Free;
        public readonly Type type;
        public readonly decimal time, endtime;
        public readonly int column;

        public Note(int x, decimal t, Type notetype, decimal endt = 0) {
            time = t;
            endtime = endt;
            type = notetype;
            column = x / 128;
        }

        // 检测按键是否对这个 Note 该做响应，响应条件为：Note 未处理，按键正确，松手事件只对 Hold 类型有效
        public bool IsKeyValid(Key key, bool isReleasedEvent) {
            if (key != new Key[] { Key.D, Key.F, Key.J, Key.K }[column]) return false;
            if (isReleasedEvent) {
                return type == Type.Hold && endStatus == Status.Free;
            } else {
                return status == Status.Free;
            }
        }

        // 执行判定，不判定因超时引起的 Miss 判定
        public Status? Judge(decimal t, bool isReleasedEvent, ref int combo) {
            decimal dt = t - (isReleasedEvent ? endtime : time);
            Console.WriteLine("dt: {0}", dt);
            // 松手判定
            if (isReleasedEvent) {
                // 头判没判上，忽略尾判
                if (status == Status.Free || status == Status.Miss) return null;
                // 松手太早，判定 Miss
                if (dt < -200) {
                    combo = 0;
                    endStatus = Status.Miss;
                    return endStatus;
                }
                return JudgeInterval(ref endStatus, ref combo, dt);
            } else {
                if (dt < -200) {
                    // 太早的忽略
                    return null;
                }
                return JudgeInterval(ref status, ref combo, dt);   
            }
        }

        // 不进行 Miss 判定
        private Status? JudgeInterval(ref Status s, ref int combo, decimal _dt) {
            decimal dt = Math.Abs(_dt);
            if (dt <= 20) {
                s = Status.PGreat;
            } else if (dt <= 50) {
                s = Status.Great;
            } else if (dt <= 100) {
                s = Status.Good;
            } else if (dt <= 200) {
                s = Status.Bad;
                combo = -1;
            } else {
                return null;
            }
            combo++;
            return s;
        }

        public bool Draw(CanvasHelper cv, decimal now, double[] segments, Dictionary<string, object> resources, double factor = 0.7) {
            if (!OnScreen(now, factor)) return false;
            if (type == Type.Tap) {
                if (status == Status.Free)
                    if (column == 0 || column == 3)
                        cv.Image(GetNoteX(segments), GetNoteY(time, now, factor), GetNoteWidth(segments), GetNoteLength(factor), (Brush)resources["img.note1"]);
                    else
                        cv.Image(GetNoteX(segments), GetNoteY(time, now, factor), GetNoteWidth(segments), GetNoteLength(factor), (Brush)resources["img.note2"]);
            }
            else {
                Brush b = null;
                if (column == 0 || column == 3)
                    b = (Brush)resources["img.note1L"];
                else
                    b = (Brush)resources["img.note2L"];
                if (status == Status.Miss || endStatus == Status.Miss) {
                    b = b.Clone();
                    b.Opacity = 0.6;
                }
                cv.Image(GetNoteX(segments), GetNoteY(endtime, now, factor), GetNoteWidth(segments), GetNoteLength(factor), b);
            }
            return true;
        }

        private double GetNoteLength(double factor) {
            if (type == Type.Tap) {
                return 10;
            }
            return Convert.ToDouble(endtime - time) * factor + 10;
        }

        private double GetNoteX(double[] segments) {
            return segments[column];
        }
        private double GetNoteWidth(double[] segments) {
            return segments[column + 1] - GetNoteX(segments);
        }

        private double GetNoteY(decimal noteTime, decimal now, double factor) {
            return 480 - 30 - Convert.ToDouble(noteTime - now) * factor;
        }

        public bool OnScreen(decimal now, double factor) {
            if (GetNoteY(time, now, factor) > -30 && GetNoteY(time, now, factor) < 480 + 30) {
                return true;
            }
            if (type == Type.Tap) return false;
            return GetNoteY(endtime, now, factor) < 480 + 30;
        }
    }
}
