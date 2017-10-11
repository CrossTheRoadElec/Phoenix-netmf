using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.MotorControllers
{
    public interface IMotorController : Signals.IOutputSignal , IFollower, IInvertable
    {
        void SetControlMode(BasicControlMode Mode);
        void SetVoltageRampRate(float rampRate);
        void SetVoltageCompensationRampRate(float rampRate);
        int ConfigPeakOutputVoltage(float forwardVoltage, float reverseVoltage, uint timeoutMs = 0);
        int ConfigNominalOutputVoltage(float forwardVoltage, float reverseVoltage, uint timeoutMs = 0);
        float GetBusVoltage();
    }
}