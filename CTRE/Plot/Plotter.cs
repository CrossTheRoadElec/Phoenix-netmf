using System;
using Microsoft.SPOT;
using CTRE.Phoenix.Signals;
using CTRE.Phoenix.Tasking;

namespace CTRE.Phoenix.Plot
{
    /* smartdash plotting is garbage, we need something good */
    public class Plotter : IProcessable, ILoopable
    {
        /* find a good PC-side 2D plotter that is cross-platform.  Qt looks pretty good.  Maybe Xamarin has someting.
         * Android/iOS would be useful
         * Try UDP and send timestamped values */
        Plotter()
        {

        }

        void Add(String key, IInputSignal sig)
        {

        }
        // --- ILoopable --- //
        public bool IsDone()
        {
            throw new NotImplementedException();
        }

        public void OnLoop()
        {
            throw new NotImplementedException();
        }

        public void OnStart()
        {
            throw new NotImplementedException();
        }

        public void OnStop()
        {
            throw new NotImplementedException();
        }
        // --- IProcessable --- //
        public void Process()
        {
            throw new NotImplementedException();
        }

    }
}
