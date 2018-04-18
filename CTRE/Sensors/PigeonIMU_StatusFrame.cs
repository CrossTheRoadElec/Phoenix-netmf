using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.Sensors
{
    /** Enumerated type for status frame types. */
    public enum PigeonIMU_StatusFrame
    {
        CondStatus_1_General = 0x042000,
        CondStatus_9_SixDeg_YPR = 0x042200,
        CondStatus_6_SensorFusion = 0x042140,
        CondStatus_11_GyroAccum = 0x042280,
        CondStatus_2_GeneralCompass = 0x042040,
        CondStatus_3_GeneralAccel = 0x042080,
        CondStatus_10_SixDeg_Quat = 0x042240,
        RawStatus_4_Mag = 0x041CC0,
        BiasedStatus_2_Gyro = 0x041C40,
        BiasedStatus_4_Mag = 0x041CC0,
        BiasedStatus_6_Accel = 0x41D40,
    };
}
