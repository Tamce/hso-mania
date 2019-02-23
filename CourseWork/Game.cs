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
using System.Windows.Shapes;

// TODO 成绩结算页面，暂定和返回逻辑
namespace CourseWork
{
    class Game
    {
        public enum State { Menu, Selecting, Playing, Result };
        private State CurrentState;
        private CanvasHelper cv;
        bool redraw = true;

        // 速度系数
        private double factor = 0.7;

        // 分数
        int score = 0;
        // 百分比
        decimal percent = 0;
        // Combo 数
        int combo = 0;
        // 按下的键，位或，从低位到高位分别是DFJK
        int keyPressed = 0;

        // 需要绘制的判定 UI
        string judgeUI = null;
        int judgeUITimeout = 10;

        // stage 位置等数据
        private double stageOffset = 100;
        private double[] segments = new double[] { 28, 64, 95, 126, 162 };

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
            for (int i = 0; i < segments.Length; ++i) segments[i] += stageOffset;

            // 初始化加载各种资源
            cv.cv.Background = Helper.ColorBrush("#000");
            cv.Text(320 - 80, 240 - 40, 80, "Loading...", Helper.ColorBrush("#fff"));
            resources["img.bg"] = Helper.loadImage("Skins/bg.jpg");
            resources["img.start"] = Helper.loadImage("Skins/start_btn.png");
            resources["wav.startup"] = Helper.loadSound("Skins/startup.mp3");
            resources["img.bg-black"] = Helper.loadImage("Skins/bg-black.png");
            resources["img.stage"] = Helper.loadImage("Skins/mania-stage.png");
            resources["img.note1"] = Helper.loadImage("Skins/mania-note1.png");
            resources["img.note1L"] = Helper.loadImage("Skins/mania-note1L.png");
            resources["img.note2"] = Helper.loadImage("Skins/mania-note2.png");
            resources["img.note2L"] = Helper.loadImage("Skins/mania-note2L.png");
            resources["img.light"] = Helper.loadImage("Skins/mania-stage-light.png");
            for (int i = 0; i <= 9; ++i)
                resources["img.score-" + i] = Helper.loadImage("Skins/score-" + i + ".png");
            resources["img.key1"] = Helper.loadImage("Skins/key1.png");
            resources["img.key2"] = Helper.loadImage("Skins/key2.png");
            resources["img.key1D"] = Helper.loadImage("Skins/key1D.png");
            resources["img.key2D"] = Helper.loadImage("Skins/key2D.png");
            resources["wav.se"] = Helper.loadSound("Skins/se.wav");
            foreach (string s in new string[] { "0", "50", "100", "200", "300", "300g"}) {
                resources["img.hit-" + s] = Helper.loadImage("Skins/hit-" + s + ".png");
            }
            foreach (string s in new string[] { "ss", "s", "a", "b", "c", "d" }) {
                resources["img.rank-" + s] = Helper.loadImage("Skins/rank-" + s + ".png");
            }

