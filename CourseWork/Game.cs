using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Collections;
using System.Windows.Media;
using System.IO;

namespace CourseWork
{
    class Game
    {
        public enum State { Menu, Selecting, Playing, Result };
        private State CurrentState;
        private CanvasHelper cv;
        bool redraw = true;

        // 当前选中的歌曲
        SongResources song = null;

        #region 初始化以及事件绑定等
        public Game(CanvasHelper _cv) {
            cv = _cv;
            cv.MouseLeftButtonUp += Canvas_MouseLeftButtonUp;
            cv.KeyDown += CanvasKeyDown;
            cv.KeyUp += CanvasKeyUp;
            CurrentState = State.Menu;
        }

        private Dictionary<string, object> resources = new Dictionary<string, object>();
        public void Initialize() {
            cv.SetRange(640, 480);

            // 初始化加载各种资源
            cv.cv.Background = Helper.ColorBrush("#000");
            cv.Text(320 - 80, 240 - 40, 80, "Loading...", Helper.ColorBrush("#fff"));
            resources["img.bg"] = Helper.loadImage("Skins/bg.jpg");
            resources["img.start"] = Helper.loadImage("Skins/start_btn.png");
            resources["img.bg-black"] = Helper.loadImage("Skins/bg-black.png");
            ((Brush)resources["img.bg"]).Opacity = 0;
            ((Brush)resources["img.start"]).Opacity = 0;
            LoadSongList();
        }

        List<SongResources> songList = new List<SongResources>();
        private void LoadSongList() {
            DirectoryInfo songDir = new DirectoryInfo("Songs");
            foreach (DirectoryInfo dir in songDir.GetDirectories()) {
                songList.Add(new SongResources(dir));
            }
        }

        ArrayList keyStatus = new ArrayList(16);
        private void CanvasKeyUp(object sender, KeyEventArgs e) {
            if (keyStatus.Contains(e.Key))
                keyStatus.Remove(e.Key);
            switch (CurrentState) {
                case State.Playing:
                    OnKeyUpPlaying(e);
                    break;
            }
        }

        private void CanvasKeyDown(object sender, KeyEventArgs e) {
            // 用于确保按下事件仅触发一次
            if (keyStatus.Contains(e.Key)) return;
            keyStatus.Add(e.Key);
            //Console.WriteLine("Key Down: " + e.Key);
            switch (CurrentState) {
                case State.Menu:
                    break;
                case State.Selecting:
                    OnKeySelecting(e);
                    break;
                case State.Playing:
                    OnKeyPlaying(e);
                    break;
                case State.Result:
                    break;
            }
        }
        // 处理鼠标事件
        private void Canvas_MouseLeftButtonUp(object sender, CanvasHelper.PointEventArg e) {
            Console.WriteLine("Mouse Click: ({0}, {1})", e.point.X, e.point.Y);
            switch (CurrentState) {
                case State.Menu:
                    OnMouseMenu(e.point);
                    break;
                case State.Selecting:
                    OnMouseSelecting(e.point);
                    break;
                case State.Result:
                    break;
            }
        }
        #endregion
 
        public void OnUpdate() {
            switch (CurrentState) {
                case State.Menu:
                    OnDrawMenu();
                    break;
                case State.Selecting:
                    OnDrawSelecting();
                    break;
                case State.Playing:
                    OnDrawPlaying();
                    break;
                case State.Result:
                    break;
            }
        }

        public void ChangeState(State target) {
            Console.WriteLine("Changing to state: " + target.ToString());
            // 切换状态的时候停止播放的音乐
            if (previewing != null) {
                previewing.Stop();
                previewing = null;
            }
            CurrentState = target;
            redraw = true;
            fade = true;
        }

        public void OnDrawMenu() {
            // 绘制菜单，只用绘制一次
            // TODO 用更好看的东西绘制
            if (redraw) {
                cv.Clear();
                if (fade) {
                    ((Brush)resources["img.bg"]).Opacity = 0;
                    ((Brush)resources["img.start"]).Opacity = 0;
                    fade = false;
                }
                cv.Image(0, 0, 640, 480, (Brush)resources["img.bg"]);
                cv.Image(220, 140, 200, 200, (Brush)resources["img.start"]);
                redraw = false;
            }
            if (((Brush)resources["img.bg"]).Opacity < 0.8) {
                ((Brush)resources["img.bg"]).Opacity += 0.04;
            }
            if (((Brush)resources["img.start"]).Opacity < 1) {
                ((Brush)resources["img.start"]).Opacity += 0.04;
            }
        }

