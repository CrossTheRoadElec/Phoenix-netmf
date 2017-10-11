using System;
using Microsoft.SPOT;
using System.Collections;
using CTRE.Phoenix.Tasking;

namespace CTRE.Phoenix.Motion
{
    public abstract class AbstractMotionProfilerFeeder : ILoopable, IProcessable
    {
        ArrayList _points = new ArrayList();

        public AbstractMotionProfilerFeeder()
        {

        }

        public void Add(TrajectoryPoint trajPt)
        {
            _points.Add(trajPt);
        }

        public void RemoveAll()
        {
            _points.Clear();
        }

        int Count
        {
            get
            {
                return _points.Count;
            }
        }
        public TrajectoryPoint this[int index]
        {
            get
            {
                return (TrajectoryPoint)_points[index];
            }
        }

        public abstract void Start();
        public abstract void Stop();
        public abstract void Process();


        //--- Loopable ---//
        public bool IsDone()
        {
            return false;
        }

        public void OnLoop()
        {
            Process();
        }

        public void OnStart()
        {
            Start();
        }

        public void OnStop()
        {
            Stop();
        }

    }
}
