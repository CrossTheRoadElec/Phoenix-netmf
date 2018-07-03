//using System;
//using Microsoft.SPOT;
//using CTRE.Phoenix.Tasking;
//using CTRE.Phoenix.Mechanical;
//using CTRE.Phoenix.MotorControllers;

//namespace CTRE.Phoenix.Motion
//{
//    public class ServoVelocity : ILoopable
//    {
//        SensoredGearbox Gearbox;
//        float _targetVelocity = 0;
        
//        /* SmartMotorController Constructor */
//        public ServoVelocity(SmartMotorController motor, FeedbackDevice feedbackDevice, float velocity)
//        {
//            SensoredGearbox temp1 = new SensoredGearbox(1, motor, feedbackDevice);
//            Gearbox = temp1;
//            _targetVelocity = velocity;
//        }
//        /* SensoredGearbox Constructor */
//        public ServoVelocity(SensoredGearbox gearbox, float velocity)
//        {
//            Gearbox = gearbox;
//            _targetVelocity = velocity;
//        }

//        public void OnStart()
//        {
//        }

//        public void OnStop()
//        {
//            /* stop the talon */
//            Gearbox.SetControlMode(ControlMode.PercentOutput);
//            Gearbox.Set(0);
//        }

//        public bool IsDone()
//        {
//            return false;
//        }

//        public void Set(float targetSpeed, float maxOutput)
//        {
//            Gearbox.ConfigPeakOutputVoltage(maxOutput, -maxOutput);
//            Gearbox.SetControlMode(ControlMode.Velocity);
//            Gearbox.Set(targetSpeed);
//        }

//        public void OnLoop()
//        {
//            Gearbox.SetControlMode(ControlMode.Velocity);
//            Gearbox.Set(_targetVelocity);
//        }
//    }
//}
