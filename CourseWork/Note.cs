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
        private Type type;
        public readonly decimal time, endtime;
        public readonly int column;

        public bool catched = false;

        public Note(int x, decimal t, Type notetype, decimal endt = 0) {
            time = t;
            endtime = endt;
            type = notetype;
            column = x / 128;
        }

        public bool IsKeyValid(Key key, decimal t, bool isReleasedEvent = false) {
            if (catched) return false;
            if (key != new Key[] { Key.D, Key.F, Key.J, Key.K }[column]) return false;
            if (isReleasedEvent) {
                return type == Type.Hold;
            }
            return true;
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
