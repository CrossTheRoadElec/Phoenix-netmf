using System;
using Microsoft.SPOT;
using CTRE.Phoenix.MotorControllers;
using CTRE.Phoenix.Signals;

namespace CTRE.Phoenix.Mechanical
{
    public class Linkage : IOutputSignal, IInvertable
    {
        MotorControllers.IMotorController _motor = null;
        MotorControllers.IFollower[] _follower = { null, null, null };
        int _followerCount = 0;
        bool _isInverted = false;

        /* Multiple constructors that take 1 to 4 motors per gearbox */
        public Linkage(MotorControllers.IMotorController mc1)
        {
            _motor = mc1;

            Setup();
        }
        public Linkage(MotorControllers.IMotorController mc1, MotorControllers.IFollower mc2)
        {
            _motor = mc1;
            _follower[_followerCount++] = mc2;

            Setup();
        }
        public Linkage(MotorControllers.IMotorController mc1, MotorControllers.IFollower mc2, MotorControllers.IFollower mc3)
        {
            _motor = mc1;
            _follower[_followerCount++] = mc2;
            _follower[_followerCount++] = mc3;

            Setup();
        }
        public Linkage(MotorControllers.IMotorController mc1, MotorControllers.IFollower mc2, MotorControllers.IFollower mc3, MotorControllers.IFollower mc4)
        {
            _motor = mc1;
            _follower[_followerCount++] = mc2;
            _follower[_followerCount++] = mc3;
            _follower[_followerCount++] = mc4;

            Setup();
        }

        /** Set all follower SimpleMotorcontrollers to follow the Master SimpleMotorcontroller */
        private void Setup()
        {
            for(int i = 0; i < _followerCount; i++)
            {
                _follower[i].Follow(_motor);
            }
        }

        /** Sets the motor output and takes inversion into account */
        public void Set(float output)
        {
            if (_isInverted)
                output = -output;

            _motor.Set(output);

            for(int i = 0; i < _followerCount; i++)
            {
                _follower[i].ValueUpdated();
            }
        }

        public void SetControlMode(BasicControlMode Mode)
        {
            _motor.SetControlMode(Mode);
        }

        /** IInvertable **/
        public void SetInverted(bool invert)
        {
            _isInverted = invert;
        }

        public bool GetInverted()
        {
            return _isInverted;
        }

        /* Voltage stuff */
        public void SetVoltageRampRate(float rampRate)
        {
            _motor.SetVoltageRampRate(rampRate);
        }

        public void SetVoltageCompensationRampRate(float rampRate)
        {
            _motor.SetVoltageCompensationRampRate(rampRate);
        }

        public void ConfigNominalOutputVoltage(float forwardVoltage, float reverseVoltage)
        {
            _motor.ConfigNominalOutputVoltage(forwardVoltage, reverseVoltage);
        }

        public void ConfigPeakOutputVoltage(float forwardVoltage, float reverseVoltage)
        {
            _motor.ConfigPeakOutputVoltage(forwardVoltage, reverseVoltage);
        }

        public IMotorController GetMaster()
        {
            return _motor;
        }
    }
}
