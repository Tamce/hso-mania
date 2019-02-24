using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace CourseWork.States
{
    abstract class StateBase
    {
        public enum State { Menu, Selecting, Playing, Result };
        protected CanvasHelper cv;
        protected Dictionary<string, object> resources;
        protected Dictionary<string, object> env;
        protected Game.PlayerWraper playing;
        public delegate void StateChangeDelegate(State target, object args);
        public event StateChangeDelegate OnPushState;
        public bool redraw = true;
        public StateBase(CanvasHelper _cv, Dictionary<string, object> res, Game.PlayerWraper playing) {
            cv = _cv;
            resources = res;
            this.playing = playing;
        }
        public void PushState(State target, object args = null) {
            OnPushState(target, args);
        }
        public virtual void OnStateEnter(object args) { redraw = true; }
        public virtual void OnKeyDown(object sender, KeyEventArgs e) { }

        public virtual void OnKeyUp(object sender, KeyEventArgs e) { }

        public virtual void OnMouseLeftButtonDown(object sender, CanvasHelper.PointEventArg e) {
            return;
        }

        public virtual void OnDraw() { }
    }
}
