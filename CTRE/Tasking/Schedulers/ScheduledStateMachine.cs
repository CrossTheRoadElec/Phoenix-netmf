using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.Tasking
{
    public abstract class ScheduledStateMachine : StateMachine, ILoopable
    {
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
            this.Process();
        }

        public void OnStart()
        {
            ChangeState(_initalState, 0);
        }

        public void OnStop()
        {
        }
    }
}