using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.MotorControl
{
    public enum VelocityMeasPeriod
    {
        Period_1Ms = 1,
        Period_2Ms = 2,
        Period_5Ms = 5,
        Period_10Ms = 10,
        Period_20Ms = 20,
        Period_25Ms = 25,
        Period_50Ms = 50,
        Period_100Ms = 100,
    };
    public class VelocityMeasPeriodRoutines {
        public static string ToString(VelocityMeasPeriod value) {
            switch(value) {
                case VelocityMeasPeriod.Period_1Ms : return "VelocityMeasPeriod.Period_1Ms";
                case VelocityMeasPeriod.Period_2Ms : return "VelocityMeasPeriod.Period_2Ms";
                case VelocityMeasPeriod.Period_5Ms : return "VelocityMeasPeriod.Period_5Ms";
                case VelocityMeasPeriod.Period_10Ms : return "VelocityMeasPeriod.Period_10Ms";
                case VelocityMeasPeriod.Period_20Ms : return "VelocityMeasPeriod.Period_20Ms";
                case VelocityMeasPeriod.Period_25Ms : return "VelocityMeasPeriod.Period_25Ms";
                case VelocityMeasPeriod.Period_50Ms : return "VelocityMeasPeriod.Period_50Ms";
                case VelocityMeasPeriod.Period_100Ms : return "VelocityMeasPeriod.Period_100Ms";
                default : return "InvalidValue";
            }
        }
    };

}
