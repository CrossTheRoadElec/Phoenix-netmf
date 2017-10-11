using System;
using Microsoft.SPOT;
using CTRE.Phoenix.Mechanical;
using CTRE.Phoenix.MotorControllers;

namespace CTRE.Phoenix.Drive
{
    public class SensoredTank : ISmartDrivetrain
    {
        SensoredGearbox _left;
        SensoredGearbox _right;

        /** Encoder heading properties */
        public float distanceBetweenWheels { get; set; }
        public uint ticksPerRev { get; set; }
        public float wheelRadius { get; set; }
        float tinyScalor = 1;   /* Not sure if I should make this setable */

        /* Sensored Tank constructor (uses two gearboxes)*/
        public SensoredTank(SensoredGearbox left, SensoredGearbox right, bool leftInverted, bool rightInverted, float wheelRadius)
        {
            GroupMotorControllers.Register(left.GetMaster());
            _left = left;
            _right = right;

            _left.SetInverted(leftInverted);
            _right.SetInverted(rightInverted);

            if (wheelRadius < 0.01)
                Debug.Print("CTR: Wheel radius must be greater than 0.01");
            this.wheelRadius = wheelRadius;
        }

        public SensoredTank(SmartMotorController m1, SmartMotorController m2, SmartMotorController.FeedbackDevice feedbackDevice,  bool leftInverted, bool rightInverted, float wheelRadius)
        {
            GroupMotorControllers.Register(m1);

            /* Create 2 single motor gearboxes */
            SensoredGearbox temp1 = new SensoredGearbox(1, m1, feedbackDevice);
            SensoredGearbox temp2 = new SensoredGearbox(1, m2, feedbackDevice);

            _left = temp1;
            _right = temp2;

            _left.SetInverted(leftInverted);
            _right.SetInverted(rightInverted);

            if (wheelRadius < 0.01)
                Debug.Print("CTR: Wheel radius must be greater than 0.01");
            this.wheelRadius = wheelRadius;
        }

        /** Part of IDrivetrain; Takes control mode, forward output and turn output */
        public void Set(Styles.Smart mode, float forward, float turn)
        {
            float l, r;
            Util.Split_1(forward, turn, out l, out r);

            Drive(mode, l, r);
        }
        public void Set(Styles.Basic basicStyle, float forward, float turn)
        {
            Set(Styles.StylesRoutines.Promote(basicStyle), forward, turn);
        }
        

        /* Set the currentlimit with Amps and a timeout */
        public void SetCurrentLimit(uint currentAmps, uint timeoutMs)
        {
            _left.SetCurrentLimit(currentAmps, timeoutMs);
            _right.SetCurrentLimit(currentAmps, timeoutMs);
        }

        /* Grab the position throught the talons' Smart features */
        public float GetDistance()
        {
            float l = _left.GetPosition();
            float r = _right.GetPosition();

            return (l + r) * 0.5f;
        }

        /* Grab the velocity throught the talons' Smart features */
        public float GetVelocity()
        {
            float l = _left.GetVelocity();
            float r = _right.GetVelocity();

            return (l + r) * 0.5f;
        }

        public float GetEncoderHeading()
        {
            float l = _left.GetPosition();
            float r = _right.GetPosition();

            if (wheelRadius < 0.01)
            {
                Debug.Print("CTR: Sensored Tank has too small of a wheel radius, cannot get heading");
                return 0;
            }

            if (ticksPerRev == 0)
            {
                Debug.Print("CTR: Sensored Tank has not set ticks per wheel revolution, cannot get heading");
                return 0;
            }

            if (distanceBetweenWheels < 0.01)
            {
                Debug.Print("CTR: Sensored Tank has too small of a distance between wheels, cannot get heading");
                return 0;
            }

            float unitsPerTick = (float)(2 * System.Math.PI * wheelRadius) / ticksPerRev;
            float theta = ((r-l) / (distanceBetweenWheels / unitsPerTick) * (float)(180 / System.Math.PI)) * tinyScalor;

            return theta;
        }

        /* Reset the encoders on both side of the TankDrivetrain */
        public void SetPosition(float position)
        {
            _left.SetSensor(position);
            _right.SetSensor(position);
        }

        /** Sensored Tank drive that takes the mode, left, and right side */
        private void Drive(Styles.Smart mode, float left, float right)
        {
            if(mode == Styles.Smart.Voltage)
            {
                _left.SetControlMode(ControlMode.kVoltage);
                _right.SetControlMode(ControlMode.kVoltage);
            }
            else if (mode == Styles.Smart.PercentOutput)
            {
                _left.SetControlMode(ControlMode.kPercentVbus);
                _right.SetControlMode(ControlMode.kPercentVbus);
            }
            else if(mode == Styles.Smart.VelocityClosedLoop)
            {
                _left.SetControlMode(ControlMode.kSpeed);
                _right.SetControlMode(ControlMode.kSpeed);
            }

            _left.Set(left);
            _right.Set(right);
        }

        public void ConfigNominalPercentOutputVoltage(float forwardVoltage, float reverseVoltage)
        {
            _left.ConfigNominalOutputVoltage(forwardVoltage, reverseVoltage);
            _right.ConfigNominalOutputVoltage(forwardVoltage, reverseVoltage);
        }

        public void ConfigPeakPercentOutputVoltage(float forwardVoltage, float reverseVoltage)
        {
            _left.ConfigPeakOutputVoltage(forwardVoltage, reverseVoltage);
            _right.ConfigPeakOutputVoltage(forwardVoltage, reverseVoltage);
        }

        public void SetVoltageCompensationRampRate(float rampRate)
        {
            _left.SetVoltageCompensationRampRate(rampRate);
            _right.SetVoltageCompensationRampRate(rampRate);
        }

        public void SetVoltageRampRate(float rampRate)
        {
            _left.SetVoltageRampRate(rampRate);
            _right.SetVoltageRampRate(rampRate);
        }

        /* IMotionMagical */
        public void SetMotionMagicAcceleration(float rotationsPerMinPerSec)
        {
            /* RPMPS?? */
            _left.SetMotionMagicAcceleration(rotationsPerMinPerSec);
            _right.SetMotionMagicAcceleration(rotationsPerMinPerSec);
        }

        public void SetMotionMagicCruiseVelocity(float rotationsPerMin)
        {
            _left.SetMotionMagicCruiseVelocity(rotationsPerMin);
            _right.SetMotionMagicCruiseVelocity(rotationsPerMin);
        }
    }
}
