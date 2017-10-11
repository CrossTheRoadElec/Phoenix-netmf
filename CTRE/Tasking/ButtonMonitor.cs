using System;
using Microsoft.SPOT;
using CTRE.Phoenix;

namespace CTRE.Phoenix.Tasking
{
    public class ButtonMonitor : IProcessable, ILoopable
    {
        public delegate void ButtonPressEventHandler(int idx, bool isDown);

        public ButtonPressEventHandler ButtonPress { get; set; }

        CTRE.Phoenix.Controller.GameController _gameCntrlr;
        int _btnIdx;
        bool _isDown = false;

        public ButtonMonitor(CTRE.Phoenix.Controller.GameController gameCntrlr, int btnIdx, ButtonPressEventHandler handler)
        {
            _gameCntrlr = gameCntrlr;
            _btnIdx = btnIdx;
            ButtonPress = handler;
        }
        public void Process()
        {
            bool down = _gameCntrlr.GetButton((uint)_btnIdx);

            if (!_isDown && down)
                ButtonPress(_btnIdx, down);

            _isDown = down;
        }

        public void OnStart()
        {
        }

        public void OnLoop()
        {
            Process();
        }

        public bool IsDone()
        {
            return false;
        }

        public void OnStop()
        {
        }
    }
}
