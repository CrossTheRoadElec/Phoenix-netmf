using System;
using Microsoft.SPOT;
using CTRE.Phoenix.MotorControllers;

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
  
        public VersaPlanetaryWithMagEnc(float slice1, TalonSrx talon)
            : base(slice1, talon, TalonSrx.FeedbackDevice.CtreMagEncoder_Relative)
        {
        }
        public VersaPlanetaryWithMagEnc(float slice1, float slice2, TalonSrx talon)
            : base(slice1 * slice2, talon, TalonSrx.FeedbackDevice.CtreMagEncoder_Relative)
        {
        }
        public VersaPlanetaryWithMagEnc(float slice1, float slice2, float slice3, TalonSrx talon)
            : base(slice1 * slice2 * slice3, talon, TalonSrx.FeedbackDevice.CtreMagEncoder_Relative)
        {
        }
    }
}
