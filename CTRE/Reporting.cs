using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix
{
    public static class Reporting
    {
        
        

        /**
         * Prints to Visual Studio console.
         * @param message 
         *                  Message to print.
         */
        public static void ConsolePrint(String message)
        {
            Debug.Print(message);
        }
        public static void SetError(ErrorCode status, int unused)
        {
            SetError((int)status, unused);
        }
        public static void SetError(int status, int unused)
        {
            SetError(status);
        }
        public static void SetError(ErrorCode status)
        {
            Log(status, "", 0, "");
        }
        private static void SetError(int status)
        {
            Log((ErrorCode)status, "", 0, "");
        }
        public static int getHALErrorMessage(int status)
        {
            return 0;
        }

        public static ErrorCode Log (ErrorCode code, string origin, int hierarchy, string stacktrace)
        {
            LowLevel.MsgEntry _me = new LowLevel.MsgEntry(code, origin, stacktrace, hierarchy);

            if (_me.NotWorthLogging())
            {

            }
            else
            {
                _me.LogToDs();
            }

            return ErrorCode.OK;
        }

        
    }
}
