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
    public class SensoredGearbox : RemoteSensoredGearbox
    {
        private new IMotorControllerEnhanced _master;

        //--------------------- Constructors -----------------------------//
        public SensoredGearbox(float unitsPerRevolution, IMotorControllerEnhanced master, FeedbackDevice feedbackDevice) : base(unitsPerRevolution, master)
        {
            _master = master;
            _master.ConfigSelectedFeedbackSensor(feedbackDevice);
        }

        public SensoredGearbox(float unitsPerRevolution, IMotorControllerEnhanced master, IFollower[] followers, FeedbackDevice feedbackDevice) : base(unitsPerRevolution, master, followers)
        {
            _master = master;
            _master.ConfigSelectedFeedbackSensor(feedbackDevice);
        }

        public SensoredGearbox(float unitsPerRevolution, IMotorControllerEnhanced master, RemoteFeedbackDevice remoteFeedbackDevice) : base(unitsPerRevolution, master, remoteFeedbackDevice)
        {
            _master = master;
            /* parent class selects sensor */
        }

        public SensoredGearbox(float unitsPerRevolution, IMotorControllerEnhanced master, IFollower[] followers, RemoteFeedbackDevice remoteFeedbackDevice) : base(unitsPerRevolution, master, followers, remoteFeedbackDevice)
        {
            _master = master;
            /* parent class selects sensor */
        }

        public SensoredGearbox(float unitsPerRevolution, IMotorControllerEnhanced mc0, IFollower mc1, RemoteFeedbackDevice remoteFeedbackDevice)
            : this(unitsPerRevolution, mc0, new IFollower[] { mc1 }, remoteFeedbackDevice) { }
        public SensoredGearbox(float unitsPerRevolution, IMotorControllerEnhanced mc0, IFollower mc1, IFollower mc2, RemoteFeedbackDevice remoteFeedbackDevice)
            : this(unitsPerRevolution, mc0, new IFollower[] { mc1, mc2 }, remoteFeedbackDevice) { }

        public SensoredGearbox(float unitsPerRevolution, IMotorControllerEnhanced mc0, IFollower mc1, FeedbackDevice feedbackDevice)
            : this(unitsPerRevolution, mc0, new IFollower[] { mc1 }, feedbackDevice) { }
        public SensoredGearbox(float unitsPerRevolution, IMotorControllerEnhanced mc0, IFollower mc1, IFollower mc2, FeedbackDevice feedbackDevice)
            : this(unitsPerRevolution, mc0, new IFollower[] { mc1, mc2 }, feedbackDevice) { }

        //------ Set output routines. ----------//
        /* done in parent */

        //------ Invert behavior ----------//
        /* done in parent */

        //----- general output shaping ------------------//
        /* done in parent */

        //------ Voltage Compensation ----------//
        /* done in parent */

        //------ General Status ----------//
        /* done in parent */

        //------ sensor selection ----------//


        //------ ??? ----------//
        //      ErrorCode SelectFeedbackSensor(FeedbackDevice feedbackDevice, int timeoutMs = 0) 
        //{ 
        //	return _motor.SelectFeedbackSensor(feedbackDevice, timeoutMs);
        //}
        //      ErrorCode ConfigSensorIsContinuous(bool isContinuous, int timeoutMs = 0)
        //{
        //	return _motor.ConfigSensorIsContinuous(isContinuous, timeoutMs);
        //}
        //      ErrorCode ConfigAutoZeroSensor(ZeroSensorCriteria zeroSensorCriteria, int timeoutMs = 0)
        //{ 
        //	return _motor.ConfigAutoZeroSensor(zeroSensorCriteria, timeoutMs);
        //      }

        //------- sensor status --------- //
        /* done in parent except SetPosition is unique for local sensors */
        public override ErrorCode SetPosition(float sensorPos, int timeoutMs = 0)
        {
            return _master.SetSelectedSensorPosition((int)(_unitsPerRevolution * sensorPos), timeoutMs);
        }

        //----- velocity signal conditionaing ------//
        ErrorCode ConfigVelocityMeasurementPeriod(VelocityMeasPeriod period, int timeoutMs = 0)
        {
            return _master.ConfigVelocityMeasurementPeriod(period, timeoutMs);
        }
        ErrorCode ConfigVelocityMeasurementWindow(int windowSize, int timeoutMs = 0)
        {
            return _master.ConfigVelocityMeasurementWindow(windowSize, timeoutMs);
        }

        ////------ Current Lim ----------//
        //ErrorCode ConfigCurrentLimit(uint amps, uint timeoutMs = 0)
        //{
        //    return _master.ConfigCurrentLimit(amps, timeoutMs); // only if single Talon works
        //}
        //void EnableCurrentLimit(bool enable)
        //{
        //    _master.EnableCurrentLimit(enable);
        //}

        //------ Motion Profile Settings used in Motion Magic and Motion Profile ----------//
        /* done in parent */

        //------ Motion Profile Buffer ----------//
        /* done in parent */
    }
}
