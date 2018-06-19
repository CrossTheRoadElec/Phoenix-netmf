using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.MotorControl.CAN
{
    public class VictorSPX : BaseMotorController, IMotorController
    {
        [Obsolete("Use single parameter constructor instead.")]
        public VictorSPX(int deviceNumber, bool externalEnable = false) : base(deviceNumber | 0x01040000, externalEnable)
        {
            
        }

        public VictorSPX(int deviceNumber) : base(deviceNumber | 0x01040000, false)
        {
            
        }
    }
}
