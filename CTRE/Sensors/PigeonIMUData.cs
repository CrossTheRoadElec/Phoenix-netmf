using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.Sensors
{
    //public static class PigeonIMUData
    //{
        /** Various calibration modes supported by Pigeon. */
        public enum CalibrationMode
        {
            BootTareGyroAccel = 0,
            Temperature = 1,
            Magnetometer12Pt = 2,
            Magnetometer360 = 3,
            Accelerometer = 5,
        };
        /** Overall state of the Pigeon. */
        public enum PigeonState
        {
            NoComm, Initializing, Ready, UserCalibration,
        };

        /**
         * Data object for status on current calibration and general status.
         *
         * Pigeon has many calibration modes supported for a variety of uses. The
         * modes generally collects and saves persistently information that makes
         * the Pigeon signals more accurate. This includes collecting temperature,
         * gyro, accelerometer, and compass information.
         *
         * For FRC use-cases, typically compass and temperature calibration is not
         * required.
         *
         * Additionally when motion driver software in the Pigeon boots, it will
         * perform a fast boot calibration to initially bias gyro and setup
         * accelerometer.
         *
         * These modes can be enabled with the EnterCalibration mode.
         *
         * When a calibration mode is entered, caller can expect...
         *
         * - PigeonState to reset to Initializing and bCalIsBooting is set to true.
         * Pigeon LEDs will blink the boot pattern. This is similar to the normal
         * boot cal, however it can an additional ~30 seconds since calibration
         * generally requires more information. currentMode will reflect the user's
         * selected calibration mode.
         *
         * - PigeonState will eventually settle to UserCalibration and Pigeon LEDs
         * will show cal specific blink patterns. bCalIsBooting is now false.
         *
         * - Follow the instructions in the Pigeon User Manual to meet the
         * calibration specific requirements. When finished calibrationError will
         * update with the result. Pigeon will solid-fill LEDs with red (for
         * failure) or green (for success) for ~5 seconds. Pigeon then perform
         * boot-cal to cleanly apply the newly saved calibration data.
         */
        public class GeneralStatus
        {
            /**
             * The current state of the motion driver. This reflects if the sensor
             * signals are accurate. Most calibration modes will force Pigeon to
             * reinit the motion driver.
             */
            public PigeonState state;
            /**
             * The currently applied calibration mode if state is in UserCalibration
             * or if bCalIsBooting is true. Otherwise it holds the last selected
             * calibration mode (when calibrationError was updated).
             */
            public CalibrationMode currentMode;
            /**
             * The error code for the last calibration mode. Zero represents a
             * successful cal (with solid green LEDs at end of cal) and nonzero is a
             * failed calibration (with solid red LEDs at end of cal). Different
             * calibration
             */
            public int calibrationError;
            /**
             * After caller requests a calibration mode, pigeon will perform a
             * boot-cal before entering the requested mode. During this period, this
             * flag is set to true.
             */
            public bool bCalIsBooting;
            /**
             * Temperature in Celsius
             */
            public float tempC;
            /**
             * Number of seconds Pigeon has been up (since boot). This register is
             * reset on power boot or processor reset. Register is capped at 255
             * seconds with no wrap around.
             */
            public int upTimeSec;
            /**
             * Number of times the Pigeon has automatically rebiased the gyro. This
             * counter overflows from 15 -> 0 with no cap.
             */
            public int noMotionBiasCount;
            /**
             * Number of times the Pigeon has temperature compensated the various
             * signals. This counter overflows from 15 -> 0 with no cap.
             */
            public int tempCompensationCount;
            /**
             * Same as getLastError()
             */
            public ErrorCode lastError;

            public String description;

            /**
             * general string description of current status
             */
            override public String ToString()
            {
                /* build description string */
                if (lastError != ErrorCode.OK)
                { // same as NoComm
                    description = "Status frame was not received, check wired connections and web-based config.";
                }
                else if (bCalIsBooting)
                {
                    description = "Pigeon is boot-caling to properly bias accel and gyro.  Do not move Pigeon.  When finished biasing, calibration mode will start.";
                }
                else if (state == PigeonState.UserCalibration)
                {
                    /* mode specific descriptions */
                    switch (currentMode)
                    {
                        case CalibrationMode.BootTareGyroAccel:
                            description = "Boot-Calibration: Gyro and Accelerometer are being biased.";
                            break;
                        case CalibrationMode.Temperature:
                            description = "Temperature-Calibration: Pigeon is collecting temp data and will finish when temp range is reached. \n";
                            description += "Do not move Pigeon.";
                            break;
                        case CalibrationMode.Magnetometer12Pt:
                            description = "Magnetometer Level 1 calibration: Orient the Pigeon PCB in the 12 positions documented in the User's Manual.";
                            break;
                        case CalibrationMode.Magnetometer360:
                            description = "Magnetometer Level 2 calibration: Spin robot slowly in 360' fashion.  ";
                            break;
                        case CalibrationMode.Accelerometer:
                            description = "Accelerometer Calibration: Pigeon PCB must be placed on a level source.  Follow User's Guide for how to level surfacee.  ";
                            break;
                        default:
                            description = "Unknown status";
                            break;
                    }
                }
                else if (state == PigeonState.Ready)
                {
                    /*
                     * definitely not doing anything cal-related. So just instrument
                     * the motion driver state
                     */
                    description = "Pigeon is running normally.  Last CAL error code was ";
                    description += calibrationError;
                    description += ".";
                }
                else if (state == PigeonState.Initializing)
                {
                    /*
                     * definitely not doing anything cal-related. So just instrument
                     * the motion driver state
                     */
                    description = "Pigeon is boot-caling to properly bias accel and gyro.  Do not move Pigeon.";
                }
                else
                {
                    description = "Not enough data to determine status.";
                }
                return description;
            }
        };

    /** Data object for holding fusion information. */
    public class FusionStatus
    {
        public float heading;
        public bool bIsValid;
        public bool bIsFusing;

        /**
         * Same as getLastError()
         */
        public ErrorCode lastError;

        public String description;

        override public String ToString()
        {
            if (lastError != ErrorCode.OK)
            {
                description = "Could not receive status frame.  Check wiring and web-config.";
            }
            else if (bIsValid == false)
            {
                description = "Fused Heading is not valid.";
            }
            else if (bIsFusing == false)
            {
                description = "Fused Heading is valid.";
            }
            else
            {
                description = "Fused Heading is valid and is fusing compass.";
            }
            return description;
        }
    }

    public struct PigeonIMU_Faults
        {
            //!< True iff any of the above flags are true.
            bool HasAnyFault()
            {
                return false;
            }
            int ToBitfield()
            {
                int retval = 0;
                return retval;
            }

            PigeonIMU_Faults(int bits) { }
        };

        public struct PigeonIMU_StickyFaults
        {
            //!< True iff any of the above flags are true.
            bool HasAnyFault()
            {
                return false;
            }
            int ToBitfield()
            {
                int retval = 0;
                return retval;
            }

            PigeonIMU_StickyFaults(int bits) { }
        };
    //}
}
