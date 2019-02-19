using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Controls;

namespace CourseWork
{
    class Game
    {
        enum State { Idle, Playing, Selecting };
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
            cv.Image(0, 0, 50, 50, Helper.loadImage("1.jpg"));
            cv.Line(0, 0, x, 100 - x);
            cv.Rectangle(10, 20, x, 100 - x);
            x += y;
            if (x >= 100 || x <= 0) y *= -1;
        }
    }
}
