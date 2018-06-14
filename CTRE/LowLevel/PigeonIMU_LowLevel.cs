using System;
using Microsoft.SPOT;
using CTRE.Phoenix.Sensors;

namespace CTRE.Phoenix.LowLevel
{
    public class PigeonIMU_LowLevel : Device_LowLevel
    {

        /** overall threshold for when frame data is too old */
        const int EXPECTED_RESPONSE_TIMEOUT_MS = (200);

        const int RAW_STATUS_2 = 0x00040C40;
        const int RAW_STATUS_4 = 0x00040CC0;
        const int RAW_STATUS_6 = 0x00040D40;

        const int BIASED_STATUS_2 = 0x00041C40;
        const int BIASED_STATUS_4 = 0x00041CC0;
        const int BIASED_STATUS_6 = 0x00041D40;

        const int COND_STATUS_1 = 0x00042000;
        const int COND_STATUS_2 = 0x00042040;
        const int COND_STATUS_3 = 0x00042080;
        const int COND_STATUS_4 = 0x000420c0;
        const int COND_STATUS_5 = 0x00042100;
        const int COND_STATUS_6 = 0x00042140;
        const int COND_STATUS_7 = 0x00042180;
        const int COND_STATUS_8 = 0x000421c0;
        const int COND_STATUS_9 = 0x00042200;
        const int COND_STATUS_10 = 0x00042240;
        const int COND_STATUS_11 = 0x00042280;
        const int COND_STATUS_13 = 0x00042300;

        const int CONTROL_1 = 0x00042800;

        const int PARAM_REQUEST = 0x00042C00;
        const int PARAM_RESPONSE = 0x00042C40;
        const int PARAM_SET = 0x00042C80;

        private UInt64 _cache;
        private UInt32 _len;

        const int kMinFirmwareVersionMajor = 0;
        const int kMinFirmwareVersionMinor = 40;

        

        enum MotionDriverState
        {
            Init0 = 0,
            WaitForPowerOff = 1,
            ConfigAg = 2,
            SelfTestAg = 3,
            StartDMP = 4,
            ConfigCompass_0 = 5,
            ConfigCompass_1 = 6,
            ConfigCompass_2 = 7,
            ConfigCompass_3 = 8,
            ConfigCompass_4 = 9,
            ConfigCompass_5 = 10,
            SelfTestCompass = 11,
            WaitForGyroStable = 12,
            AdditionalAccelAdjust = 13,
            Idle = 14,
            Calibration = 15,
            LedInstrum = 16,
            Error = 31,
        };
        /** sub command for the various Set param enums */
        enum TareType
        {
            SetValue = 0x00, AddOffset = 0x01, MatchCompass = 0x02, SetOffset = 0xFF,
        };

        


public PigeonIMU_LowLevel(int baseArbId) :

            base((uint)baseArbId, (uint)baseArbId | COND_STATUS_5, (uint)baseArbId | PARAM_REQUEST, (uint)baseArbId | PARAM_RESPONSE,
                 (uint)baseArbId | PARAM_SET, (uint)baseArbId | COND_STATUS_13)
        {
            /* fill description */
            System.Text.StringBuilder work = new System.Text.StringBuilder();
            work.Append("Pigeon IMU ");
            switch (_baseArbId & 0xFF000000)
            {
                case 0x02000000:
                    work.Append("(Talon SRX ");
                    work.Append(GetDeviceNumber());
                    work.Append(")");
                    break;
                case 0x15000000:
                    work.Append(GetDeviceNumber());
                    break;
                default:
                    break;
            }
            SetDescription(work.ToString());

            CTRE.Native.CAN.Send(CONTROL_1 | _baseArbId, 0x00, 2, 50); /* two bytes, all zeros */
        }

