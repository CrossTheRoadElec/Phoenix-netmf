//using System;
//using Microsoft.SPOT;
//using CTRE.MotorControllers;

//namespace CTRE.Phoenix.Mechanical
//{
//    public class VexBallShifter : SensoredGearbox
//    {
//        /** 1 CIM */
//        public VexBallShifter(float gearRatio, TalonSRX talon, PneumaticControlModule pcm, int solenoidChannel)
//            : base(gearRatio, talon, TalonSRX.FeedbackDevice.CtreMagEncoder_Relative)
//        {
//        }
//        /** 2 CIM */
//        public VexBallShifter(float gearRatio, TalonSRX talon, IFollower f1, PneumaticControlModule pcm, int solenoidChannel)
//            : base(gearRatio, talon, TalonSRX.FeedbackDevice.CtreMagEncoder_Relative)
//        {
//        }
//        /** 3 CIM */
//        public VexBallShifter(float gearRatio, TalonSRX talon, IFollower f1, IFollower f2, PneumaticControlModule pcm, int solenoidChannel)
//            : base(gearRatio, talon, TalonSRX.FeedbackDevice.CtreMagEncoder_Relative)
//        {
//        }


//    }
//}
