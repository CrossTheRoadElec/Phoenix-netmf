using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.MotorControllers
{
    public enum ControlMode
    {
        kPercentVbus = 0,
        kCurrent = 1,
        kSpeed = 2,
        kPosition = 3,
        kVoltage = 4,
        kFollower = 5,
        kMotionProfile = 6,
        kMotionMagic = 7,
    };

    public enum BasicControlMode
    {
        kPercentVbus = 0,
        kVoltage = 4,
        kFollower = 5,
    }
    public static class ControlModeRoutines {
        public static ControlMode Promote(BasicControlMode basicControlMode)
        {
            return (ControlMode)basicControlMode;
        }
    }
}