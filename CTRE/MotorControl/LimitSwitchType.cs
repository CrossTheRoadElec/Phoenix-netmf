using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.MotorControl
{
    public enum LimitSwitchSource
    {
        Disabled = 0,
        FeedbackConnector = 1,
        RemoteTalonSRX = 2,
        RemoteCANifier = 3,
    }

    public enum RemoteLimitSwitchSource
    {
        Disabled = 0,
        RemoteTalonSRX = 2,
        RemoteCANifier = 3,
    }

    public enum LimitSwitchNormal
    {
        NormallyOpen,
        NormallyClosed,
    }

    public interface IHasRemoteLimitSwitches
    {
        ErrorCode ConfigForwardLimitSwitchSource(RemoteLimitSwitchSource type, LimitSwitchNormal normalOpenOrClose, int deviceID);
        ErrorCode ConfigReverseLimitSwitchSource(RemoteLimitSwitchSource type, LimitSwitchNormal normalOpenOrClose, int deviceID);

        void EnableLimitSwitch(bool forwardEnable, bool reverseEnable);
    }

    public interface IHasLimitSwitches
    {
        ErrorCode ConfigForwardLimitSwitchSource(LimitSwitchSource type, LimitSwitchNormal normalOpenOrClose);
        ErrorCode ConfigReverseLimitSwitchSource(LimitSwitchSource type, LimitSwitchNormal normalOpenOrClose);

        void EnableLimitSwitch(bool forwardEnable, bool reverseEnable);
    }

    public static class LimitSwitchRoutines
    {
        public static LimitSwitchSource Promote(RemoteLimitSwitchSource limitSwitchSource)
        {
            return (LimitSwitchSource)limitSwitchSource;
        }
    }
}