        new void CheckFirmVers(int minMajor = kMinFirmwareVersionMajor, int minMinor = kMinFirmwareVersionMinor, ErrorCode failcode = ErrorCode.FirmwareTooOld)
        {
            base.CheckFirmVers(minMajor, minMinor, failcode);
        }
        public ErrorCode SetYaw(float angleDeg, int timeoutMs)
        {
            ErrorCode errCode = ConfigSetWrapper(ParamEnum.eYawOffset, TareType.SetValue, angleDeg, timeoutMs);
            return SetLastError(errCode);
        }
        /**
         * Atomically add to the Yaw register.
         */
        public ErrorCode AddYaw(float angleDeg, int timeoutMs)
        {
            ErrorCode errCode = ConfigSetWrapper(ParamEnum.eYawOffset, TareType.AddOffset, angleDeg, timeoutMs);
            return SetLastError(errCode);
        }
        public ErrorCode SetYawToCompass(int timeoutMs)
        {
            ErrorCode errCode = ConfigSetWrapper(ParamEnum.eYawOffset, TareType.MatchCompass, 0, timeoutMs);
            return SetLastError(errCode);
        }
        public ErrorCode SetFusedHeading(float angleDeg, int timeoutMs)
        {
            ErrorCode errCode = ConfigSetWrapper(ParamEnum.eFusedHeadingOffset, TareType.SetValue, angleDeg, timeoutMs);
            return SetLastError(errCode);
        }
        public ErrorCode SetAccumZAngle(float angleDeg, int timeoutMs)
        {
            ErrorCode errCode = ConfigSetWrapper(ParamEnum.eAccumZ, TareType.SetValue, angleDeg, timeoutMs);
            return SetLastError(errCode);
        }
        /**
         * Enable/Disable Temp compensation.  Pigeon defaults with this on at boot.
         * @param tempCompEnable
         * @return nonzero for error, zero for success.
         */
        public ErrorCode ConfigTemperatureCompensationDisable(bool bTempCompDisable, int timeoutMs)
        {
            ErrorCode errCode = ConfigSetWrapper(ParamEnum.eTempCompDisable, bTempCompDisable ? 1 : 0, timeoutMs);
            return SetLastError(errCode);
        }
        /**
         * Atomically add to the Fused Heading register.
         */
        public ErrorCode AddFusedHeading(float angleDeg, int timeoutMs)
        {
            ErrorCode errCode = ConfigSetWrapper(ParamEnum.eFusedHeadingOffset, TareType.AddOffset, angleDeg, timeoutMs);
            return SetLastError(errCode);
        }
        public ErrorCode SetFusedHeadingToCompass(int timeoutMs)
        {
            ErrorCode errCode = ConfigSetWrapper(ParamEnum.eFusedHeadingOffset, TareType.MatchCompass, 0, timeoutMs);
            return SetLastError(errCode);
        }
        /**
         * Set the declination for compass.
         * Declination is the difference between Earth Magnetic north, and the geographic "True North".
         */
        public ErrorCode SetCompassDeclination(float angleDegOffset, int timeoutMs)
        {
            return SetLastError(ErrorCode.FeatureNotSupported);
            //ErrorCode errCode = ConfigSetParameter(ParamEnum.eCompassOffset, TareType.SetOffset, 0, timeoutMs));
            //return SetLastError(errCode);
        }
        /**
         * Sets the compass angle.
         * Although compass is absolute [0,360) degrees, the continuous compass
         * register holds the wrap-arounds.
         */
        public ErrorCode SetCompassAngle(float angleDeg, int timeoutMs)
        {
            return SetLastError(ErrorCode.FeatureNotSupported);
            //ErrorCode errCode = ConfigSetParameter(ParamEnum.eCompassOffset, TareType.SetValue, 0, timeoutMs));
            //return SetLastError(errCode);
        }
        //----------------------- Calibration routines -----------------------//
        public ErrorCode EnterCalibrationMode(CalibrationMode calMode, int timeoutMs)
        {
            ErrorCode errCode = ConfigSetWrapper(ParamEnum.eEnterCalibration, (int)calMode, timeoutMs);
            return SetLastError(errCode);
        }
        /**
         * Get the status of the current (or previousley complete) calibration.
         * @param statusToFill
         */
        public ErrorCode GetGeneralStatus(GeneralStatus statusToFill)
        {
            ErrorCode errCode = (ErrorCode)ReceiveCAN(COND_STATUS_1 | (int)_baseArbId);

            byte b3 = (byte)(_cache >> 0x18);
            byte b5 = (byte)(_cache >> 0x28);

            byte iCurrMode = (byte)((b5 >> 4) & 0xF);
            CalibrationMode currentMode = (CalibrationMode)(iCurrMode);

            /* shift up bottom nibble, and back down with sign-extension */
            int calibrationErr = b5 & 0xF;
            calibrationErr <<= (32 - 4);
            calibrationErr >>= (32 - 4);

            int noMotionBiasCount = (byte)(_cache >> 0x24) & 0xF;
            int tempCompensationCount = (byte)(_cache >> 0x20) & 0xF;
            int upTimSec = (byte)(_cache >> 0x38);

            statusToFill.currentMode = currentMode;
            statusToFill.calibrationError = calibrationErr;
            statusToFill.bCalIsBooting = ((b3 & 1) == 1);
            statusToFill.state = GetState((int)errCode, _cache);
            statusToFill.tempC = (float)GetTemp(_cache);
            statusToFill.noMotionBiasCount = noMotionBiasCount;
            statusToFill.tempCompensationCount = tempCompensationCount;
            statusToFill.upTimeSec = upTimSec;
            statusToFill.lastError = errCode;

            /* build description string */
            if (errCode != 0)
            { // same as NoComm
                statusToFill.description = "Status frame was not received, check wired connections and web-based config.";
            }
            else if (statusToFill.bCalIsBooting)
            {
                statusToFill.description = "Pigeon is boot-caling to properly bias accel and gyro.  Do not move Pigeon.  When finished biasing, calibration mode will start.";
            }
            else if (statusToFill.state == PigeonState.UserCalibration)
            {
                /* mode specific descriptions */
                switch (currentMode)
                {
                    case CalibrationMode.BootTareGyroAccel:
                        statusToFill.description = "Boot-Calibration: Gyro and Accelerometer are being biased.";
                        break;
                    case CalibrationMode.Temperature:
                        statusToFill.description = "Temperature-Calibration: Pigeon is collecting temp data and will finish when temp range is reached.  Do not move Pigeon.";
                        break;
                    case CalibrationMode.Magnetometer12Pt:
                        statusToFill.description = "Magnetometer Level 1 calibration: Orient the Pigeon PCB in the 12 positions documented in the User's Manual.";
                        break;
                    case CalibrationMode.Magnetometer360:
                        statusToFill.description = "Magnetometer Level 2 calibration: Spin robot slowly in 360' fashion.  ";
                        break;
                    case CalibrationMode.Accelerometer:
                        statusToFill.description = "Accelerometer Calibration: Pigeon PCB must be placed on a level source.  Follow User's Guide for how to level surfacee.  ";
                        break;
                }
            }
            else if (statusToFill.state == PigeonState.Ready)
            {
                /* definitely not doing anything cal-related.  So just instrument the motion driver state */
                statusToFill.description = "Pigeon is running normally.  Last CAL error code was ";
                statusToFill.description += calibrationErr;
                statusToFill.description += ".";
            }
            else if (statusToFill.state == PigeonState.Initializing)
            {
                /* definitely not doing anything cal-related.  So just instrument the motion driver state */
                statusToFill.description = "Pigeon is boot-caling to properly bias accel and gyro.  Do not move Pigeon.";
            }
            else
            {
                statusToFill.description = "Not enough data to determine status.";
            }



            return SetLastError(errCode);
        }
        public ErrorCode GetGeneralStatus(out int state, out int currentMode, out int calibrationError, out int bCalIsBooting, out float tempC, out int upTimeSec, out int noMotionBiasCount, out int tempCompensationCount, out ErrorCode lastError)
        {
            GeneralStatus statusToFill = new GeneralStatus();
            ErrorCode errCode = GetGeneralStatus(statusToFill);

            state = (int)statusToFill.state;
            currentMode = (int)statusToFill.currentMode;
            calibrationError = statusToFill.calibrationError;
            bCalIsBooting = statusToFill.bCalIsBooting ? 1 : 0;
            tempC = statusToFill.tempC;
            upTimeSec = statusToFill.upTimeSec;
            noMotionBiasCount = statusToFill.noMotionBiasCount;
            tempCompensationCount = statusToFill.tempCompensationCount;
            lastError = statusToFill.lastError;

            return SetLastError(errCode);
        }

