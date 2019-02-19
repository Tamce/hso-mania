using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CourseWork
{
    class CanvasHelper : Canvas
    {
        public Canvas cv;
        protected double Height, Width;
        public CanvasHelper(Canvas _cv) {
            cv = _cv;
            if (double.IsNaN(cv.Height) && cv.ActualHeight == 0) {
                throw new Exception("Error spawning CanvasHelper, Canvas should be rendered first!");
            }
            Height = cv.ActualHeight;
            Width = cv.ActualWidth;
        }

        public CanvasHelper SetRange(double width, double height) {
            Width = width;
            Height = height;
            return this;
        }
        protected double w(double x) {
            return x / Width * cv.ActualWidth;
        }

        protected double h(double y) {
            return y / Height * cv.ActualHeight;
        }

        public CanvasHelper Line(double x0, double y0, double x1, double y1, double thickness = 1, string color="#000") {
            cv.Children.Add(new Line() {
                Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)),
                StrokeThickness = thickness,
                X1 = w(x0),
                Y1 = h(y0),
                X2 = w(x1),
                Y2 = h(y1)
            });
            return this;
        }

        private CanvasHelper Shape<T> (double x0, double y0, double width, double height, double thickness = 1, Brush color = null, Brush fill = null) where T : Shape,new() {
            Shape shape = new T() {
                Width = w(width),
                Height = h(height),
                Stroke = color == null ? Helper.ColorBrush("#000") : color,
                StrokeThickness = thickness,
            };
            if (fill != null) {
                shape.Fill = fill;
            }
            shape.SetValue(LeftProperty, w(x0));
            shape.SetValue(TopProperty, h(y0));
            cv.Children.Add(shape);
            return this;
        }

        public CanvasHelper Rectangle(double x0, double y0, double width, double height, double thickness = 1, Brush color = null) {
            return Shape<Rectangle>(x0, y0, width, height, thickness, color);
        }

        public CanvasHelper Ellipse(double x0, double y0, double width, double height, double thickness = 1, Brush color = null) {
            return Shape<Ellipse>(x0, y0, width, height, thickness, color);
        }

        public CanvasHelper Image(double x, double y, double width, double height, Brush img) {
            return Shape<Rectangle>(x, y, width, height, 0, null, img);
        }
        
    }
}