        public void OnMouseMenu(Point p) {
            // TODO 重新定位
            if (Helper.PointIn(p, 220, 140, 440, 340)) {
                // 开始
                ChangeState(State.Selecting);
            }
        }

        private bool fade = true;
        private int selectIndex = 0;
        private int difficultyIndex = 0;
        private MediaPlayer previewing = null;
        public void OnDrawSelecting() {
            if (redraw) {
                redraw = false;
                // TODO 用更好看的东西绘制
                cv.Clear();
                if (songList.Count == 0) {
                    cv.Text(320 - 80, 240 - 40, 80, "没有发现任何歌曲...");
                    return;
                }
                cv.Image(0, 0, 640, 480, (Brush)resources["img.bg-black"]);
                // 这里 fade 了表示切歌了，重新载入 song 和 bgm 并且开始从中间位置播放
                if (fade) {
                    if (previewing != null) {
                        previewing.Stop();
                    }
                    song = songList[selectIndex];
                    song.bg.Opacity = 0.2;
                    fade = false;
                    song.LoadMetaAndBgm(difficultyIndex);
                    previewing = null;
                }
                // 歌曲名称
                cv.Text(150, 80, 20, (song.name.Length > 55 ? song.name.Substring(0, 54) + "..." : song.name), Helper.ColorBrush("#ddd"), 640-300);
                // 歌曲封面
                cv.Text(208 - 40, 100 + 80 - 10, 40, "◀", null, 40);
                cv.Text(208 + 224, 100 + 80 - 10, 40, "▶", null, 40);
                cv.Image(208, 100, 224, 160, song.bg);
                // “选择歌曲”
                cv.Rectangle(0, 0, 640, 50, 0, null, Helper.ColorBrush("#000", 0.4));
                cv.Text(10, 12, 40, "选择歌曲");
                // 歌曲数量
                cv.Text(640 - 80, 15, 30, string.Format("({0} / {1}) ", selectIndex + 1, songList.Count));
                // 难度名显示
                for (int i = 0; i < song.difficuties.Count; ++i) {
                    string dname = song.difficuties[i].Name;
                    if (i == difficultyIndex)
                        cv.Text(100, 480 - 220 + 10 + 20 * i, 20, dname.Substring(0, dname.Length - 4), Helper.ColorBrush("#fff"), 640 - 200);
                    else
                        cv.Text(100, 480 - 220 + 10 + 20 * i, 20, dname.Substring(0, dname.Length - 4), Helper.ColorBrush("#666"), 640 - 200);
                }
                // 开始按钮
                cv.Rectangle(260, 380, 120, 40, 3, Helper.ColorBrush("#aef"), Helper.ColorBrush("#000", 0.3));
                cv.Text(260, 385, 40, "Start!", null, 120);
            }

            // 预览音乐的播放，因为可能需要等待音乐加载所以用 previewing 检查
            if (previewing == null && song.bgm.NaturalDuration.HasTimeSpan) {
                song.bgm.Position = TimeSpan.FromSeconds(song.bgm.NaturalDuration.TimeSpan.TotalSeconds / 4);
                song.bgm.Play();
                previewing = song.bgm;
            }
            // 循环播放
            if (previewing != null && previewing.Position.TotalSeconds >= previewing.NaturalDuration.TimeSpan.TotalSeconds - 5) {
                previewing.Position = TimeSpan.FromSeconds(previewing.NaturalDuration.TimeSpan.TotalSeconds / 4);
            }

            // 歌曲封面图的淡出动画
            if (song.bg.Opacity < 0.8) {
                song.bg.Opacity += 0.05;
            }
        }

        public void OnKeySelecting(KeyEventArgs e) {
            if (e.Key == Key.Right) {
                selectIndex = (selectIndex + 1) % songList.Count;
                difficultyIndex = 0;
                redraw = true;
                fade = true;
            } else if (e.Key == Key.Left) {
                selectIndex = (selectIndex - 1 + songList.Count) % songList.Count;
                difficultyIndex = 0;
                redraw = true;
                fade = true;
            } else if (e.Key == Key.Space || e.Key == Key.Enter) {
                GameStart();
            } else if (e.Key == Key.Escape) {
                ChangeState(State.Menu);
            } else if (e.Key == Key.Up) {
                difficultyIndex = (difficultyIndex + song.difficuties.Count - 1) % song.difficuties.Count;
                redraw = true;
            } else if (e.Key == Key.Down) {
                difficultyIndex = (difficultyIndex + 1) % song.difficuties.Count;
                redraw = true;
            }
        }