        //----------------------- General Signal decoders  -----------------------//
        private int ReceiveCAN(int arbId)
        {
            return ReceiveCAN(arbId, true);
        }
        private int ReceiveCAN(int arbId, bool allowStale)
        {
            int errCode = CTRE.Native.CAN.Receive((uint)arbId | GetDeviceNumber(), ref _cache, ref _len);
            if (errCode == 1)
            {
                /* frame is stale */
                if (allowStale)
                    return 0; /* signal ok */
                else
                    return 1; /* error code of 1 */
            }
            return errCode;
        }

        /**
         * Decode two 16bit parameters.
         */
        ErrorCode GetTwoParam16(int arbId, short[] words)
        {
            if(words.Length < 2) { return ErrorCode.InvalidParamValue; }
            ErrorCode errCode = (ErrorCode)ReceiveCAN(arbId);
            /* always give caller the latest */
            words[0] = (short)((byte)(_cache));
            words[0] <<= 8;
            words[0] |= (short)((byte)(_cache >> 0x08));

            words[1] = (short)((byte)(_cache >> 0x10));
            words[1] <<= 8;
            words[1] |= (short)((byte)(_cache >> 0x18));

            return errCode;
        }
        ErrorCode GetThreeParam16(int arbId, short[] words)
        {
            if (words.Length < 3) { return ErrorCode.InvalidParamValue; }
            ErrorCode errCode = (ErrorCode)ReceiveCAN(arbId);

            words[0] = (short)((byte)(_cache));
            words[0] <<= 8;
            words[0] |= (short)((byte)(_cache >> 0x08));

            words[1] = (short)((byte)(_cache >> 0x10));
            words[1] <<= 8;
            words[1] |= (short)((byte)(_cache >> 0x18));

            words[2] = (short)((byte)(_cache >> 0x20));
            words[2] <<= 8;
            words[2] |= (short)((byte)(_cache >> 0x28));
            return errCode;
        }

