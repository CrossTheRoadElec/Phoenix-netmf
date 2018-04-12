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
        Current = 3,
        Follower = 5,
        MotionProfile = 6,
        MotionMagic = 7,
        MotionProfileArc = 10,

        Disabled = 15,
    };
}