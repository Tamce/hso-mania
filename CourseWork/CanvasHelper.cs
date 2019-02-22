using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;

namespace CourseWork
{
    class CanvasHelper
    {
        public Canvas cv;
        protected double Height, Width;
        public class PointEventArg : EventArgs {
            public Point point;
        }
        public event EventHandler<PointEventArg> MouseLeftButtonUp;
        public event KeyEventHandler KeyDown;
        public event KeyEventHandler KeyUp;
        public CanvasHelper(Canvas _cv) {
            cv = _cv;
            if (double.IsNaN(cv.Height) && cv.ActualHeight == 0) {
                throw new Exception("Error spawning CanvasHelper, Canvas should be rendered first!");
            }
            Height = cv.ActualHeight;
            Width = cv.ActualWidth;
            cv.PreviewMouseLeftButtonUp += Canvas_MouseLeftButtonUp;
            cv.PreviewKeyDown += Canvas_KeyDown;
            cv.PreviewKeyUp += Canvas_KeyUp; ;
        }

        private void Canvas_KeyUp(object sender, KeyEventArgs e) {
            KeyUp(sender, e);
        }

        private void Canvas_KeyDown(object sender, KeyEventArgs e) {
            KeyDown(sender, e);
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            Point p = e.GetPosition(cv);
            MouseLeftButtonUp(sender, new PointEventArg() {
                point = new Point(w(p.X, true), h(p.Y, true))
            });
        }

        public CanvasHelper SetRange(double width, double height) {
            Width = width;
            Height = height;
            return this;
        }
        protected double w(double x, bool reverse = false) {
            if (reverse)
                return x / cv.ActualWidth * Width;
            return x / Width * cv.ActualWidth;
        }

        protected double h(double y, bool reverse = false) {
            if (reverse)
                return y / cv.ActualHeight * Height;
            return y / Height * cv.ActualHeight;
        }

        public CanvasHelper Clear() {
            cv.Children.Clear();
            return this;
        }

        public CanvasHelper Line(double x0, double y0, double x1, double y1, double thickness = 1, string color="#fff") {
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
                Stroke = color == null ? Helper.ColorBrush("#fff") : color,
                StrokeThickness = thickness,
            };
            if (fill != null) {
                shape.Fill = fill;
            }
            shape.SetValue(Canvas.LeftProperty, w(x0));
            shape.SetValue(Canvas.TopProperty, h(y0));
            cv.Children.Add(shape);
            return this;
        }

        public CanvasHelper Rectangle(double x0, double y0, double width, double height, double thickness = 1, Brush color = null, Brush fill = null) {
            return Shape<Rectangle>(x0, y0, width, height, thickness, color, fill);
        }

        public CanvasHelper Ellipse(double x0, double y0, double width, double height, double thickness = 1, Brush color = null, Brush fill = null) {
            return Shape<Ellipse>(x0, y0, width, height, thickness, color, fill);
        }

        public CanvasHelper Image(double x, double y, double width, double height, Brush img) {
            return Shape<Rectangle>(x, y, width, height, 0, null, img);
        }

        public CanvasHelper Text(double x, double y, double fontSize, string t, Brush color = null, double width = -1) {
            TextBlock text = new TextBlock() {
                Text = t,
                FontSize = fontSize,
                Foreground = color == null ? Helper.ColorBrush("#fff") : color,
                TextAlignment = TextAlignment.Center
            };
            if (width > 0) {
                text.Width = w(width);
            }
            text.SetValue(Canvas.LeftProperty, w(x));
            text.SetValue(Canvas.TopProperty, h(y));
            cv.Children.Add(text);
            return this;
        }
    }
}
