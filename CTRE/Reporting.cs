using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix
{
    public class Reporting
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
            SetError((int)status);
        }
        private static void SetError(int status)
        {
            // if(status < 0)

            switch ((ErrorCode)status)
            {
                case ErrorCode.PORT_MODULE_TYPE_MISMATCH:
                    {
                        Debug.Print("The selected Gadgeteer Port does not support the Socket Type required by this Module.");
                        break;
                    }
                case ErrorCode.MODULE_NOT_INIT_SET_ERROR:
                    {
                        Debug.Print("The Module parameter cannot be set - Module is not initialized.");
                        break;
                    }
                case ErrorCode.MODULE_NOT_INIT_GET_ERROR:
                    {
                        Debug.Print("The Module parameter could not be read - Module is not initialized.");
                        break;
                    }
                default:
                    {
                        Debug.Print("CTRE.Reporting.SetError called with status " + status);
                        break;
                    }
            }
        }
        public static int getHALErrorMessage(int status)
        {
            return 0;
        }
    }
}
