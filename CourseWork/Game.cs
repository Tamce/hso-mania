using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CourseWork
{
    class Game
    {
        private CanvasHelper cv;
        public Game(CanvasHelper _cv) {
            cv = _cv;
        }

        public void Initialize() {
            cv.SetRange(100, 100);
        }

        private double x = 0, y = 1;
        public void OnUpdate() {
            cv.cv.Children.Clear();
            cv.Line(0, 0, x, 100 - x);
            x += y;
            if (x >= 100 || x <= 0) y *= -1;
            if (x % 10 == 0) {
                Console.WriteLine("{0}:{1}", DateTime.Now.Second, DateTime.Now.Millisecond);
            }
        }
    }
}
