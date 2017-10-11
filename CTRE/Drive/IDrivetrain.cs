using System;
using Microsoft.SPOT;
using CTRE.Phoenix.Mechanical;

namespace CTRE.Phoenix.Drive
{
    public interface IDrivetrain
    {
        void Set(Styles.Basic mode, float forward, float turn);

        /* As of now, SimpleMotors has ramp rate because of VoltageCompensation */
        void SetVoltageRampRate(float RampRate);
        void SetVoltageCompensationRampRate(float RampRate);
        void ConfigPeakPercentOutputVoltage(float forwardVoltage, float reverseVoltage); // TODO: rename to PercentOutput
        void ConfigNominalPercentOutputVoltage(float forwardVoltage, float reverseVoltage); // TODO: rename to PercentOutput
    }
}
