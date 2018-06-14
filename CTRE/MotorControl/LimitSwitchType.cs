using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.MotorControl
{
    public enum LimitSwitchSource
    {
        FeedbackConnector = 0, /* default */
        RemoteTalonSRX = 1,
        RemoteCANifier = 2,
        Deactivated = 3,
    }

    public enum RemoteLimitSwitchSource
    {
        RemoteTalonSRX = 1,
        RemoteCANifier = 2,
        Deactivated = 3,
    }

    public enum LimitSwitchNormal
    {
        NormallyOpen,
        NormallyClosed,
        Disabled,
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
    public static string ToString(LimitSwitchSource value) {
        switch(value) {
            case LimitSwitchSource.FeedbackConnector : return "LimitSwitchSource.FeedbackConnector";
            case LimitSwitchSource.RemoteTalonSRX : return "LimitSwitchSource.RemoteTalonSRX";
            case LimitSwitchSource.RemoteCANifier : return "LimitSwitchSource.RemoteCANifier";
            case LimitSwitchSource.Deactivated : return "LimitSwitchSource.Deactivated";
            default : return "InvalidValue";
        }

    }
    public static string ToString(RemoteLimitSwitchSource value) {
        switch(value) {
            case RemoteLimitSwitchSource.RemoteTalonSRX : return "RemoteLimitSwitchSource.RemoteTalonSRX";
            case RemoteLimitSwitchSource.RemoteCANifier : return "RemoteLimitSwitchSource.RemoteCANifier";
            case RemoteLimitSwitchSource.Deactivated : return "RemoteLimitSwitchSource.Deactivated";
            default : return "InvalidValue";
        }

    }
    public static string ToString(LimitSwitchNormal value) {
        switch(value) {
            case LimitSwitchNormal.NormallyOpen : return "LimitSwitchNormal.NormallyOpen";
            case LimitSwitchNormal.NormallyClosed : return "LimitSwitchNormal.NormallyClosed";
            case LimitSwitchNormal.Disabled : return "LimitSwitchNormal.Disabled";
            default : return "InvalidValue";
        }

    }

    }
}
