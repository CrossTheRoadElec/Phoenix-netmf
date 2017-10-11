using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix
{
    public class Stopwatch
    {
        private long _t0 = 0;
        private long _t1 = 0;
        private float _scalar = 0.001f / TimeSpan.TicksPerMillisecond;
        public void Start()
        {
            _t0 = DateTime.Now.Ticks;
        }
        public float Duration
        {
            get
            {
                _t1 = DateTime.Now.Ticks;
                long retval = _t1 - _t0;
                if (retval < 0)
                    retval = 0;
                return retval * _scalar;
            }
        }
        public uint DurationMs
        {
            get
            {
                return (uint)(Duration * 1000);
            }
        }

        public String Caption
        {
            get
            {
                float timeS = Duration;
                if (timeS < 0.000001)
                {
                    return "" + (int)(timeS * 1000 * 1000) + " us";
                }
                else if (timeS < 0.001)
                {
                    return "" + (int)(timeS * 1000) + " ms";
                }
                return "" + (int)timeS + " sec";
            }
        }
    }
}