        ErrorCode GetThreeParam16(int arbId, float[] signals, float scalar)
        {
            if (signals.Length < 3) { return ErrorCode.InvalidParamValue; }
            short word_p1;
            short word_p2;
            short word_p3;
            ErrorCode errCode = (ErrorCode)ReceiveCAN(arbId);

            word_p1 = (short)((byte)(_cache));
            word_p1 <<= 8;
            word_p1 |= (short)((byte)(_cache >> 0x08));

            word_p2 = (short)((byte)(_cache >> 0x10));
            word_p2 <<= 8;
            word_p2 |= (short)((byte)(_cache >> 0x18));

            word_p3 = (short)((byte)(_cache >> 0x20));
            word_p3 <<= 8;
            word_p3 |= (short)((byte)(_cache >> 0x28));

            signals[0] = word_p1 * scalar;
            signals[1] = word_p2 * scalar;
            signals[2] = word_p3 * scalar;

            return errCode;
        }

        ErrorCode GetThreeBoundedAngles(int arbId, float[] boundedAngles)
        {
            if (boundedAngles.Length < 3) { return ErrorCode.InvalidParamValue; }
            return GetThreeParam16(arbId, boundedAngles, 360f / 32768f);
        }
        ErrorCode GetFourParam16(int arbId, float[] parameters, float scalar)
        {
            if (parameters.Length < 4) { return ErrorCode.InvalidParamValue; }
            short p0, p1, p2, p3;
            ErrorCode errCode = (ErrorCode)ReceiveCAN(arbId);

            p0 = (short)((byte)(_cache));
            p0 <<= 8;
            p0 |= (short)((byte)(_cache >> 0x08));

            p1 = (short)((byte)(_cache >> 0x10));
            p1 <<= 8;
            p1 |= (short)((byte)(_cache >> 0x18));

            p2 = (short)((byte)(_cache >> 0x20));
            p2 <<= 8;
            p2 |= (short)((byte)(_cache >> 0x28));

            p3 = (short)((byte)(_cache >> 0x30));
            p3 <<= 8;
            p3 |= (short)((byte)(_cache >> 0x38));

	        /* always give caller the latest */
	        parameters[0] = p0* scalar;
	        parameters[1] = p1* scalar;
	        parameters[2] = p2* scalar;
	        parameters[3] = p3* scalar;


	        return errCode;
        }

