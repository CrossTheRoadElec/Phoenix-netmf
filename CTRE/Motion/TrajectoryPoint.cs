using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.Motion
{
    /**
     * Motion Profile Trajectory Point
     * This is simply a data transer object.
     */
    public struct TrajectoryPoint
    {
        public float position; //!< The position to servo to.
        public float velocity; //!< The velocity to feed-forward.
                               /**
                                * Time in milliseconds to process this point.
                                * Value should be between 1ms and 255ms.  If value is zero
                                * then Talon will default to 1ms.  If value exceeds 255ms API will cap it.
                                */
        public UInt32 timeDurMs;
        /**
         * Which slot to get PIDF gains.
         * PID is used for position servo.
         * F is used as the Kv constant for velocity feed-forward.
         * Typically this is hardcoded to the a particular slot, but you are free
         * gain schedule if need be.
         */
        public UInt32 profileSlotSelect;
        /**
         * Set to true to only perform the velocity feed-forward and not perform
         * position servo.  This is useful when learning how the position servo
         * changes the motor response.  The same could be accomplish by clearing the
         * PID gains, however this is synchronous the streaming, and doesn't require restoing
         * gains when finished.
         *
         * Additionaly setting this basically gives you direct control of the motor output
         * since motor output = targetVelocity X Kv, where Kv is our Fgain.
         * This means you can also scheduling straight-throttle curves without relying on
         * a sensor.
         */
        public bool velocityOnly;
        /**
         * Set to true to signal Talon that this is the final point, so do not
         * attempt to pop another trajectory point from out of the Talon buffer.
         * Instead continue processing this way point.  Typically the velocity
         * member variable should be zero so that the motor doesn't spin indefinitely.
         */
        public bool isLastPoint;
        /**
          * Set to true to signal Talon to zero the selected sensor.
          * When generating MPs, one simple method is to make the first target position zero,
          * and the final target position the target distance from the current position.
          * Then when you fire the MP, the current position gets set to zero.
          * If this is the intent, you can set zeroPos on the first trajectory point.
          *
          * Otherwise you can leave this false for all points, and offset the positions
          * of all trajectory points so they are correct.
          */
        public bool zeroPos;

        public float headingDeg;
    };
}
