using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.Drive
{
    public class Styles
    {
        /**
         * Drive styles made available by adding sensors 
         */
        public enum AdvancedStyle
        {
            PercentOutput = 0,
            Position = 1,
            VelocityClosedLoop = 2,
            Follower = 5,
            MotionProfile = 6,
            MotionMagic = 7,
            MotionMagicArc = 8,
            TimedPercentOutput = 9,

            Disabled = 15,
        }
        /**
         * Drive styles that are available when there are no sensors.
         */
        public enum BasicStyle
        {
            PercentOutput = 0,
            TimedPercentOutput = 9,
        }

        internal static class Routines
        {
            public static Styles.AdvancedStyle Promote(BasicStyle basicStyle) { return (Styles.AdvancedStyle)basicStyle; }

            public static MotorControl.ControlMode LookupCM(AdvancedStyle basicStyle) { return (MotorControl.ControlMode)(basicStyle); }
            public static MotorControl.ControlMode LookupCM(BasicStyle basicStyle) { return (MotorControl.ControlMode)(basicStyle); }
        }
    }
}
