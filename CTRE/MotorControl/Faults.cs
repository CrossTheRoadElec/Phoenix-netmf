using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.MotorControl
{
    public struct Faults
    {
        public bool HardwareFailure;
        public bool UnderVoltage;
        public bool OverTemp;
        public bool ForwardLimitSwitch;
        public bool ReverseLimitSwitch;
        public bool ForwardSoftLimit;
        public bool ReverseSoftLimit;
        public bool MsgOverflow;
        public bool ResetDuringEn;

        //!< True iff any of the above flags are true.
        public bool AnyFault
        {
            get
            {
                return HardwareFailure |
                        UnderVoltage |
                        OverTemp |
                        ForwardLimitSwitch |
                        ReverseLimitSwitch |
                        ForwardSoftLimit |
                        ReverseSoftLimit |
                        MsgOverflow |
                        ResetDuringEn;
            }
        }
    }
}
