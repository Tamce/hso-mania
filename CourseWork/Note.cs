using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace CourseWork
{
    class Note
    {
        public enum Type { Tap, Hold };
        public enum Status { Free, Perfect, Great, Good, Miss };
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
        public void Judge(decimal t, bool isReleasedEvent, ref int combo) {
            decimal dt = t - (isReleasedEvent ? endtime : time);
            Console.WriteLine("dt: {0}", dt);
            // 松手判定
            if (isReleasedEvent) {
                // 头判没判上，忽略尾判
                if (status == Status.Free || status == Status.Miss) return;
                // 松手太早，判定 Miss
                if (dt < -200) {
                    combo = 0;
                    endStatus = Status.Miss;
                    return;
                }
                if (Math.Abs(dt) <= 200) {
                    endStatus = Status.Perfect;
                    combo++;
                    return;
                }
            } else {
                if (dt < -200) {
                    // 太早的忽略
                    return;
                }
                if (Math.Abs(dt) <= 200) {
                    status = Status.Perfect;
                    combo++;
                    Console.WriteLine("Combo++ : {0}", combo);
                    return;
                }
            }
        }

        public bool Draw(CanvasHelper cv, decimal now, double factor = 0.7) {
            if (!OnScreen(now, factor)) return false;
            if (type == Type.Tap) {
                cv.Rectangle(50 + column * 30, GetNoteY(time, now, factor), 30, 10, 1, null, Helper.ColorBrush("#666"));
            }
            else
                cv.Rectangle(50 + column * 30, GetNoteY(endtime, now, factor), 30, Convert.ToDouble(endtime - time) * factor, 1, null, Helper.ColorBrush("#666"));
            return true;
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
