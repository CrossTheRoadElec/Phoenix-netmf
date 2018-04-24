using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix
{
    /** Enumerated type for status frame types. */
    public enum CANifierStatusFrame
    {
        Status_1_General = 0x041400,
        Status_2_General = 0x041440,
        Status_3_PwmInputs0 = 0x041480,
        Status_4_PwmInputs1 = 0x0414C0,
        Status_5_PwmInputs2 = 0x041500,
        Status_6_PwmInputs3 = 0x041540,
        Status_8_Misc = 0x0415C0,
    };
}
