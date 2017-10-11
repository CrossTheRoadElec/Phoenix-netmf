using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.Tasking
{
    public class ConcurrentScheduler : IProcessable, ILoopable
    {
        bool _running = false;
        System.Collections.ArrayList _loops = new System.Collections.ArrayList();
        int _periodMs;
        PeriodicTimeout _timeout;

        public ConcurrentScheduler(int periodMs)
        {
            _periodMs = periodMs;
            _timeout = new PeriodicTimeout(periodMs);
        }
        public void Add(ILoopable aLoop)
        {
            _loops.Add(aLoop);
        }

        public ILoopable GetCurrent()
        {
            return null;
        }

        public void RemoveAll()
        {
            _loops.Clear();
        }

        public void Start()
        {
            foreach (ILoopable lp in _loops)
            {
                lp.OnStart();
            }
            _running = true;
        }
        public void Stop()
        {
            foreach (ILoopable lp in _loops)
            {
                lp.OnStop();
            }
            _running = false;
        }
        public void Process()
        {
            if (_timeout.Process())
            {
                foreach (ILoopable lp in _loops)
                {
                    lp.OnLoop();
                }
            }
        }
        //--- Loopable ---/
        public void OnStart()
        {
            Start();
        }

        public void OnLoop()
        {
            Process();
        }

        public bool IsDone()
        {
            /* Should this be this? */
            //return _isRunning;
            return false;
        }

        public void OnStop()
        {
            Stop();
        }
    }
}