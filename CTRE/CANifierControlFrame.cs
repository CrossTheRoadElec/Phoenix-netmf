using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix
{
    /** Enumerated type for status frame types. */
    public enum CANifierControlFrame
    {
        CANifier_Control_1_General = 0x03040000,
        CANifier_Control_2_PwmOutput = 0x03040040,
    };
}
