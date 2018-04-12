using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.MotorControl.CAN
{
    public class VictorSPX : BaseMotorController, IMotorController
    {
        public VictorSPX(int deviceNumber, bool externalEnable = false) : base(deviceNumber | 0x01040000, externalEnable)
        {
            
        }
    }
}