        ErrorCode GetThreeParam20(int arbId, float[] parameters, float scalar)
        {
            if (parameters.Length < 3) { return ErrorCode.InvalidParamValue; }
            int p1, p2, p3;

            ErrorCode errCode = (ErrorCode)ReceiveCAN(arbId);

            byte p1_h8 = (byte)_cache;
            byte p1_m8 = (byte)(_cache >> 8);
            byte p1_l4 = (byte)(_cache >> 20);

            byte p2_h4 = (byte)(_cache >> 16);
            byte p2_m8 = (byte)(_cache >> 24);
            byte p2_l8 = (byte)(_cache >> 32);

            byte p3_h8 = (byte)(_cache >> 40);
            byte p3_m8 = (byte)(_cache >> 48);
            byte p3_l4 = (byte)(_cache >> 60);

            p1_l4 &= 0xF;
            p2_h4 &= 0xF;
            p3_l4 &= 0xF;

            p1 = p1_h8;
            p1 <<= 8;
            p1 |= p1_m8;
            p1 <<= 4;
            p1 |= p1_l4;
            p1 <<= (32 - 20);
            p1 >>= (32 - 20);

            p2 = p2_h4;
            p2 <<= 8;
            p2 |= p2_m8;
            p2 <<= 8;
            p2 |= p2_l8;
            p2 <<= (32 - 20);
            p2 >>= (32 - 20);

            p3 = p3_h8;
            p3 <<= 8;
            p3 |= p3_m8;
            p3 <<= 4;
            p3 |= p3_l4;
            p3 <<= (32 - 20);
            p3 >>= (32 - 20);

	        parameters[0] = p1* scalar;
	        parameters[1] = p2* scalar;
	        parameters[2] = p3* scalar;

	        return errCode;
        }
        //----------------------- Strongly typed Signal decoders  -----------------------//
        public ErrorCode Get6dQuaternion(float[] wxyz)
        {
            if (wxyz.Length < 4) { return ErrorCode.InvalidParamValue; }
            ErrorCode errCode = GetFourParam16(COND_STATUS_10 | (int)_baseArbId, wxyz, 1.0f / 16384f);
            return SetLastError(errCode);
        }
        public ErrorCode GetYawPitchRoll(float[] ypr)
        {
            if (ypr.Length < 3) { return ErrorCode.InvalidParamValue; }
            CheckFirmVers();
            ErrorCode errCode = GetThreeParam20(COND_STATUS_9 | (int)_baseArbId, ypr, (360f / 8192f));
            return SetLastError(errCode);
        }
        public ErrorCode GetAccumGyro(float[] xyz_deg)
        {
            if (xyz_deg.Length < 3) { return ErrorCode.InvalidParamValue; }
            ErrorCode errCode = GetThreeParam20(COND_STATUS_11 | (int)_baseArbId, xyz_deg, (360f / 8192f));
            return SetLastError(errCode);
        }
        /**
         *  @return compass heading [0,360) degrees.
         */
        public ErrorCode GetAbsoluteCompassHeading(out float value)
        {
            int raw;
            float retval;
            ErrorCode errCode = (ErrorCode)ReceiveCAN(COND_STATUS_2 | (int)_baseArbId);

            byte m8 = (byte)((_cache >> 0x30) & 0xFF);
            byte l8 = (byte)((_cache >> 0x38) & 0xFF);

            raw = m8;
            raw <<= 8;
            raw |= l8;
            raw &= 0x1FFF;

            retval = raw * (360f / 8192f);

            value = retval;
            return SetLastError(errCode);
        }
        /**
         *  @return continuous compass heading [-23040, 23040) degrees.
         *  Use SetCompassHeading to modify the wrap-around portion.
         */
        public ErrorCode GetCompassHeading(out float value)
        {
            int raw;
            float retval;
            ErrorCode errCode = (ErrorCode)ReceiveCAN(COND_STATUS_2 | (int)_baseArbId);
            byte h4 = (byte)((_cache >> 0x28) & 0xF);
            byte m8 = (byte)((_cache >> 0x30) & 0xFF);
            byte l8 = (byte)((_cache >> 0x38) & 0xFF);

            raw = h4;
            raw <<= 8;
            raw |= m8;
            raw <<= 8;
            raw |= l8;
            raw <<= (32 - 20);
            raw >>= (32 - 20);

            retval = raw * (360f / 8192f);

            value = retval;
            return SetLastError(errCode);
        }
        /**
         * @return field strength in Microteslas (uT).
         */
        public ErrorCode GetCompassFieldStrength(out float value)
        {
            float magnitudeMicroTeslas;
            short[] words = new short[2];
            ErrorCode errCode = GetTwoParam16(COND_STATUS_2, words);
            magnitudeMicroTeslas = words[1] * (0.15f);

            value = magnitudeMicroTeslas;
            return SetLastError(errCode);
        }

