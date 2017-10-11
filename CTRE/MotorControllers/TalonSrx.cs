//TODO: talon will be native untis only, classes above talon will scale to rotations

using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.MotorControllers
{
    public class TalonSrx : SmartMotorController
    {
        // : CANBusDevice TODO CLEANUP and package CAN stuff  /* all CAN stuff here */
        public TalonSrx(int deviceNumber, bool externalEnable = false) : base(deviceNumber, externalEnable) { }
    }
}