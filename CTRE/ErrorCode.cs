using System;
using Microsoft.SPOT;
using CTRE.Phoenix;

namespace CTRE
{
    public enum ErrorCode
    {
        OK = 0,

        //CAN Error Codes
        CAN_MSG_STALE = 1,
        CAN_TX_FULL = -1,
        CAN_INVALID_PARAM = -2,
        CAN_MSG_NOT_FOUND = -3,
        CAN_NO_MORE_TX_JOBS = -4,
        CAN_NO_SESSIONS_AVAIL = -5,
        CAN_OVERFLOW = -6,

        GENERAL_ERROR = -100,

        SIG_NOT_UPDATED = -200,

        //Gadgeteer Port Error Codes
        //These include errors between ports and modules
        GEN_PORT_ERROR = -300,
        PORT_MODULE_TYPE_MISMATCH = -301,

        //Gadgeteer Module Error Codes
        //These apply only to the module units themselves
        GEN_MODULE_ERROR = -400,
        MODULE_NOT_INIT_SET_ERROR = -401,
        MODULE_NOT_INIT_GET_ERROR = -402,

        FeatureNotSupported = 101,

        EEPROM_ERROR = -500,
        EEPROM_TIMED_OUT = -501,
    }

    internal struct ErrorCodeVar
    {
        public ErrorCode LastError { get; private set; }

        public ErrorCode GetLastError()
        {
            return LastError;
        }
        public ErrorCode SetLastError(ErrorCode errorCode)
        {
            if (errorCode != ErrorCode.OK)
            {
                Reporting.SetError((int)errorCode, Reporting.getHALErrorMessage((int)errorCode));
            }
            /* mirror last status */
            LastError = errorCode;
            return LastError;
        }
        public ErrorCode SetLastError(int errorCode)
        {
            return SetLastError((ErrorCode)errorCode);
        }
        //---------------------- Casts ----------------------//
        public static implicit operator ErrorCode(ErrorCodeVar d)
        {
            return d.LastError;
        }
        public static implicit operator int(ErrorCodeVar d)
        {
            return (int)d.LastError;
        }
        public static implicit operator ErrorCodeVar(ErrorCode d)
        {
            ErrorCodeVar ret = new ErrorCodeVar();
            ret.LastError = d;
            return ret;
        }
        public static implicit operator ErrorCodeVar(int d)
        {
            ErrorCodeVar ret = new ErrorCodeVar();
            ret.LastError = (ErrorCode)d;
            return ret;
        }
    }
    internal class ErrorCodeVarColl
    {
        public ErrorCode LastError { get; private set; }

        public ErrorCodeVarColl() { }

        public void Push(ErrorCode er)
        {
            if (LastError == ErrorCode.OK)
                LastError = er;
            else
            {
                /* we already captured the first one */
            }
        }
        public void Clear()
        {
            LastError = ErrorCode.OK;
        }
    }
}