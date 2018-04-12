using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.MotorControl
{
    public interface IFollower // : CANBusDevice TODO CLEANUP and package CAN stuff
    {
        //void Follow(IMotorController masterToFollow, FollowerType followerType);
        void Follow(IMotorController masterToFollow);
        void ValueUpdated();
    }
}
