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
using State = CourseWork.States.StateBase.State;

// TODO 成绩结算页面，暂定和返回逻辑
namespace CourseWork
{
    class Game
    {
        public class PlayerWraper
        {
            public PlayerWraper(MediaPlayer p = null) { player = p; }
            public MediaPlayer player;
        }
        private CanvasHelper cv;
        PlayerWraper player = new PlayerWraper();
        // 重构状态机
        Dictionary<State, States.StateBase> GameStates;
        private State CurrentState;
        public Game(CanvasHelper _cv) {
            cv = _cv;
            cv.MouseButtonEvent += Canvas_MouseLeftButtonUp;
            cv.KeyDown += CanvasKeyDown;
            cv.KeyUp += CanvasKeyUp;

            GameStates = new Dictionary<State, States.StateBase>(4);
            GameStates[State.Menu] = new States.MenuState(cv, resources, player);
            GameStates[State.Selecting] = new States.SelectingState(cv, resources, player);
            GameStates[State.Playing] = new States.PlayingState(cv, resources, player);
            GameStates[State.Result] = new States.ResultState(cv, resources, player);
            foreach (var s in GameStates) {
                s.Value.OnPushState += OnPushState;
            }
        }

        void OnPushState(State target, object args = null, bool stopMusic = true) {
            if (stopMusic && player.player != null) {
                player.player.Stop();
                player.player = null;
            }
            CurrentState = target;
            GameStates[CurrentState].OnStateEnter(args);
        }

        List<SongResources> songList = new List<SongResources>();
        private Dictionary<string, object> resources = new Dictionary<string, object>();
        public void Initialize() {
            cv.SetRange(640, 480);

            // 初始化加载各种资源
            cv.Text(320 - 80, 240 - 40, 80, "Loading...", Helper.ColorBrush("#fff"));

            resources["img.bg"] = Helper.loadImage("Skins/bg.jpg");
            resources["img.instruction"] = Helper.loadImage("Skins/instruction.jpg");
            resources["img.start"] = Helper.loadImage("Skins/start_btn.png");
            resources["wav.startup"] = Helper.loadSound("Skins/startup.mp3");
            resources["img.bg-black"] = Helper.loadImage("Skins/bg-black.png");
            resources["img.stage"] = Helper.loadImage("Skins/mania-stage.png");
            resources["img.note1"] = Helper.loadImage("Skins/mania-note1.png");
            resources["img.note1L"] = Helper.loadImage("Skins/mania-note1L.png");
            resources["img.note2"] = Helper.loadImage("Skins/mania-note2.png");
            resources["img.note2L"] = Helper.loadImage("Skins/mania-note2L.png");
            resources["img.light"] = Helper.loadImage("Skins/mania-stage-light.png");
            ((Brush)resources["img.light"]).Opacity = 0.7;
            for (int i = 0; i <= 9; ++i)
                resources["img.score-" + i] = Helper.loadImage("Skins/score-" + i + ".png");
            resources["img.key1"] = Helper.loadImage("Skins/key1.png");
            resources["img.key2"] = Helper.loadImage("Skins/key2.png");
            resources["img.key1D"] = Helper.loadImage("Skins/key1D.png");
            resources["img.key2D"] = Helper.loadImage("Skins/key2D.png");
            resources["wav.se"] = Helper.loadSound("Skins/se.wav");
            resources["wav.result"] = Helper.loadSound("Skins/result.mp3");
            ((MediaPlayer)resources["wav.result"]).MediaEnded += (s, e) => {
                ((MediaPlayer)s).Stop();
                ((MediaPlayer)s).Play();
            };
            foreach (string s in new string[] { "0", "50", "200", "300", "300g" }) {
                resources["img.hit-" + s] = Helper.loadImage("Skins/hit-" + s + ".png");
            }
            foreach (string s in new string[] { "ss", "s", "a", "b", "c", "d" }) {
                resources["img.rank-" + s] = Helper.loadImage("Skins/rank-" + s + ".png");
            }            
            LoadSongList();

            // 设置 resource 引用
            resources["res.songlist"] = songList;

            OnPushState(State.Menu);
        }

        private void LoadSongList() {
            DirectoryInfo songDir = new DirectoryInfo("Songs");
            try {
                foreach (DirectoryInfo dir in songDir.GetDirectories()) {
                    songList.Add(new SongResources(dir));
                }
            } catch (Exception e) {
                MessageBox.Show("在加载谱面数据时出错，请移除错误的文件并重新启动！\n\n详细信息: \n" + e.Message);
            }
            for (int i = songList.Count - 1; i >= 0; --i) {
                if (songList[i].difficuties.Count == 0) {
                    songList.RemoveAt(i);
                }
            }
        }

        ArrayList keyStatus = new ArrayList(16);
        private void CanvasKeyUp(object sender, KeyEventArgs e) {
            if (keyStatus.Contains(e.Key))
                keyStatus.Remove(e.Key);
            GameStates[CurrentState].OnKeyUp(sender, e);
        }

        private void CanvasKeyDown(object sender, KeyEventArgs e) {
            // 用于确保按下事件仅触发一次
            if (keyStatus.Contains(e.Key)) return;
            keyStatus.Add(e.Key);
            GameStates[CurrentState].OnKeyDown(sender, e);
        }
        // 处理鼠标事件
        private void Canvas_MouseLeftButtonUp(object sender, CanvasHelper.PointEventArg e) {
            Console.WriteLine("Mouse Click: ({0}, {1})", e.point.X, e.point.Y);
            GameStates[CurrentState].OnMouseDown(sender, e);
        }
 
        public void OnUpdate() {
            GameStates[CurrentState].OnDraw();
        }
    }
}
