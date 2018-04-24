using System;
using Microsoft.SPOT;
using CTRE.Phoenix.Signals;
using CTRE.Phoenix.MotorControl;

namespace CTRE.Phoenix.Mechanical
{
    public class Linkage : IInvertable
    {
        protected IMotorController _motor = null;
        protected IFollower[] _followers = new IFollower[0];

        //--------------------- Constructors -----------------------------//
        /* Multiple constructors that take 1 to 4 motors per gearbox */
        public Linkage(IMotorController mc0)
        {
            _motor = mc0;

            Setup();
        }
        public Linkage(IMotorController master, IFollower[] followers)
        {
            _motor = master;
            _followers = new IFollower[followers.Length];

            for (int i = 0; i < _followers.Length; ++i)
            {
                _followers[i] = followers[i];
            }

            Setup();
        }
        public Linkage(IMotorController mc0, IFollower mc1) : this(mc0, new IFollower[] { mc1 }) { }
        public Linkage(IMotorController mc0, IFollower mc1, IFollower mc2) : this(mc0, new IFollower[] { mc1, mc2 }) { }
        public Linkage(IMotorController mc0, IFollower mc1, IFollower mc2, IFollower mc3) : this(mc0, new IFollower[] { mc1, mc2, mc3 }) { }

        public IMotorController MasterMotorController { get { return _motor; } }

        public IFollower [] Followers { get { return _followers; } }

        /** Set all follower SimpleMotorControl to follow the Master SimpleMotorcontroller */
        private void Setup()
        {
            foreach (var follower in _followers) { follower.Follow(_motor); }
        }

        //------ Set output routines. ----------//

        /** Sets the motor output and takes inversion into account */
        public virtual void Set(ControlMode controlMode, float output0, DemandType demand1Type = DemandType.Neutral, float output1 = 0)
        {
            _motor.Set(controlMode, output0, demand1Type, output1);

            foreach (var follower in _followers) { follower.ValueUpdated(); }
        }

        public virtual void NeutralOutput() { Set(ControlMode.PercentOutput, 0); }

        public virtual void SetNeutralMode(NeutralMode neutralMode) { _motor.SetNeutralMode(neutralMode); }

        //------ Invert behavior ----------//
        public virtual void SetSensorPhase(bool PhaseSensor) { _motor.SetSensorPhase(PhaseSensor); }

        public virtual void SetInverted(bool invert) { _motor.SetInverted(invert); }

        public virtual bool GetInverted() { return _motor.GetInverted(); }


        //----- general output shaping ------------------//
        public virtual ErrorCode ConfigOpenloopRamp(float secondsFromNeutralToFull, int timeoutMs = 0)
        {
            return _motor.ConfigOpenloopRamp(secondsFromNeutralToFull, timeoutMs);
        }
        public virtual ErrorCode ConfigClosedloopRamp(float secondsFromNeutralToFull, int timeoutMs = 0)
        {
            return _motor.ConfigClosedloopRamp(secondsFromNeutralToFull, timeoutMs);
        }
        public virtual ErrorCode ConfigPeakOutputForward(float percentOut, int timeoutMs = 0)
        {
            return _motor.ConfigPeakOutputForward(percentOut, timeoutMs);
        }
        public virtual ErrorCode ConfigPeakOutputReverse(float percentOut, int timeoutMs = 0)
        {
            return _motor.ConfigPeakOutputReverse(percentOut, timeoutMs);
        }
        public virtual ErrorCode ConfigNominalOutputForward(float percentOut, int timeoutMs = 0)
        {
            return _motor.ConfigNominalOutputForward(percentOut, timeoutMs);
        }
        public virtual ErrorCode ConfigNominalOutputReverse(float percentOut, int timeoutMs = 0)
        {
            return _motor.ConfigNominalOutputReverse(percentOut, timeoutMs);
        }
        public virtual ErrorCode ConfigNeutralDeadband(float percentDeadband = Constants.DefaultDeadband, int timeoutMs = 0)
        {
            return _motor.ConfigNeutralDeadband(percentDeadband, timeoutMs);
        }

        //------ Voltage Compensation ----------//
        public virtual ErrorCode ConfigVoltageCompSaturation(float voltage, int timeoutMs = 0)
        {
            return _motor.ConfigVoltageCompSaturation(voltage, timeoutMs);
        }
        public virtual ErrorCode ConfigVoltageMeasurementFilter(int filterWindowSamples, int timeoutMs = 0)
        {
            return _motor.ConfigVoltageCompSaturation(filterWindowSamples, timeoutMs);
        }
        public virtual void EnableVoltageCompensation(bool enable) { _motor.EnableVoltageCompensation(enable); }

        //------ General Status ----------//
        public virtual float GetBusVoltage() { return _motor.GetBusVoltage(); }
        public virtual float GetMotorOutputPercent() { return _motor.GetMotorOutputPercent(); }
        public virtual float GetMotorOutputVoltage() { return _motor.GetMotorOutputVoltage(); }
        public virtual float GetOutputCurrent() { return _motor.GetOutputCurrent(); }
        public virtual float GetTemperature() { return _motor.GetTemperature(); }

        //------ sensor selection ----------//
        /* not supported */

        //------- sensor status --------- //
        /* not supported */

        //------ status frame period changes ----------//
        public virtual ErrorCode SetControlFramePeriod(ControlFrame frame, int periodMs)
        {
            return _motor.SetControlFramePeriod(frame, periodMs);
        }
        public virtual ErrorCode SetStatusFramePeriod(StatusFrame frame, int periodMs, int timeoutMs = 0)
        {
            return _motor.SetStatusFramePeriod(frame, periodMs, timeoutMs);
        }

        //------ limit switch ----------//
        public virtual ErrorCode ConfigForwardLimitSwitchSource(RemoteLimitSwitchSource type, LimitSwitchNormal normalOpenOrClose, int deviceID)
        {
            return _motor.ConfigForwardLimitSwitchSource(type, normalOpenOrClose, deviceID);
        }
        public virtual ErrorCode ConfigReverseLimitSwitchSource(RemoteLimitSwitchSource type, LimitSwitchNormal normalOpenOrClose, int deviceID)
        {
            return _motor.ConfigReverseLimitSwitchSource(type, normalOpenOrClose, deviceID);
        }
        public virtual void EnableLimitSwitches(bool enable)
        {
            _motor.OverrideLimitSwitchesEnable(enable);
        }

        //------ soft limit ----------//
        /* not sensored */

        //------ Current Lim ----------//
        /* not supported */

        //------ General Close loop ----------//
        //------ Motion Profile Settings used in Motion Magic and Motion Profile ----------//
        //------ Motion Profile Buffer ----------//
        /* not sensored */
    }
}
