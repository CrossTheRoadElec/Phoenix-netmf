using System;
using Microsoft.SPOT;
using CTRE.Phoenix.Mechanical;
using CTRE.Phoenix.MotorControllers;

namespace CTRE.Phoenix.Drive
{
    public class Tank : IDrivetrain
    {
        Gearbox _left;
        Gearbox _right;

        /** Tank Drive constructor that takes a left gearbox, right gearbox, and side inverted */
        public Tank(Gearbox left, Gearbox right, bool leftInvert, bool rightInvert)
        {
            GroupMotorControllers.Register(left.GetMaster());

            _left = left;
            _right = right;

            _left.SetInverted(leftInvert);
            _right.SetInverted(rightInvert);
        }

        public Tank(IMotorController m1, IMotorController m2, bool leftInvert, bool rightInvert)
        {
            GroupMotorControllers.Register(m1);
            /* Create 2 single motor gearboxes */
            Gearbox temp1 = new Gearbox(m1);
            Gearbox temp2 = new Gearbox(m2);

            _left = temp1;
            _right = temp2;

            _left.SetInverted(leftInvert);
            _right.SetInverted(rightInvert);
        }

        /** Inherited from IDrivetrain */
        public void Set(Styles.Basic mode, float forward, float turn)
        {
            float l, r;
            Util.Split_1(forward, turn, out l, out r);

            Drive(mode, l, r);
        }
        
        public void SetVoltageRampRate(float rampRate)
        {
            _left.SetVoltageRampRate(rampRate);
            _right.SetVoltageRampRate(rampRate);
        }

        public void SetVoltageCompensationRampRate(float rampRate)
        {
            _left.SetVoltageCompensationRampRate(rampRate);
            _right.SetVoltageCompensationRampRate(rampRate);
        }

        public void ConfigPeakPercentOutputVoltage(float forwardVoltage, float reverseVoltage)
        {
            _left.ConfigPeakOutputVoltage(forwardVoltage, reverseVoltage);
            _right.ConfigPeakOutputVoltage(forwardVoltage, reverseVoltage);
        }

        public void ConfigNominalPercentOutputVoltage(float forwardVoltage, float reverseVoltage)
        {
            _left.ConfigNominalOutputVoltage(forwardVoltage, reverseVoltage);
            _right.ConfigNominalOutputVoltage(forwardVoltage, reverseVoltage);
        }

        private void Drive(Styles.Basic mode, float left, float right)
        {
            if (mode == Styles.Basic.Voltage)
            {
                _left.SetControlMode(BasicControlMode.kVoltage);
                _right.SetControlMode(BasicControlMode.kVoltage);
            }
            else if (mode == Styles.Basic.PercentOutput)
            {
                _left.SetControlMode(BasicControlMode.kPercentVbus);
                _right.SetControlMode(BasicControlMode.kPercentVbus);
            }

            _left.Set(left);
            _right.Set(right);
        }
    }
}
