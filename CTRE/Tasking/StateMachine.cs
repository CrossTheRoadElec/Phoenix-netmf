using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.Tasking
{
    public abstract class StateMachine : IProcessable
    {
        Enum _state; //!< The current state the SM is in.
        uint _stateTimeout; //!< How long to wait before calling ProcessState().

        Stopwatch _stopWatch;

        public StateMachine(Enum initalState)
        {
            _state = initalState;
            _stateTimeout = 0;
            _stopWatch = new Stopwatch();
            _stopWatch.Start();
        }
        public void Start()
        {

        }
        public void Process()
        {
            if (_stopWatch.DurationMs >= _stateTimeout)
            {
                ProcessState(_state, (int)_stopWatch.DurationMs);
            }
        }

        public void ChangeState(Enum newState, UInt16 newTimeout = 0)
        {
            _state = newState;
            _stateTimeout = newTimeout;
            _stopWatch.Start();
        }

        /**
         * Child class implements this 
         */
        public abstract void ProcessState(Enum currentState, int timeInStateMs);
    }
}