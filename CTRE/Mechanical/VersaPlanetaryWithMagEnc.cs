using System;
using Microsoft.SPOT;
using CTRE.Phoenix.MotorControl;
using CTRE.Phoenix.MotorControl.CAN;

namespace CTRE.Phoenix.Mechanical
{
    public class VersaPlanetaryWithMagEnc : SensoredGearbox
    {
        /**
         * @param gearRatio ratio between the motor output and the final geared output. 
         * Typically a reduction, example: ToDO
         * @param integratedEncoderRatio ratio between the motor output and the sensor output.
         * Typically a reduction, example: ToDO
         */

        public VersaPlanetaryWithMagEnc(float slice1, TalonSRX talon)
            : base(slice1, talon, FeedbackDevice.QuadEncoder)
        {
        }
        public VersaPlanetaryWithMagEnc(float slice1, float slice2, TalonSRX talon)
            : base(slice1 * slice2, talon, FeedbackDevice.QuadEncoder)
        {
        }
        public VersaPlanetaryWithMagEnc(float slice1, float slice2, float slice3, TalonSRX talon)
            : base(slice1 * slice2 * slice3, talon, FeedbackDevice.QuadEncoder)
        {
        }
    }
}
