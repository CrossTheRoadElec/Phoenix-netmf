using System;
using Microsoft.SPOT;
using CTRE.Phoenix.MotorControllers;

namespace CTRE.Phoenix.Mechanical
{
    public class AndMark_PickOne : SensoredGearbox
    {
        public enum Reduction
        {
            Reduction_1_0,
            Reduction_1_5,
            Reduction_2_0,
        };

        private static float RtoI(Reduction r)
        {
            switch(r)
            {
                case Reduction.Reduction_1_0:
                    return 1f;
                case Reduction.Reduction_1_5:
                    return 1.5f;
                default:
                case Reduction.Reduction_2_0:
                    return 2f;
            }
        }

        /**
         * @param gearRatio ratio between the motor output and the final geared output. 
         * Typically a reduction, example: ToDO
         * @param integratedEncoderRatio ratio between the motor output and the sensor output.
         * Typically a reduction, example: ToDO
         */
        public AndMark_PickOne(TalonSrx talon, Reduction reduction)
            : base(RtoI(reduction), talon, TalonSrx.FeedbackDevice.QuadEncoder)
        {
        }
    }
}
