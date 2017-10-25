using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.MotorControl
{
    public enum ControlFrame
    {
        Control_2_Enable_50m = 0x040040,
        Control_3_General = 0x040080,
        Control_4_Advanced = 0x0400C0,
        Control_6_MotProfAddTrajPoint = 0x040140,
    };

    public enum ControlFrameEnhanced
    {
        Control_2_Enable_50m = 0x040040,
        Control_3_General = 0x040080,
        Control_4_Advanced = 0x0400c0,
        Control_5_FeedbackOutputOverride = 0x040100,
        Control_6_MotProfAddTrajPoint = 0x040140,
    }
    public static class ControlFrameRoutines
    {
        public static ControlFrameEnhanced Promote(ControlFrame controlFrame)
        {
            return (ControlFrameEnhanced)controlFrame;
        }
    }
}
