using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.MotorControl
{
    public interface IFollower
    {
        void Follow(IMotorController masterToFollow);
        void ValueUpdated();
    }
}
