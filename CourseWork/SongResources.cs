using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Collections;

namespace CourseWork
{
    class SongResources
    {
        public readonly string osu;
        private string timing;
        
        public SongResources(string osufile) {
            try {
                using (StreamReader sr = new StreamReader(osufile)) {
                    osu = sr.ReadToEnd();
                }
            } catch (Exception e) {
                throw new Exception("无法读取谱面文件 " + osufile, e);
            }
            if (Helper.OsuGetKey(osu, "General", "Mode") != "3" || Helper.OsuGetKey(osu, "Difficulty", "CircleSize") != "4") {
                throw new Exception("谱面格式不是 osu!mania 4K 格式");
            }
            timing = Helper.OsuGetSection(osu, "TimingPoints");
        }

        public ArrayList notes;
        public MediaPlayer bgm;
        public MediaPlayer se;
        public SongResources Load() {
            // 先读入 note 数据
            using (StringReader sr = new StringReader(Helper.OsuGetSection(osu, "HitObjects"))) {
                // 丢弃 Section 头
                string line = sr.ReadLine();
                notes = new ArrayList(512);
                while ((line = sr.ReadLine()) != null) {
                    string[] param = line.Split(new char[] { ',', ':' });
                    Note note = null;
                    if (param[3] == "1") {
                        note = new Note(Convert.ToInt32(param[0]), Convert.ToDecimal(param[2]), Note.Type.Tap);
                    } else if (param[3] == "128") {
                        note = new Note(Convert.ToInt32(param[0]), Convert.ToDecimal(param[2]), Note.Type.Hold, Convert.ToDecimal(param[5]));
                    } else {
                        throw new Exception("谱面物件含有无法解析的部分！\nline: " + line);
                    }
                    notes.Add(note);
                }
            }

            // 读入音频和图像等文件
            bgm = Helper.loadSound(Helper.OsuGetKey(osu, "General", "AudioFilename"));
            se = Helper.loadSound("se.wav");
            return this;
        }
    }
}