        public float GetTemp(ulong statusFrame)
        {
            byte H = (byte)(statusFrame >> 0);
            byte L = (byte)(statusFrame >> 8);
            int raw = 0;
            raw |= H;
            raw <<= 8;
            raw |= L;
            float tempC = raw * (1.0f / 256.0f);
            return tempC;
        }
        public ErrorCode GetTemp(out float value)
        {
            ErrorCode errCode = (ErrorCode)ReceiveCAN(COND_STATUS_1 | (int)_baseArbId);
            float tempC = GetTemp(_cache);

            value = tempC;
            return SetLastError(errCode);
        }
        public PigeonState GetState(int errCode, ulong statusFrame)
        {
            CheckFirmVers();
            PigeonState retval = PigeonState.NoComm;

            if (errCode != 0)
            {
                /* bad frame */
            }
            else
            {
                /* good frame */
                byte b2 = (byte)(statusFrame >> 0x10);

                MotionDriverState mds = (MotionDriverState)(b2 & 0x1f);
                switch (mds)
                {
                    case MotionDriverState.Error:
                    case MotionDriverState.Init0:
                    case MotionDriverState.WaitForPowerOff:
                    case MotionDriverState.ConfigAg:
                    case MotionDriverState.SelfTestAg:
                    case MotionDriverState.StartDMP:
                    case MotionDriverState.ConfigCompass_0:
                    case MotionDriverState.ConfigCompass_1:
                    case MotionDriverState.ConfigCompass_2:
                    case MotionDriverState.ConfigCompass_3:
                    case MotionDriverState.ConfigCompass_4:
                    case MotionDriverState.ConfigCompass_5:
                    case MotionDriverState.SelfTestCompass:
                    case MotionDriverState.WaitForGyroStable:
                    case MotionDriverState.AdditionalAccelAdjust:
                        retval = PigeonState.Initializing;
                        break;
                    case MotionDriverState.Idle:
                        retval = PigeonState.Ready;
                        break;
                    case MotionDriverState.Calibration:
                    case MotionDriverState.LedInstrum:
                        retval = PigeonState.UserCalibration;
                        break;
                    default:
                        retval = PigeonState.Initializing;
                        break;
                }
            }
            return retval;
        }
        public PigeonState GetState()
        {
            int errCode = ReceiveCAN(COND_STATUS_1 | (int)_baseArbId);
            PigeonState retval = GetState(errCode, _cache);
            SetLastError((ErrorCode)errCode);
            return retval;
        }
        public ErrorCode GetState(out int state)
        {
            int errCode = ReceiveCAN(COND_STATUS_1 | (int)_baseArbId);
            PigeonState retval = GetState(errCode, _cache);
            state = (int)retval;
            return SetLastError((ErrorCode)errCode);
        }
        /// <summary>
        /// How long has Pigeon been running
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ErrorCode GetUpTime(out int value)
        {
            /* repoll status frame */
            ErrorCode errCode = (ErrorCode)ReceiveCAN(COND_STATUS_1 | (int)_baseArbId);
            value = (byte)(_cache >> 56);
            return SetLastError(errCode);
        }

        public ErrorCode GetRawMagnetometer(short[] rm_xyz)
        {
            if (rm_xyz.Length < 3) { return ErrorCode.InvalidParamValue; }
            ErrorCode errCode = GetThreeParam16(RAW_STATUS_4 | (int)_baseArbId, rm_xyz);
            return SetLastError(errCode);
        }
        public ErrorCode GetBiasedMagnetometer(short[] bm_xyz)
        {
            if (bm_xyz.Length < 3) { return ErrorCode.InvalidParamValue; }
            ErrorCode errCode = GetThreeParam16(BIASED_STATUS_4 | (int)_baseArbId, bm_xyz);
            return SetLastError(errCode);
        }
        public ErrorCode GetBiasedAccelerometer(short[] ba_xyz)
        {
            if (ba_xyz.Length < 3) { return ErrorCode.InvalidParamValue; }
            ErrorCode errCode = GetThreeParam16(BIASED_STATUS_6 | (int)_baseArbId, ba_xyz);
            return SetLastError(errCode);
        }
        public ErrorCode GetRawGyro(float[] xyz_dps)
        {
            if (xyz_dps.Length < 3) { return ErrorCode.InvalidParamValue; }
            ErrorCode errCode = GetThreeParam16(BIASED_STATUS_2 | (int)_baseArbId, xyz_dps, 1.0f / 16.4f);
            return SetLastError(errCode);
        }

