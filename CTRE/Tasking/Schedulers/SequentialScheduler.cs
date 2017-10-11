using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.Tasking
{
    public class SequentialScheduler : IProcessable, ILoopable
    {
        bool _running = false;
        System.Collections.ArrayList _loops = new System.Collections.ArrayList();
        int _periodMs;
        PeriodicTimeout _timeout;
        int _idx;

        public SequentialScheduler(int periodMs)
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
            _idx = 0;
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
            if (_idx < _loops.Count)
            {
                if (_running && _timeout.Process())
                {
                    ILoopable loop = (ILoopable)_loops[_idx];
                    loop.OnLoop();
                    if (loop.IsDone())
                    {
                        ++_idx;
                    }
                }
            }
            else
            {
                _running = false;
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
            /* Have to return something to know if we are done */
            if (_running == false)
                return true;
            else
                return false;
        }

        public void OnStop()
        {
            Stop();
        }
    }
}