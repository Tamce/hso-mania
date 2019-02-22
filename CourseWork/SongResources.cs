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
        public List<FileInfo> difficuties;
        public ImageBrush bg;
        public readonly string name;
        public readonly DirectoryInfo dir;
        public SongResources(DirectoryInfo songDir) {
            dir = songDir;
            difficuties = new List<FileInfo>(songDir.GetFiles("*.osu"));
            FilterValidMania4K();
            // 载入一下背景图用于选曲和游戏内
            string events = Helper.OsuGetSection(metadata, "Events");
            int start = events.IndexOf("0,0,\"");
            bg = Helper.loadImage(songDir.FullName + "/" + events.Substring(start + 5, events.IndexOf('"', start + 5) - start - 5));
            // 把谱面编号去掉，后面一个分段就是歌名了
            name = songDir.Name.Substring(songDir.Name.IndexOf(' ') + 1);
        }

        private string metadata;
        private string timing;
        public List<Note> notes;
        public MediaPlayer bgm;

        public void FilterValidMania4K() {
            difficuties.RemoveAll((FileInfo f) => {
                try {
                    using (StreamReader sr = f.OpenText()) {
                        metadata = sr.ReadToEnd();
                    }
                } catch (Exception e) {
                    Console.WriteLine("无法读取谱面文件，忽略之，相关谱面文件: \n{0}", f.Name);
                    return true;
                }
                if (Helper.OsuGetKey(metadata, "General", "Mode") != "3" || Helper.OsuGetKey(metadata, "Difficulty", "CircleSize") != "4") {
                    Console.WriteLine("谱面不是 osu!mania 4K 格式，忽略之，相关谱面文件：\n{0}", f.Name);
                    return true;
                }
                return false;
            });
        }

        public SongResources LoadMetaAndBgm(int difficultyIndex) {
            using (StreamReader sr = difficuties[difficultyIndex].OpenText()) {
                metadata = sr.ReadToEnd();
            }
            timing = Helper.OsuGetSection(metadata, "TimingPoints");
            bgm = Helper.loadSound(dir.FullName + "/" + Helper.OsuGetKey(metadata, "General", "AudioFilename"));
            return this;
        }

        // 读取并解析谱面具体内容，为游玩做最后准备
        public SongResources LoadAll(int difficultyIndex) {
            LoadMetaAndBgm(difficultyIndex);

            // 解析 note 数据
            using (StringReader sr = new StringReader(Helper.OsuGetSection(metadata, "HitObjects"))) {
                // 丢弃 Section 头
                string line = sr.ReadLine();
                notes = new List<Note>(512);
                while ((line = sr.ReadLine()) != null) {
                    string[] param = line.Split(new char[] { ',', ':' });
                    Note note = null;
                    int type = Convert.ToInt32(param[3]);
                    if ((type & 1) != 0) {
                        note = new Note(Convert.ToInt32(param[0]), Convert.ToDecimal(param[2]), Note.Type.Tap);
                    } else if ((type & 128) != 0) {
                        note = new Note(Convert.ToInt32(param[0]), Convert.ToDecimal(param[2]), Note.Type.Hold, Convert.ToDecimal(param[5]));
                    } else {
                        throw new Exception("谱面物件含有无法解析的部分！\nline: " + line);
                    }
                    notes.Add(note);
                }
            }
            return this;
        }
    }
}
