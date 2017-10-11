using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.Tasking
{
    public abstract class ScheduledStateMachine : StateMachine, ILoopable
    {
        bool _running;
        Enum _initalState;

        public ScheduledStateMachine(Enum initalState) : base(initalState)
        {
            _initalState = initalState;
        }

        public bool IsDone()
        {
            return false;
        }

        public void OnLoop()
        {
            if(_running)
                this.Process();
        }

        public void OnStart()
        {
            ChangeState(_initalState, 0);
            _running = true;
        }

        public void OnStop()
        {
            _running = false;
        }
    }
}