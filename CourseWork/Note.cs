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

        public decimal? Judge(Key key, decimal t, bool isReleasedEvent = false) {
            if (key != new Key[] { Key.D, Key.F, Key.J, Key.K }[column]) return null;
            if (isReleasedEvent) {
                return type == Type.Hold ? t - endtime : (decimal?)null;
            }
            return t - time;
        }

        public void Draw(CanvasHelper cv, decimal now, double factor = 0.7) {
            if (type == Type.Tap)
                cv.Rectangle(50 + column * 30, 480 - 30 - Convert.ToDouble(time - now) * factor, 30, 10);
            else
                cv.Rectangle(50 + column * 30, 480 - 30 - Convert.ToDouble(endtime - now) * factor, 30, Convert.ToDouble(endtime - time) * factor);
        }
    }
}
