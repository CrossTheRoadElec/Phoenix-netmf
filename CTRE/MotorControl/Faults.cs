using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.MotorControl
{
    public struct Faults
    {
        public bool UnderVoltage;
        public bool ForwardLimitSwitch;
        public bool ReverseLimitSwitch;
        public bool ForwardSoftLimit;
        public bool ReverseSoftLimit;
        public bool HardwareFailure;
        public bool ResetDuringEn;
        public bool SensorOverflow;
        public bool SensorOutOfPhase;
        public bool HardwareESDReset;
        public bool RemoteLossOfSignal;

        //!< True iff any of the above flags are true.
        public bool AnyFault
        {
            get
            {
                return 
                        UnderVoltage |
                        ForwardLimitSwitch |
                        ReverseLimitSwitch |
                        ForwardSoftLimit |
                        ReverseSoftLimit |
                        HardwareFailure |
                        ResetDuringEn |
                        SensorOverflow |
                        SensorOutOfPhase |
                        HardwareESDReset |
                        RemoteLossOfSignal;
            }
        }
    }
}
