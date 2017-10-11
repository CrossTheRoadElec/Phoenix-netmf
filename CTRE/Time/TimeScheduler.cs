using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix
{
    public class PeriodicTimeout
    {
        long _periodMs; //!< Holds the target time period to fire scheduled events
        long _lastTime = 0; //!< Time at which last event fired.
        long _tenTimesPeriodMs; //!< 10X the target time period, if we fall to far ahead or behind, rearm everything cleanly to robustly protect against clock resets.
        bool _error = false; //!< Unused flag for debugging purposes

        public PeriodicTimeout(long periodMs)
        {
            Restart(periodMs);
        }
        public void Restart(long periodMs)
        {
            if (periodMs < 1)
                periodMs = 1;

            _periodMs = periodMs;
            _tenTimesPeriodMs = 10 * periodMs;
            _lastTime = GetMs();
        }

        long GetMs()
        {
            long now = DateTime.Now.Ticks;
            now /= 10000; //100ns per unit
            return now;
        }

        public bool Process()
        {
            bool retval = false;

            long t = GetMs();
            long delta = t - _lastTime;
            if ((delta > _tenTimesPeriodMs) || (delta < -_tenTimesPeriodMs))
            {
                /* something is wrong, signal a time out, and rearm cleanly for next timeout */
                _lastTime = GetMs();
                retval = true;
                /* for now note the event, wire this to something if timing issues ensue */
                _error = true;
                _error = (_error == true); // removes unused warning
            }
            else if (delta >= _periodMs)
            {
                /* move up last time by one timeout period, this assumes we never fall to far behind, also this gives us ideal mean time */
                _lastTime += _periodMs;
                retval = true;
            }

            return retval;
        }
    }
}