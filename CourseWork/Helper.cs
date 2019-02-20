using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Media;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows;

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

        static public bool PointIn(Point p, double x1, double y1, double x2, double y2) {
            return p.X >= x1 && p.X <= x2 && p.Y >= y1 && p.Y <= y2;
        }

        static public string OsuGetKey(string osudata, string section, string key) {
            string sec = OsuGetSection(osudata, section);
            int start = sec.IndexOf(key + ":");
            return sec.Substring(start + key.Length + 1, sec.IndexOf("\n", start) - start - key.Length - 1).Trim();
        }

        static public string OsuGetSection(string osudata, string section) {
            int start = osudata.IndexOf("[" + section + "]");
            if (osudata.IndexOf("[", start + 1) < 0)
                return osudata.Substring(start).Trim();
            return osudata.Substring(start, osudata.IndexOf("[", start + 1) - start).Trim();
        }
    }
}
