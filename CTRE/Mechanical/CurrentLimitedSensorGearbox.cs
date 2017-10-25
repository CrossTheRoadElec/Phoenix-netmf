using System;
using Microsoft.SPOT;
using CTRE.Phoenix.Drive;
using CTRE.Phoenix.MotorControl;

namespace CTRE.Phoenix.Mechanical
{
    /**
     * Represents a mechanical system whereby a number of motor controllers is connected together via a gearbox.
     * One of the motor controllers must be a Talon SRX, and it must use the feedback connector.
     */
    public class CurrentLimitedSensorGearbox : SensoredGearbox
    {
        private new IMotorControllerEnhanced _master;

        //--------------------- Constructors -----------------------------//
        public CurrentLimitedSensorGearbox(float unitsPerRevolution, IMotorControllerEnhanced master, FeedbackDevice feedbackDevice) : base(unitsPerRevolution, master, feedbackDevice)
        {
        }

        public CurrentLimitedSensorGearbox(float unitsPerRevolution, IMotorControllerEnhanced master, IMotorControllerEnhanced[] followers, FeedbackDevice feedbackDevice) : base(unitsPerRevolution, master, followers, feedbackDevice)
        {
        }

        public CurrentLimitedSensorGearbox(float unitsPerRevolution, IMotorControllerEnhanced master, RemoteFeedbackDevice remoteFeedbackDevice) : base(unitsPerRevolution, master, remoteFeedbackDevice)
        {
        }

        public CurrentLimitedSensorGearbox(float unitsPerRevolution, IMotorControllerEnhanced master, IMotorControllerEnhanced[] followers, RemoteFeedbackDevice remoteFeedbackDevice) : base(unitsPerRevolution, master, followers, remoteFeedbackDevice)
        {
        }

        public CurrentLimitedSensorGearbox(float unitsPerRevolution, IMotorControllerEnhanced mc0, IMotorControllerEnhanced mc1, RemoteFeedbackDevice remoteFeedbackDevice)
            : base(unitsPerRevolution, mc0, mc1, remoteFeedbackDevice) { }
        public CurrentLimitedSensorGearbox(float unitsPerRevolution, IMotorControllerEnhanced mc0, IMotorControllerEnhanced mc1, IMotorControllerEnhanced mc2, RemoteFeedbackDevice remoteFeedbackDevice)
            : base(unitsPerRevolution, mc0, mc1, mc2 , remoteFeedbackDevice) { }

        public CurrentLimitedSensorGearbox(float unitsPerRevolution, IMotorControllerEnhanced mc0, IMotorControllerEnhanced mc1, FeedbackDevice feedbackDevice)
            : base(unitsPerRevolution, mc0, mc1, feedbackDevice) { }
        public CurrentLimitedSensorGearbox(float unitsPerRevolution, IMotorControllerEnhanced mc0, IMotorControllerEnhanced mc1, IMotorControllerEnhanced mc2, FeedbackDevice feedbackDevice)
            : base(unitsPerRevolution, mc0, mc1, mc2 , feedbackDevice) { }

        //------ Current Lim ----------//
        ErrorCode ConfigPeakCurrentLimit(int amps, int timeoutMs = 0)
        {
            return _master.ConfigPeakCurrentLimit(amps, timeoutMs);
        }
        ErrorCode ConfigPeakCurrentDuration(int milliseconds, int timeoutMs = 0)
        {
            return _master.ConfigPeakCurrentDuration(milliseconds, timeoutMs);
        }
        ErrorCode ConfigContinuousCurrentLimit(int amps, int timeoutMs = 0)
        {
            return _master.ConfigContinuousCurrentLimit(amps, timeoutMs);
        }
        void EnableCurrentLimit(bool enable)
        {
            _master.EnableCurrentLimit(enable);
        }
    }
}
