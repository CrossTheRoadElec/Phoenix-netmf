using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.Tasking
{
    public abstract class StateMachine : IProcessable
    {
        public Enum CurrentState { get; private set; }

        public int TimeInStateMs
        {
            get
            {
                return (int)_stopWatch.DurationMs;
            }
        }

        uint _stateTimeout; //!< How long to wait before calling ProcessState().

        Stopwatch _stopWatch;

        public StateMachine(Enum initalState)
        {
            CurrentState = initalState;
            _stateTimeout = 0;
            _stopWatch = new Stopwatch();
            _stopWatch.Start();
        }
        public void Process()
        {
            if (_stopWatch.DurationMs >= _stateTimeout)
            {
                ProcessState(CurrentState, TimeInStateMs);
            }
        }

        public void ChangeState(Enum newState, UInt16 newTimeout = 0)
        {
			/* did the state change? */
            bool stateChanged = (CurrentState != newState);
			/* save current state and timeout */
            CurrentState = newState;
            _stateTimeout = newTimeout;
            /* restart time-in-state stopwatch iff state has changed */
            if (stateChanged) { _stopWatch.Start(); }
        }

        /**
         * Child class implements this 
         */
        public abstract void ProcessState(Enum currentState, int timeInStateMs);
    }
}