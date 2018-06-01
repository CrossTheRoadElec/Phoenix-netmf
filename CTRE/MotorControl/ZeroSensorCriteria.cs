using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.MotorControl
{
    public class ZeroSensorCriteria
    {
        public bool ZeroSensorOnIdx;
        public bool IdxRisingEdge;
        public bool ZeroSensorOnReverseLimitSwitch;
        public bool ZeroSensorOnForwardLimitSwitch;
    }
}
