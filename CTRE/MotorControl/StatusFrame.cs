using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.MotorControl
{
    public enum StatusFrameEnhanced
    {
        Status_1_General = 0x1400,
        Status_2_Feedback = 0x1440,
        Status_4_AinTempVbat = 0x14C0,
        Status_6_Misc = 0x1540,
        Status_7_CommStatus = 0x1580,
        Status_9_MotProfBuffer= 0x1600,
        Status_10_MotionMagic = 0x1640,
        Status_12_RobotPose = 0x16C0,
        Status_13_Base_PIDF1 = 0x1700,
        Status_14_Turn_PIDF2 = 0x1740,
        Status_15_FirmareApiStatus= 0x1780,

        Status_3_Quadrature = 0x1480,
        Status_8_PulseWidth = 0x15C0,
        Status_11_UartGadgeteer = 0x1680,
    };

    public enum StatusFrame
    {
        Status_1_General = 0x1400,
        Status_2_Feedback = 0x1440,
        Status_4_AinTempVbat = 0x14C0,
        Status_6_Misc = 0x1540,
        Status_7_CommStatus = 0x1580,
        Status_9_MotProfBuffer = 0x1600,
        Status_10_MotionMagic = 0x1640,
        Status_12_RobotPose = 0x16C0,
        Status_13_Base_PIDF1 = 0x1700,
        Status_14_Turn_PIDF2 = 0x1740,
        Status_15_FirmareApiStatus = 0x1780,
    }
    public static class StatusFrameRoutines
    {
        public static StatusFrameEnhanced Promote(StatusFrame statusFrame)
        {
            return (StatusFrameEnhanced)statusFrame;
        }
    }
}