            ((Brush)resources["img.bg"]).Opacity = 0;
            ((Brush)resources["img.start"]).Opacity = 0;
            ((Brush)resources["img.light"]).Opacity = 0.7;
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
                    if (e.Key == Key.Enter || e.Key == Key.Space) {
                        ChangeState(State.Selecting);
                    }
                    break;
                case State.Selecting:
                    OnKeySelecting(e);
                    break;
                case State.Playing:
                    OnKeyPlaying(e);
                    break;
                case State.Result:
                    if (e.Key == Key.Enter || e.Key == Key.Space) {
                        ChangeState(State.Selecting);
                    }
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
                case State.Playing:
                    // TODO 临时用于测试的
                    ChangeState(State.Result);
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
                    OnDrawResult();
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
            if (redraw) {
                cv.Clear();
                if (fade) {
                    ((Brush)resources["img.bg"]).Opacity = 0;
                    ((Brush)resources["img.start"]).Opacity = 0;
                    fade = false;
                }
                cv.Image(0, 0, 640, 480, (Brush)resources["img.bg"]);
                cv.Image(220, 140, 200, 200, (Brush)resources["img.start"]);
                previewing = (MediaPlayer)resources["wav.startup"];
                previewing.Play();
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
                cv.Text(20, 12, 40, "选择歌曲");
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
                song.bgm.Position = TimeSpan.FromSeconds(song.bgm.NaturalDuration.TimeSpan.TotalSeconds / 5);
                song.bgm.Play();
                previewing = song.bgm;
            }
            // 循环播放
            if (previewing != null && previewing.Position.TotalSeconds >= previewing.NaturalDuration.TimeSpan.TotalSeconds - 5) {
                previewing.Position = TimeSpan.FromSeconds(previewing.NaturalDuration.TimeSpan.TotalSeconds / 5);
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

        public void DrawSpriteNumber(int n, int nDigit, double x, double y, double size, double margin = 0) {
            if (nDigit < 0) {
                int temp = n;
                nDigit = 0;
                do {
                    nDigit++;
                    temp /= 10;
                } while (temp != 0);
            }
            x = x + (nDigit - 1) * (size + margin);
            for (int i = 0; i < nDigit; ++i) {
                cv.Image(x - (margin + size) * i, y, size, size, (Brush)resources["img.score-" + n % 10]);
                n /= 10;
            }
        }

        public void OnDrawResult() {
            Brush bg = song.bg;
            bg.Opacity = 0.4;
            Brush stage = ((Brush)resources["img.stage"]);
            if (fade) {
                if (cv.cv.Opacity > 0.1) {
                    cv.Clear();
                    cv.Image(0, 0, 640, 480, bg);
                    cv.Image(stageOffset, 0, 203, 480, stage);
                    cv.cv.Opacity -= 0.05;
                    return;
                }
                fade = false;
            }
            if (!fade) {
                if (cv.cv.Opacity < 1) {
                    cv.cv.Opacity += 0.05;
                }
            }
            
            if (redraw) {
                redraw = false;
                bg = (Brush)resources["img.bg-black"];
                cv.Clear();
                cv.Image(0, 0, 640, 480, bg);
                // 顶部遮罩
                cv.Rectangle(0, 0, 640, 50, 0, null, Helper.ColorBrush("#000", 0.4));
                cv.Text(20, 12, 40, "游玩结果");
                // 歌曲名和难度名
                // cv.Text(10, 80, 25, song.name);
                // cv.Text(10, 100, 20, song.difficuties[difficultyIndex].Name.Substring(0, song.difficuties[difficultyIndex].Name.Length - 4), Helper.ColorBrush("#bbb"));

                // 统计各类音符的数量并绘制
                {
                    int[] count = new int[] { 0, 0, 0, 0, 0, 0, 0 };
                    foreach (Note n in song.notes) {
                        count[(int)n.status]++;
                        if (n.type == Note.Type.Hold) {
                            count[(int)n.endStatus]++;
                        }
                    }

                    int margin = 35;
                    cv.Image(15, 185, 81, 27, (Brush)resources["img.hit-300g"]);
                    cv.Image(15 + 200, 185, 81, 27, (Brush)resources["img.hit-300"]);
                    cv.Image(15, 185 + margin + 30, 81, 27, (Brush)resources["img.hit-200"]);
                    cv.Image(15 + 200, 185 + margin + 30, 81, 27, (Brush)resources["img.hit-50"]);
                    cv.Image(15, 185 + 2*(margin + 30), 81, 27, (Brush)resources["img.hit-0"]);

                    DrawSpriteNumber(count[(int)Note.Status.PGreat], -1, 105, 187, 25, 0);
                    DrawSpriteNumber(count[(int)Note.Status.Great], -1, 305, 187, 25, 0);
                    DrawSpriteNumber(count[(int)Note.Status.Good], -1, 105, 185+margin + 30 + 2, 25, 0);
                    DrawSpriteNumber(count[(int)Note.Status.Bad], -1, 305, 185+margin + 30 + 2, 25, 0);
                    DrawSpriteNumber(count[(int)Note.Status.Miss], -1, 105, 185+2*(margin + 30) + 2, 25, 0);
                }

                // TODO 响应按键高亮选择按钮
                cv.Rectangle(370, 390, 120, 35, 3, Helper.ColorBrush("#aef"), Helper.ColorBrush("#000", 0.3));
                cv.Text(370, 395, 35, "返回", null, 120);
                cv.Rectangle(370 + 130, 390, 120, 35, 3, Helper.ColorBrush("#aef", 0.3), Helper.ColorBrush("#000", 0.3));
                cv.Text(370 + 130, 395, 35, "重试", null, 120);

                // 绘制 rank
                {
                    Brush ranking = null;
                    if (percent >= 99) ranking = (Brush)resources["img.rank-ss"];
                    else if (percent >= 97) ranking = (Brush)resources["img.rank-s"];
                    else if (percent >= 90) ranking = (Brush)resources["img.rank-a"];
                    else if (percent >= 80) ranking = (Brush)resources["img.rank-b"];
                    else if (percent >= 60) ranking = (Brush)resources["img.rank-c"];
                    else ranking = (Brush)resources["img.rank-d"];
                    cv.Image(380, 130, 240, 200, ranking);
                }

                // 绘制分数和完成率
                {
                    // 分数
                    DrawSpriteNumber(score, 7, 75, 100, 25, 5);
                    // 整数部分
                    DrawSpriteNumber((int)percent, 2, 100, 400, 30, 3);
                    // 小数部分
                    DrawSpriteNumber((int)((percent - (int)percent) * 100), 2, 100 + 2 * 33 + 10, 400, 30, 3);
                    // 小数点和百分号
                    cv.Text(100 + 2 * 30, 400+10, 30, "●", Helper.ColorBrush("#fff", 0.8));
                    cv.Text(100 + 2 * 33 + 10 + 2 * 33 + 10, 400 - 10, 70, "%");
                }                
            }
        }

        public void OnDrawPlaying() {
            cv.Clear();


            decimal t = Convert.ToDecimal(song.bgm.Position.TotalMilliseconds);

            // 歌曲结束判定
            if (song.bgm.Position.TotalMilliseconds > song.bgm.NaturalDuration.TimeSpan.TotalMilliseconds - 100) {
                ChangeState(State.Result);
                return;
            }

            // 绘制背景图片先
            Brush bg = song.bg.Clone();
            bg.Opacity = 0.5;
            cv.Image(0, 0, 640, 480, bg);

            // 绘制轨道和 Note
            cv.Image(stageOffset, 0, 203, 480, (Brush)resources["img.stage"]);
            foreach (double x in segments) {
                cv.Line(x, 0, x, 406);
            }
            for (int i = 0; i < song.notes.Count; ++i) {
                song.notes[i].Draw(cv, t, segments, resources, factor);
            }

            // 绘制轨道光
            for (int i = 0; i < 4; ++i) {
                if ((keyPressed & (1 << i))> 0) {
                    cv.Image(segments[i], 406 - 350, segments[i + 1] - segments[i], 350, (Brush)resources["img.light"]);
                    if (i == 0 || i == 3)
                        cv.Image(segments[i], 413, segments[i + 1] - segments[i], 480 - 413, (Brush)resources["img.key1D"]);
                    else
                        cv.Image(segments[i], 413, segments[i + 1] - segments[i], 480 - 413, (Brush)resources["img.key2D"]);
                } else {
                    if (i == 0 || i == 3)
                        cv.Image(segments[i], 413, segments[i + 1] - segments[i], 480 - 413, (Brush)resources["img.key1"]);
                    else
                        cv.Image(segments[i], 413, segments[i + 1] - segments[i], 480 - 413, (Brush)resources["img.key2"]);
                }
            }

            // 绘制分数和完成率
            {
                int s = score;
                // 一共七位数字
                for (int i = 0; i < 7; ++i) {
                    cv.Image(640 - 35  - 25 * i, 8, 25, 25, (Brush)resources["img.score-" + s % 10]);
                    s /= 10;
                }
                // 小数部分
                s = (int)((percent - (int)percent)*100);
                for (int i = 0; i < 2; ++i) {
                    cv.Image(640 - 50 - 20 * i, 41, 20, 20, (Brush)resources["img.score-" + s % 10]);
                    s /= 10;
                }
                // 小数点
                cv.Text(640 - 50 - 20 - 10, 48, 20, "●", Helper.ColorBrush("#fff", 0.8));
                // 百分号
                cv.Text(640 - 25, 35, 45, "%");
                // 整数部分
                s = (int)percent;
                for (int i = 0; i < 2; ++i) {
                    cv.Image(640 - 50 - 40 - 8 - 20 * i, 41, 20, 20, (Brush)resources["img.score-" + s % 10]);
                    s /= 10;
                }
            }

            // 绘制 combo
            {
                int c = combo;
                // 求总位数算位置
                int n = 0;
                do {
                    c /= 10;
                    n++;
                } while (c != 0);

                c = combo;
                int i = 0;
                do {
                    cv.Image(segments[2] - 22 + 12 * n - 25 * i, 70, 25, 25, (Brush)resources["img.score-" + c % 10]);
                    ++i;
                    c /= 10;
                } while (c != 0);
            }

            // Miss 判定逻辑
            foreach (Note note in song.notes) {
                if (note.status == Note.Status.Free) {
                    if (note.time < t - 200) {
                        note.status = Note.Status.Miss;
                        // 头判 Miss 情况下尾判直接 Miss
                        note.endStatus = Note.Status.Miss;
                        combo = 0;
                        ShowJudgeUI(Note.Status.Miss);
                    }
                } else if (note.type == Note.Type.Hold && note.endStatus == Note.Status.Free) {
                    if (note.endtime < t - 200) {
                        note.endStatus = Note.Status.Miss;
                        combo = 0;
                        ShowJudgeUI(Note.Status.Miss);
                    }
                }
            }

            // 绘制判定 UI
            if (judgeUI != null) {
                double xl = segments[1] - (segments[2] - segments[1]) / 2;
                Brush b = ((Brush)resources["img.hit-" + judgeUI]).Clone();
                b.Opacity = (double)judgeUITimeout / 10;
                double scale = judgeUITimeout;
                cv.Image(xl - scale, 300 - scale, (segments[4] + segments[3]) / 2 - xl + 2 * scale, 20 + 2*scale, b);
                if (--judgeUITimeout < 5) {
                    ResetJudgeUI();
                }
            }
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


        public void OnKeyPlaying(KeyEventArgs e) {
            // TODO 重新整理这些按键的逻辑
            if (e.Key == Key.Escape) {
                song.bgm.Pause();
                //ChangeState(State.Selecting);
            } else if (e.Key == Key.Oem3) {
                Restart();
            } else if (e.Key == Key.F3) {
                if (factor > 0.2) factor -= 0.04;
            } else if (e.Key == Key.F4) {
                if (factor < 3) factor += 0.04;
            } else {
                if (e.Key == Key.D) keyPressed |= 1;
                else if (e.Key == Key.F) keyPressed |= 2;
                else if (e.Key == Key.J) keyPressed |= 4;
                else if (e.Key == Key.K) keyPressed |= 8;
                else return;
                decimal t = Convert.ToDecimal(song.bgm.Position.TotalMilliseconds);
                // Key effect
                ((MediaPlayer)resources["wav.se"]).Position = TimeSpan.Zero;
                ((MediaPlayer)resources["wav.se"]).Play();
                Note note;
                int dt = FindClosestFreeNote(song.notes, e.Key, t, false, out note);
                if (note != null) {
                    if (null != note.Judge(t, false, ref combo, ref score, ref percent, song.totalNoteCount)) {
                        // 按键判定有效，显示判定 UI
                        ShowJudgeUI(note.status);
                    }
                }
            }
        }

        public void OnKeyUpPlaying(KeyEventArgs e) {
            if (e.Key == Key.D) keyPressed &= ~1;
            else if (e.Key == Key.F) keyPressed &= ~2;
            else if (e.Key == Key.J) keyPressed &= ~4;
            else if (e.Key == Key.K) keyPressed &= ~8;
            else return;
            // 尾判处理
            decimal t = Convert.ToDecimal(song.bgm.Position.TotalMilliseconds);
            Note note;
            int dt = FindClosestFreeNote(song.notes, e.Key, t, true, out note);
            if (note != null) {
                if (null != note.Judge(t, true, ref combo, ref score, ref percent, song.totalNoteCount)) {
                    // 按键判定有效，显示判定 UI
                    ShowJudgeUI(note.endStatus);
                }
            }
        }

        public void ResetJudgeUI() {
            judgeUITimeout = 13;
            judgeUI = null;
        }
        public void ShowJudgeUI(Note.Status status) {
            ResetJudgeUI();
            switch (status) {
                case Note.Status.PGreat:
                    judgeUI = "300g";
                    break;
                case Note.Status.Great:
                    judgeUI = "300";
                    break;
                case Note.Status.Good:
                    judgeUI = "200";
                    break;
                case Note.Status.Bad:
                    judgeUI = "50";
                    break;
                case Note.Status.Miss:
                    judgeUI = "0";
                    break;
            }
        }

        // 先加载各种相关资源并设置变量，然后切换状态到 Playing
        public void GameStart() {
            song.LoadAll(difficultyIndex);
            song.bgm.Position = TimeSpan.Zero;
            // 确保歌曲加载完毕
            while (!song.bgm.NaturalDuration.HasTimeSpan) System.Threading.Thread.Sleep(5);
            ChangeState(State.Playing);
            previewing = song.bgm;
            previewing.Play();
        }

        public void Restart() {
            previewing.Stop();
            foreach (Note n in song.notes) {
                n.status = Note.Status.Free;
                n.endStatus = Note.Status.Free;
            }
            combo = 0;
            score = 0;
            percent = 0;
            song.bgm.Play();
        }
    }
}
