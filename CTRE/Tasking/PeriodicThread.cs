using System;
using Microsoft.SPOT;
using System.Threading;

namespace CTRE.Phoenix.Tasking
{
    public class PeriodicThread
    {
        Thread _thread = null;
        int _periodMs;
        IProcessable _processable;
        ILoopable _loopable;

        public PeriodicThread(int periodMs, IProcessable processable)
        {
            _periodMs = periodMs;
            _processable = processable;
            _loopable = null;
        }
        public PeriodicThread(int periodMs, ILoopable loopable)
        {
            _periodMs = periodMs;
            _processable = null;
            _loopable = loopable;
        }
        public PeriodicThread(int periodMs, IProcessable processable, ILoopable loopable)
        {
            _periodMs = periodMs;
            _processable = processable;
            _loopable = loopable;
        }

        public void Start()
        {
            if (_thread == null)
            {
                _thread = new Thread(OnThread);
                _thread.Start();
            }
        }
        void OnThread()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(_periodMs);

                if(_processable != null)
                {
                    _processable.Process();
                }
                else if (_loopable != null)
                {
                    _loopable.OnLoop();
                }
            }
        }
    }
}