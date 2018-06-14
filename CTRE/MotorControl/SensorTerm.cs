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
                case SensorTerm.SensorTerm_Sum0 : return "SensorTerm.SensorTerm_Sum0";
                case SensorTerm.SensorTerm_Sum1 : return "SensorTerm.SensorTerm_Sum1";
                case SensorTerm.SensorTerm_Diff0 : return "SensorTerm.SensorTerm_Diff0";
                case SensorTerm.SensorTerm_Diff1 : return "SensorTerm.SensorTerm_Diff1";
                default : return "InvalidValue";
            }
        }
    };

}

