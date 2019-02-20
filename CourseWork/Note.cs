using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CourseWork
{
    class Note
    {
        public enum Type { Tap, Hold };
        private Type type;
        private decimal time, endtime;
        private int column;
        public Note(int x, decimal t, Type notetype, decimal endt = 0) {
            time = t;
            endtime = endt;
            type = notetype;
            column = x / 128;
        }
    }
}