        public void OnMouseSelecting(Point p) {
            GameStart();
        }
        
        public void OnDrawPlaying() {
            if (redraw) {
                redraw = false;
                cv.Clear();
                cv.Text(300, 280, 50, "Playing...");
            }

            cv.Clear();
            decimal t = Convert.ToDecimal(song.bgm.Position.TotalMilliseconds);

            // 绘制轨道和 Note
            cv.Line(50, 0, 50, 640);
            cv.Line(50 + 30, 0, 50 + 30, 640);
            cv.Line(50 + 60, 0, 50 + 60, 640);
            cv.Line(50 + 90, 0, 50 + 90, 640);
            cv.Line(50 + 120, 0, 50 + 120, 640);
            cv.Line(0, 480 - 30, 1280, 480 - 30);
            for (int i = 0; i < song.notes.Count; ++i) {
                song.notes[i].Draw(cv, t);
            }

            // Miss 判定逻辑
            foreach (Note note in song.notes) {
                if (note.status == Note.Status.Free) {
                    if (note.time < t - 200) {
                        note.status = Note.Status.Miss;
                        // 头判 Miss 情况下尾判直接 Miss
                        note.endStatus = Note.Status.Miss;
                        combo = 0;
                    }
                } else if (note.type == Note.Type.Hold && note.endStatus == Note.Status.Free) {
                    if (note.endtime < t - 200) {
                        note.endStatus = Note.Status.Miss;
                        combo = 0;
                    }
                }
            }

            // 临时，用作测试的内容
            // 显示当前时间
            cv.Text(250, 150, 20, t.ToString());
            // 显示 Combo 数和按键事件处理时间、偏差值
            cv.Text(250, 60, 20, combo + " Combo, dt=" + error);
        }

        public int FindClosestFreeNote(List<Note> notes, Key key, decimal t, bool isReleasedEvent, out Note note) {
            int dt = int.MaxValue;
            note = null;
            foreach (Note n in song.notes) {
                if (!n.IsKeyValid(key, isReleasedEvent)) continue;
                if (Math.Abs((isReleasedEvent ? n.endtime : n.time) - t) < Math.Abs(dt)) {
                    note = n;
                    dt = Convert.ToInt32(t - (isReleasedEvent ? n.endtime : n.time));
                }
            }
            return dt;
        }

        int combo = 0;
        int error = 0;
        public void OnKeyPlaying(KeyEventArgs e) {
            // TODO 重新整理这些按键的逻辑
            if (e.Key == Key.Escape) {
                song.bgm.Pause();
                //ChangeState(State.Selecting);
            } else if (e.Key == Key.Oem3) {
                Restart();
            } else {
                decimal t = Convert.ToDecimal(song.bgm.Position.TotalMilliseconds);
                Note note;
                int dt = FindClosestFreeNote(song.notes, e.Key, t, false, out note);
                error = dt;
                if (note != null)
                    note.Judge(t, false, ref combo);
            }
        }

        public void OnKeyUpPlaying(KeyEventArgs e) {
            // 尾判处理
            decimal t = Convert.ToDecimal(song.bgm.Position.TotalMilliseconds);
            Note note;
            int dt = FindClosestFreeNote(song.notes, e.Key, t, true, out note);
            error = dt;
            if (note != null)
                note.Judge(t, true, ref combo);
        }

        // 先加载各种相关资源并设置变量，然后切换状态到 Playing
        public void GameStart() {
            song.LoadAll(difficultyIndex);
            song.bgm.Position = TimeSpan.Zero;
            // 确保歌曲加载完毕
            while (!song.bgm.NaturalDuration.HasTimeSpan) System.Threading.Thread.Sleep(5);
            song.bgm.Play();
            ChangeState(State.Playing);
        }

        public void Restart() {
            song.bgm.Stop();
            foreach (Note n in song.notes) {
                n.status = Note.Status.Free;
                n.endStatus = Note.Status.Free;
            }
            combo = 0;
            song.bgm.Play();
        }
    }
}
