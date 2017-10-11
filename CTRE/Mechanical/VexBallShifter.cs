using System;
using Microsoft.SPOT;
using CTRE.Phoenix.MotorControllers;

namespace CTRE.Phoenix.Mechanical
{
    public class VexBallShifter : SensoredGearbox
    {
        /** 1 CIM */
        public VexBallShifter(float gearRatio, TalonSrx talon, PneumaticControlModule pcm, int solenoidChannel)
            : base(gearRatio, talon, TalonSrx.FeedbackDevice.CtreMagEncoder_Relative)
        {
        }
        /** 2 CIM */
        public VexBallShifter(float gearRatio, TalonSrx talon, IFollower f1, PneumaticControlModule pcm, int solenoidChannel)
            : base(gearRatio, talon, TalonSrx.FeedbackDevice.CtreMagEncoder_Relative)
        {
        }
        /** 3 CIM */
        public VexBallShifter(float gearRatio, TalonSrx talon, IFollower f1, IFollower f2, PneumaticControlModule pcm, int solenoidChannel)
            : base(gearRatio, talon, TalonSrx.FeedbackDevice.CtreMagEncoder_Relative)
        {
        }


    }
}