        public ErrorCode GetAccelerometerAngles(float[] tiltAngles)
        {
            if (tiltAngles.Length < 3) { return ErrorCode.InvalidParamValue; }
            ErrorCode errCode = GetThreeBoundedAngles(COND_STATUS_3 | (int)_baseArbId, tiltAngles);
            return SetLastError(errCode);
        }
        /**
         * @param status 	object reference to fill with fusion status flags.  
         *					Caller may omit this parameter if flags are not needed.
         * @return fused heading in degrees.
         */
        public ErrorCode GetFusedHeading(FusionStatus status, out float value)
        {
            bool bIsFusing, bIsValid;
            float[] temp = new float[3];
            float fusedHeading;

            ErrorCode errCode = GetThreeParam20(COND_STATUS_6, temp, 360f / 8192f);
            fusedHeading = temp[0];
            byte b2 = (byte)(_cache >> 16);

            string description;

            if (errCode != 0)
            {
                bIsFusing = false;
                bIsValid = false;
                description = "Could not receive status frame.  Check wiring and web-config.";
            }
            else
            {
                int flags = (b2) & 7;
                if (flags == 7)
                {
                    bIsFusing = true;
                }
                else
                {
                    bIsFusing = false;
                }

                if ((b2 & 0x8) == 0)
                {
                    bIsValid = false;
                }
                else
                {
                    bIsValid = true;
                }

                if (bIsValid == false)
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
            }

            /* fill caller's struct */
            status.heading = fusedHeading;
            status.bIsFusing = bIsFusing;
            status.bIsValid = bIsValid;
            status.description = description;
            status.lastError = errCode;

            value = fusedHeading;
            return SetLastError(errCode);
        }
        public ErrorCode GetFusedHeading(out float value)
        {
            FusionStatus temp = new FusionStatus();
            return GetFusedHeading(temp, out value);
        }
        public ErrorCode GetFusedHeading(out int bIsFusing, out int bIsValid, out float value, out ErrorCode lastError)
        {
            FusionStatus toFill = new FusionStatus();
            ErrorCode errCode = GetFusedHeading(toFill, out value);

            bIsFusing = toFill.bIsFusing ? 1 : 0;
            bIsValid = toFill.bIsValid ? 1 : 0;
            lastError = toFill.lastError;

            return errCode;
        }

        //-------------------------------- ToString functions -----------------------//
        /* static */
        public string ToString(PigeonState state)
        {
            string retval = "Unknown";
            switch (state)
            {
                case PigeonState.Initializing:
                    return "Initializing";
                case PigeonState.Ready:
                    return "Ready";
                case PigeonState.UserCalibration:
                    return "UserCalibration";
                case PigeonState.NoComm:
                    return "NoComm";
            }
            return retval;
        }
        /* static */
        public string ToString(CalibrationMode cm)
        {
            string retval = "Unknown";
            switch (cm)
            {
                case CalibrationMode.BootTareGyroAccel:
                    return "BootTareGyroAccel";
                case CalibrationMode.Temperature:
                    return "Temperature";
                case CalibrationMode.Magnetometer12Pt:
                    return "Magnetometer12Pt";
                case CalibrationMode.Magnetometer360:
                    return "Magnetometer360";
                case CalibrationMode.Accelerometer:
                    return "Accelerometer";
            }
            return retval;
        }

