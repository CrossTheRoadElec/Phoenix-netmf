using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.MotorControl
{
    public enum SensorTerm
    {
        SensorTerm_Sum0,
        SensorTerm_Sum1,
        SensorTerm_Diff0,
        SensorTerm_Diff1,
    };

    public class SensorTermRoutines {
        public static string ToString(SensorTerm value) {
            switch(value) {
                case SensorTerm.Sum0 : return "SensorTerm.Sum0";
                case SensorTerm.Sum1 : return "SensorTerm.Sum1";
                case SensorTerm.Diff0 : return "SensorTerm.Diff0";
                case SensorTerm.Diff1 : return "SensorTerm.Diff1";
                default : return "InvalidValue";
            }
        }
    };

}

