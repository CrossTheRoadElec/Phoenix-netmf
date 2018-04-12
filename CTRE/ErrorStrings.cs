using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix
{
    public static class ErrorStrings
    {
        /**
         * @param code Error Code to get description for.
         * @param shortDescripToFill [out] string ref to fill with short description.
         * @param longDescripToFill [out] string ref to fill with long description.
         */
        public static void GetErrorDescription(ErrorCode code,
                                    out string shortDescripToFill,
                                    out string longDescripToFill)
        {

            switch (code)
            {

                case ErrorCode.NotImplemented:
                    longDescripToFill = "Feature Not Implemented.  Be sure to update to latest when available.";
                    shortDescripToFill = "Not Implemented, check latest installer.";
                    break;

                default:
                    longDescripToFill = "Error Code " + (int)code;
                    shortDescripToFill = "Error Code " + (int)code;
                    break;

                case ErrorCode.OKAY:
                    longDescripToFill = "No Error";
                    shortDescripToFill = "No Error";
                    break;

                case ErrorCode.GeneralError:
                    longDescripToFill = "User Error";
                    shortDescripToFill = "User Error";
                    break;
                case ErrorCode.GeneralWarning:
                    longDescripToFill = "User Warning";
                    shortDescripToFill = "User Warning";
                    break;
                case ErrorCode.CouldNotChangePeriod:
                    longDescripToFill = "Control Frame Period could not be changed.  Most likely it is not being transmitted.";
                    shortDescripToFill = "Control Frame Period could not be changed.  Most likely it is not being transmitted.";
                    break;
                case ErrorCode.WheelRadiusTooSmall:
                    longDescripToFill =
                            "Wheel Radius is too small, must be at least 0.01. Try \"variableName.wheelRadius = yourWheelRadius\"";
                    shortDescripToFill =
                            "Wheel Radius is too small, cannot get distance traveled";
                    break;
                case ErrorCode.TicksPerRevZero:
                    longDescripToFill =
                            "Ticks per revolution is 0, must be greater than 0. Try \"variableName.ticksPerRev = yourTicksPerRev\"";
                    shortDescripToFill = "Ticks per revolution is 0, cannot get heading";
                    break;
                case ErrorCode.DistanceBetweenWheelsTooSmall:
                    longDescripToFill =
                            "Distance between wheels is too small, must be greater than 0.01. Try \"variableName.distanceBetweenWheels = yourDistanceBetweenWheels\"";
                    shortDescripToFill =
                            "Distance between wheels is too small, cannot get heading";
                    break;

                case ErrorCode.RxTimeout:
                    longDescripToFill = "CAN frame is too stale or has not been received.";
                    shortDescripToFill = "CAN frame not received/too-stale.";
                    break;
                case ErrorCode.TxTimeout:
                    longDescripToFill = "TxTimeout";
                    shortDescripToFill = "TxTimeout";
                    break;
                case ErrorCode.InvalidParamValue:
                    longDescripToFill = "Incorrect argument passed into function/VI.  Check the input parameter values against software documentation.";
                    shortDescripToFill = "Incorrect argument passed into function/VI.";
                    break;
                case ErrorCode.UnexpectedArbId:
                    longDescripToFill = "ArbID is incorrect, UnexpectedArbId";
                    shortDescripToFill = "ArbID is incorrect";
                    break;
                case ErrorCode.TxFailed:
                    longDescripToFill = "Could not transmit CAN Frame, TxFailed";
                    shortDescripToFill = "Could not transmit CAN Frame";
                    break;
                case ErrorCode.SigNotUpdated:
                    longDescripToFill = "No new response to update signal, SigNotUpdated";
                    shortDescripToFill = "No new response to update signal";
                    break;
                case ErrorCode.BufferFull:
                    longDescripToFill =
                            "Buffer is full, cannot insert more data, BufferFull";
                    shortDescripToFill = "Buffer is full, cannot insert more data";
                    break;
                case ErrorCode.SensorNotPresent:
                    longDescripToFill = "Sensor is not present, cannot get information";
                    shortDescripToFill = "Sensor Not Present";
                    break;
                case ErrorCode.InvalidHandle:
                    longDescripToFill =
                            "Handle passed into function did not match handle in stored dictionary";
                    shortDescripToFill = "Handle passed into function is incorrect";
                    break;
                case ErrorCode.FeatureNotSupported:
                    longDescripToFill = "Feature Not Supported";
                    shortDescripToFill = "Feature Not Supported";
                    break;
                case ErrorCode.FirmwareTooOld:
                    longDescripToFill =
                            "Firmware Too Old.  Use Web-based config to field upgrade your CTRE CAN device firmware(CRF).";
                    shortDescripToFill =
                            "Firmware Too Old.  Use Web-based config to field upgrade your CTRE CAN device firmware(CRF).";
                    break;
                case ErrorCode.FirmVersionCouldNotBeRetrieved:
                    longDescripToFill =
                            "Firm Vers could not be retrieved. Most likely the device ID is incorrect or the firmware(CRF) is too old.";
                    shortDescripToFill =
                            "Firm Vers could not be retrieved. Use Web-based config to check ID and firmware(CRF) version. ";
                    break;
                case ErrorCode.FeaturesNotAvailableYet:
                    longDescripToFill =
                            "This feature will be supported in a future update.";
                    shortDescripToFill =
                            "This feature will be supported in a future update.";
                    break;
                case ErrorCode.ControlModeNotValid:
                    longDescripToFill =
                            "The motor controller's control mode is not valid for this function.";
                    shortDescripToFill =
                            "The control mode is not valid for this function.";
                    break;
                case ErrorCode.FeatureRequiresHigherFirm:
                    longDescripToFill =
                            "CANifier Quadrature features require firmware version 0.42";
                    shortDescripToFill =
                            "CANifier Quadrature requires firm vers 0.42";
                    break;
                case ErrorCode.TalonFeatureRequiresHigherFirm:
                    longDescripToFill =
                            "Motor Controller Remote/Arc features require firmware version 3.8 or higher to use";
                    shortDescripToFill =
                            "Motor Controller Remote/Arc features require firmware >=3.8";
                    break;
                case ErrorCode.ControlModeNotSupportedYet:
                    longDescripToFill =
                            "This control mode is not supported yet.  A future release will supported this soon.";
                    shortDescripToFill =
                            "This control mode is not supported yet.  A future release will supported this soon.";
                    break;
                case ErrorCode.AuxiliaryPIDNotSupportedYet:
                    longDescripToFill =
                            "Auxiliary (secondary) PID Loop in Talon SRX/Victor SPX firmware is not supported yet.  A future release will support this soon.";
                    shortDescripToFill =
                            "Auxiliary (secondary) PID Loop in Talon SRX/Victor SPX firmware is not supported yet.  A future release will support this soon.";
                    break;
                case ErrorCode.RemoteSensorsNotSupportedYet:
                    longDescripToFill =
                            "Remote sensors (for soft limit and closed loop) are not supported yet.  A future release will support this soon.";
                    shortDescripToFill =
                            "Remote sensors (for soft limit and closed loop) are not supported yet.  A future release will support this soon.";
                    break;
                case ErrorCode.MotProfFirmThreshold:
                    longDescripToFill = "Motor Controller must have >= 3.2 firmware for motion profile control mode.";
                    shortDescripToFill = "Motor Controller must have >= 3.2 firmware for motion profile control mode.";
                    break;
                case ErrorCode.MotProfFirmThreshold2:
                    longDescripToFill = "Motor Controller must have >= 3.4 firmware for advanced PID0/PID1 features.";
                    shortDescripToFill = "Motor Controller must have >= 3.4 firmware for advanced PID0/PID1 features.";
                    break;
                case ErrorCode.PORT_MODULE_TYPE_MISMATCH:
                    longDescripToFill = "The selected Gadgeteer Port does not support the Socket Type required by this Module.";
                    shortDescripToFill = "The selected Gadgeteer Port does not support the Socket Type required by this Module.";
                    break;
                case ErrorCode.MODULE_NOT_INIT_SET_ERROR:
                    longDescripToFill = "The Module parameter cannot be set - Module is not initialized.";
                    shortDescripToFill = "The Module parameter cannot be set - Module is not initialized.";
                    break;
                case ErrorCode.MODULE_NOT_INIT_GET_ERROR:
                    longDescripToFill = "The Module parameter could not be read - Module is not initialized.";
                    shortDescripToFill = "The Module parameter could not be read - Module is not initialized.";
                    break;
            }
        }
    }
}