        //-------------------------------- Device_LowLevel requirements -----------------------//
        protected override void EnableFirmStatusFrame(bool enable)
        {
            CTRE.Native.CAN.GetSendBuffer(CONTROL_1 | _baseArbId, ref _cache);

            if (_cache == 0)
            {
                return;
            }

            byte b0 = (byte)(enable ? 0 : 1);
            b0 <<= 7;

            /* flush changes */
            CTRE.Native.CAN.Send(CONTROL_1 | _baseArbId, _cache, 8, 0xFFFFFFFF);

        }
        protected override ErrorCode SetLastError(ErrorCode errorCode)
        {
            _lastError.SetLastError(errorCode);
            return _lastError;
        }
        public ErrorCode GetLastError()
        {
            return _lastError;
        }
        //----------------------------------------------------------------------------------------//
        /** private wrapper to avoid specifying ordinal */
        ErrorCode ConfigSetWrapper(ParamEnum paramEnum, TareType tareType, float angleDeg, int timeoutMs)
        {
            byte subValue = (byte)tareType;
            int ordinal = 0;
            return base.ConfigSetParameter(paramEnum, angleDeg, subValue, ordinal, timeoutMs);
        }
        /** private wrapper to avoid specifying ordinal and subvalue */
        ErrorCode ConfigSetWrapper(ParamEnum paramEnum, float value, int timeoutMs)
        {
            byte subValue = 0;
            int ordinal = 0;
            return base.ConfigSetParameter(paramEnum, value, subValue, ordinal, timeoutMs);
        }

        //------ Faults ----------//
        public ErrorCode GetFaults(PigeonIMU_Faults toFill)
        {
            //toFill.XXX = YYY;
            return SetLastError(ErrorCode.FeatureNotSupported);
        }
        public ErrorCode GetStickyFaults(PigeonIMU_StickyFaults toFill)
        {
            //toFill.XXX = YYY;
            return SetLastError(ErrorCode.FeatureNotSupported);
        }
        public ErrorCode ClearStickyFaults(int timeoutMs)
        {
            return SetLastError(ErrorCode.FeatureNotSupported);
        }

        public bool HasResetOccurred()
        {
            return HasResetOccured();
        }

        //------ Custom Persistent Params ----------//
        public new ErrorCode ConfigSetCustomParam(int newValue, int paramIndex, int timeoutMs = 0)
        {
            return base.ConfigSetCustomParam(newValue, paramIndex, timeoutMs);
        }
        public new ErrorCode ConfigGetCustomParam(out int readValue, int paramIndex, int timeoutMs = Constants.GetParamTimeoutMs)
        {
            return base.ConfigGetCustomParam(out readValue, paramIndex, timeoutMs);
        }

        //----------------------------- Frame Period -----------------------------------------------------------//
        public ErrorCode SetStatusFramePeriod(Sensors.PigeonIMU_StatusFrame frame, int periodMs, int timeoutMs)
        {
            /* this API requires 2018 firmware */
            CheckFirmVers();
            int fullId = (int)((int)_baseArbId | (int)frame); /* build ID */
            fullId &= 0x00FFFFFF; /* throw out $15/$02 */
            fullId |= 0x15000000; /* force it to $15 */
            return SetStatusFramePeriod(fullId, periodMs, timeoutMs);
        }
        public ErrorCode GetStatusFramePeriod(Sensors.PigeonIMU_StatusFrame frame,
                out int periodMs, int timeoutMs)
        {
            /* this API requires 2018 firmware */
            CheckFirmVers();
            int fullId = (int)((int)_baseArbId | (int)frame); /* build ID */
            fullId &= 0x00FFFFFF; /* throw out $15/$02 */
            fullId |= 0x15000000; /* force it to $15 */
            return GetStatusFramePeriod(fullId, out periodMs, timeoutMs);
        }
        //ErrorCode SetControlFramePeriod(Sensors.PigeonIMU_ControlFrame frame,
        //        int periodMs)
        //{
        //    /* this API requires 2018 firmware */
        //    CheckFirmVers();

        //    /* sterilize inputs */
        //    if (periodMs < 0)
        //    {
        //        periodMs = 0;
        //    }
        //    if (periodMs > 0xFF)
        //    {
        //        periodMs = 0xFF;
        //    }

        //    uint fullId = (uint)((int)_baseArbId | (int)frame); /* build ID */

        //    /* change period and report error */
        //    bool succ = GetMgr().ChangeTxPeriod(fullId, periodMs);
        //    return SetLastError(succ ? ErrorCode.OKAY : ErrorCode.CouldNotChangePeriod);
        //}
    }
}
