using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.MotorControllers
{
    public class Cyclone : SmartMotorController
    {
        /* all CAN stuff here */

        public Cyclone(int deviceNumber, bool externalEnable = false) : base(deviceNumber, externalEnable) { }
    }
}
