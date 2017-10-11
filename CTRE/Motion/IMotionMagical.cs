using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.Motion
{
    public interface IMotionMagical
    {
        void SetMotionMagicAcceleration(float rotationsPerMinPerSec);
        void SetMotionMagicCruiseVelocity(float rotationsPerMin);
    }
}