using System;
using Microsoft.SPOT;
using CTRE.Phoenix.Mechanical;
using CTRE.Phoenix.Motion;

namespace CTRE.Phoenix.Drive
{
    public interface ISmartDrivetrain : IDrivetrain, IMotionMagical
    {
        void Set(Styles.Smart mode, float forward, float turn);

        void SetCurrentLimit(uint currentLimitAmps, uint timeoutMs);
        float GetDistance();
        float GetVelocity();

        float GetEncoderHeading();
        void SetPosition(float position);

        /* need stuff of motion profile and closed loops */
    }
}