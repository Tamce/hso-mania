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

namespace CourseWork
{
    class Game
    {
        public enum State { Menu, Selecting, Playing, Result };
        private State CurrentState;
        private CanvasHelper cv;

        #region 初始化以及事件绑定等
        public Game(CanvasHelper _cv) {
            cv = _cv;
            cv.MouseLeftButtonUp += Canvas_MouseLeftButtonUp;
            cv.KeyDown += CanvasKeyDown;
            cv.KeyUp += CanvasKeyUp;
            CurrentState = State.Menu;
        }

        public void Initialize() {
            cv.SetRange(640, 480);
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
            CurrentState = target;
            redraw = true;
        }

        bool redraw = true;
        public void OnDrawMenu() {
            // 绘制菜单，只用绘制一次
            // TODO 用更好看的东西绘制
            if (redraw) {
                cv.Clear();
                cv.Rectangle(300, 200, 100, 40);
                redraw = false;
                var s = new SongResources("test.osu");
                Console.WriteLine(Helper.OsuGetKey(s.osu, "General", "Mode"));
            }
        }

        public void OnMouseMenu(Point p) {
            // TODO 重新定位
            if (Helper.PointIn(p, 300, 200, 400, 240)) {
                // 开始
                ChangeState(State.Selecting);
            }
        }

        public void OnDrawSelecting() {
            if (redraw) {
                redraw = false;
                // TODO 用更好看的东西绘制
                cv.Clear();
                cv.Text(300, 200, 50, "Song Name Here");
                cv.Rectangle(300, 400, 80, 30);
            }
        }

        public void OnMouseSelecting(Point p) {
            if (Helper.PointIn(p, 300, 400, 380, 430)) {
                GameStart();
            }
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
                ((Note)song.notes[i]).Draw(cv, t);
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

        public int FindClosestFreeNote(ArrayList notes, Key key, decimal t, bool isReleasedEvent, out Note note) {
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

        SongResources song = null;
        // 先加载各种相关资源并设置变量，然后切换状态到 Playing
        public void GameStart() {
            song = (new SongResources("test.osu")).Load();
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
