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
    public class RemoteSensoredGearbox : Gearbox
    {
        protected float _unitsPerRevolution, _rotationsPerUnit;
        protected const float kDiv_600 = 0.00166666666666666666666666666667f;
        protected RemoteFeedbackDevice _remoteFeedbackDevice;

        //--------------------- Constructors -----------------------------//
        public RemoteSensoredGearbox(float unitsPerRevolution, IMotorController mc0, RemoteFeedbackDevice remoteFeedbackDevice) : base (mc0)
        {
            _unitsPerRevolution = unitsPerRevolution;
            _rotationsPerUnit = 1f / _unitsPerRevolution;
            _remoteFeedbackDevice = remoteFeedbackDevice;

            _motor.ConfigSelectedFeedbackSensor(_remoteFeedbackDevice, 0);
        }

        public RemoteSensoredGearbox(float unitsPerRevolution, IMotorController master, IFollower[] followers, RemoteFeedbackDevice remoteFeedbackDevice) : base(master, followers)
        {
            _unitsPerRevolution = unitsPerRevolution;
            _rotationsPerUnit = 1f / _unitsPerRevolution;
            _remoteFeedbackDevice = remoteFeedbackDevice;

            _motor.ConfigSelectedFeedbackSensor(_remoteFeedbackDevice, 0);
        }

        public RemoteSensoredGearbox(float unitsPerRevolution, IMotorController mc0, IFollower mc1, RemoteFeedbackDevice remoteFeedbackDevice) : this(unitsPerRevolution, mc0, new IFollower[] { mc1 }, remoteFeedbackDevice) { }

        public RemoteSensoredGearbox(float unitsPerRevolution, IMotorController mc0, IFollower mc1, IFollower mc2, RemoteFeedbackDevice remoteFeedbackDevice) : this(unitsPerRevolution, mc0, new IFollower[] { mc1, mc2 }, remoteFeedbackDevice) { }


        protected RemoteSensoredGearbox(float unitsPerRevolution, IMotorController master, IFollower[] followers) : base(master, followers)
        {
            _unitsPerRevolution = unitsPerRevolution;
            _rotationsPerUnit = 1f / _unitsPerRevolution;
            _remoteFeedbackDevice = RemoteFeedbackDevice.RemoteFeedbackDevice_None;

            /* child class takes care of sensor */
        }
        protected RemoteSensoredGearbox(float unitsPerRevolution, IMotorController master) : base(master)
        {
            _unitsPerRevolution = unitsPerRevolution;
            _rotationsPerUnit = 1f / _unitsPerRevolution;
            _remoteFeedbackDevice = RemoteFeedbackDevice.RemoteFeedbackDevice_None;

            /* child class takes care of sensor */
        }

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
        /* not available */

        //------- sensor status --------- //
        public virtual ErrorCode SetPosition(float sensorPos, int timeoutMs = 0)
        {
            //return _remoteFeedbackDevice.SetPosition(_unitsPerRevolution * sensorPos, timeoutMs);
            return ErrorCode.NotImplemented;
        }
        public virtual float GetPosition()
        {
            return _motor.GetSelectedSensorPosition(0) * _rotationsPerUnit;
        }
        public virtual float GetVelocity()
        {
            return _motor.GetSelectedSensorVelocity(0) * _rotationsPerUnit * 600f;
        }

        //----- velocity signal conditionaing ------//
        /* not available for remote sensors */

        //------ Current Lim ----------//
        /* not available */

        //------ Motion Profile Settings used in Motion Magic and Motion Profile ----------//
        public ErrorCode ConfigMotionCruiseVelocity(float rpm, int timeoutMs = 0)
        {
            return _motor.ConfigMotionCruiseVelocity((int)(rpm * kDiv_600 * _unitsPerRevolution), timeoutMs);
        }
        public ErrorCode ConfigMotionAcceleration(float rpmPerSecond, int timeoutMs = 0)
        {
            return _motor.ConfigMotionAcceleration((int)(rpmPerSecond * kDiv_600 * _unitsPerRevolution), timeoutMs);
        }

        ////------ Motion Profile Buffer ----------//
        //public void ClearMotionProfileTrajectories() { _motor.ClearMotionProfileTrajectories(); }
        //public ErrorCode GetMotionProfileTopLevelBufferCount() { return _motor.GetMotionProfileTopLevelBufferCount(); }
        //public ErrorCode PushMotionProfileTrajectory(Motion.TrajectoryPoint trajPt) { return _motor.PushMotionProfileTrajectory(trajPt); }
        //public bool IsMotionProfileTopLevelBufferFull() { return _motor.IsMotionProfileTopLevelBufferFull(); }
        //public void ProcessMotionProfileBuffer() { _motor.ProcessMotionProfileBuffer(); }
        //public void GetMotionProfileStatus(Motion.MotionProfileStatus statusToFill) { _motor.GetMotionProfileStatus(statusToFill); }
        //public void ClearMotionProfileHasUnderrun(int timeoutMs = 0) { _motor.ClearMotionProfileHasUnderrun(); }
    }
}
