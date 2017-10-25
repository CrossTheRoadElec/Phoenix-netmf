using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.MotorControl
{
    /**
     * Common Control Modes for all CTRE Motor Controllers.
     **/
    public enum ControlMode
    {
        PercentOutput = 0,
        Position = 1,
        Velocity = 2,
        Follower = 5,
        MotionProfile = 6,
        MotionMagic = 7,
        MotionMagicArc = 8,
        TimedPercentOutput = 9,

        Disabled = 15,
    };
}