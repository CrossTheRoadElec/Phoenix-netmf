using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.MotorControllers
{
    public interface IFollower // : CANBusDevice TODO CLEANUP and package CAN stuff
    {
        void Follow(Object masterToFollow);
        void ValueUpdated();
    }
}
