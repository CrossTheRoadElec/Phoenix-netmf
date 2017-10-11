using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.Drive
{
    public class Styles
    {
        public enum Smart
        {
            Voltage = 0,
            PercentOutput = 1,
            VelocityClosedLoop = 2,
        }

        public enum Basic
        {
            Voltage = 0,
            PercentOutput = 1,
        }
        public static class StylesRoutines
        {
            public static Styles.Smart Promote(Basic basicStyle)
            {
                return (Styles.Smart)basicStyle;
            }
        }
    }
}
