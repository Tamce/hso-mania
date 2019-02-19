using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Media;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace CourseWork
{
    static class Helper
    {
        static public MediaPlayer loadSound(string wav) {
            MediaPlayer sound = new MediaPlayer();
            sound.Open(new Uri(wav, UriKind.RelativeOrAbsolute));
            return sound;
        }

        static public ImageBrush loadImage(string path) {
            BitmapImage bm = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
            return new ImageBrush(bm);
        }

        static public Brush ColorBrush(string rgb) {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(rgb));
        }
    }
}
