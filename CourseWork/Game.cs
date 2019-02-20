using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;

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
            CurrentState = State.Menu;
        }


        public void Initialize() {
            cv.SetRange(640, 480);
        }

        private void CanvasKeyDown(object sender, KeyEventArgs e) {
            Console.WriteLine("Key Down: " + e.Key);
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
                case State.Playing:
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
        }

        public void OnKeyPlaying(KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                song.bgm.Stop();
                ChangeState(State.Selecting);
            } else {
                Console.WriteLine("Song past: {0}", song.bgm.Position.TotalMilliseconds);
            }
        }

        SongResources song = null;
        // 先加载各种相关资源并设置变量，然后切换状态到 Playing
        public void GameStart() {
            song = (new SongResources("test.osu")).Load();
            song.bgm.Play();
            ChangeState(State.Playing);
        }
    }
}
