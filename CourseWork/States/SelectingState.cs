using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace CourseWork.States
{
    class SelectingState : StateBase
    {
        List<SongResources> songList;
        SongResources song;
        // 用于控制一些当前页的动画
        bool fade = true;
        int selectIndex = 0;
        int difficultyIndex = 0;
        public SelectingState(CanvasHelper _cv, Dictionary<string, object> res, Game.PlayerWraper playing) : base(_cv, res, playing) {
        }
        public override void OnStateEnter(object args) {
            base.OnStateEnter(args);
            songList = (List<SongResources>)resources["res.songlist"];
            // 在不传参过来的时候才重置选择的歌曲
            if (null == args) {
                selectIndex = 0;
                difficultyIndex = 0;
                song = songList[selectIndex];
            }
            fadeout = true;
        }

        public override void OnKeyDown(object sender, KeyEventArgs e) {
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
                Start();
            } else if (e.Key == Key.Escape) {
                PushState(State.Menu);
            } else if (e.Key == Key.Up) {
                difficultyIndex = (difficultyIndex + song.difficuties.Count - 1) % song.difficuties.Count;
                redraw = true;
            } else if (e.Key == Key.Down) {
                difficultyIndex = (difficultyIndex + 1) % song.difficuties.Count;
                redraw = true;
            }
        }

        public override void OnMouseLeftButtonDown(object sender, CanvasHelper.PointEventArg e) {
            base.OnMouseLeftButtonDown(sender, e);
            if (Helper.PointIn(e.point, 260, 380, 260 + 120, 380 + 40)) {
                Start();
            }
        }

        bool fadeout = true;
        public override void OnDraw() {
            if (fadeout) {
                if (cv.cv.Opacity > 0.1) {
                    cv.cv.Opacity -= 0.1;
                    return;
                }
                fadeout = false;
            }
            if (!fadeout && cv.cv.Opacity < 1) {
                cv.cv.Opacity += 0.1;
            }

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
                    if (playing.player != null) {
                        playing.player.Stop();
                    }
                    song = songList[selectIndex];
                    song.bg.Opacity = 0.2;
                    fade = false;
                    song.LoadMetaAndBgm(difficultyIndex);
                    playing.player = null;
                }
                // 歌曲名称
                cv.Text(150, 80, 20, (song.name.Length > 55 ? song.name.Substring(0, 54) + "..." : song.name), Helper.ColorBrush("#ddd"), 640 - 300);
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
            if (playing.player == null && song.bgm.NaturalDuration.HasTimeSpan) {
                song.bgm.Position = TimeSpan.FromSeconds(song.bgm.NaturalDuration.TimeSpan.TotalSeconds / 5);
                song.bgm.Play();
                playing.player = song.bgm;
            }
            // 循环播放
            if (playing.player != null && playing.player.Position.TotalSeconds >= playing.player.NaturalDuration.TimeSpan.TotalSeconds - 5) {
                playing.player.Position = TimeSpan.FromSeconds(playing.player.NaturalDuration.TimeSpan.TotalSeconds / 5);
            }

            // 歌曲封面图的淡出动画
            if (song.bg.Opacity < 0.8) {
                song.bg.Opacity += 0.05;
            }
        }

        void Start() {
            if (null != playing.player) playing.player.Stop();
            song.LoadAll(difficultyIndex);
            // 确保歌曲加载完毕
            while (!song.bgm.NaturalDuration.HasTimeSpan) System.Threading.Thread.Sleep(5);
            playing.player = song.bgm;
            PushState(State.Playing, song);
        }
    }
}
