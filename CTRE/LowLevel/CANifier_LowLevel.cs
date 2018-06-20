using System;
using Microsoft.SPOT;
using CTRE.Phoenix.LowLevel;

namespace CTRE.Phoenix.LowLevel
{
    public class CANifier_LowLevel : Device_LowLevel
    {
        const UInt32 STATUS_1 = 0x03041400;
        const UInt32 STATUS_2 = 0x03041440;
        const UInt32 STATUS_3 = 0x03041480;
        const UInt32 STATUS_4 = 0x030414C0;
        const UInt32 STATUS_5 = 0x03041500;
        const UInt32 STATUS_6 = 0x03041540;
        const UInt32 STATUS_7 = 0x03041580;
        const UInt32 STATUS_8 = 0x030415C0;
        const UInt32 STATUS_9 = 0x03041600;

        const UInt32 CANifier_Control_1_General_20Ms = 0x03040000;
        const UInt32 CANifier_Control_2_PwmOutput = 0x03040040;

        const UInt32 PARAM_REQUEST = 0x03041800;
        const UInt32 PARAM_RESPONSE = 0x03041840;
        const UInt32 PARAM_SET = 0x03041880;

        const UInt32 kParamArbIdValue = PARAM_RESPONSE;
        const UInt32 kParamArbIdMask = 0xFFFFFFFF;

        private UInt64 _cache;
        private UInt32 _len;

        private System.Collections.Hashtable _sigs = new System.Collections.Hashtable();

        private uint _regInput = 0; //!< Decoded inputs
        private uint _regLat = 0; //!< Decoded output latch
        private uint _regIsOutput = 0; //!< Decoded data direction register

        const int kMinFirmwareVersionMajor = 0;
        const int kMinFirmwareVersionMinor = 40;

        public enum GeneralPin
        {
            QUAD_IDX = (0),
            QUAD_B = (1),
            QUAD_A = (2),
            LIMR = (3),
            LIMF = (4),
            SDA = (5),
            SCL = (6),
            SPI_CS = (7),
            SPI_MISO_PWM2P = (8),
            SPI_MOSI_PWM1P = (9),
            SPI_CLK_PWM0P = (10),
        }

        private const int kTotalGeneralPins = 11;

        bool _SendingPwmOutput = false;

        public CANifier_LowLevel(UInt16 deviceId, bool externalEnable = false) : 
            base((uint)0x03040000 | deviceId, STATUS_7 | deviceId, PARAM_REQUEST | deviceId, PARAM_RESPONSE | deviceId, PARAM_SET | deviceId, (uint)0x03041600 | deviceId) // todo startup frame
        {
            if (false == externalEnable)
            {
                CTRE.Native.CAN.Send(CANifier_Control_1_General_20Ms | _baseArbId, 0x00, 8, 20);
            }
        }
        new void CheckFirmVers(int minMajor = kMinFirmwareVersionMajor, int minMinor = kMinFirmwareVersionMinor, ErrorCode failCode = ErrorCode.FirmwareTooOld)
        {
            base.CheckFirmVers(minMajor, minMinor, failCode);
        }
        
        //-------------------------------- frame decoders -----------------------//
        private void EnsurePwmOutputFrameIsTransmitting()
        {
            if (false == _SendingPwmOutput)
            {
                _SendingPwmOutput = true;

                ulong frame = 0x0000000000000000;
                CTRE.Native.CAN.Send(CANifier_Control_2_PwmOutput | _baseArbId, frame, 8, 10);
            }
        }

        private static int DecodeRegLat(ulong frame)
        {
            byte b5 = (byte)(frame >> 40);
            byte b6 = (byte)(frame >> 48);
            int retval = b5;
            retval <<= 3;
            retval |= b6 >> 5;
            return retval;
        }
        private static int DecodeRegIsOutput(ulong frame)
        {
            byte b6 = (byte)(frame >> 48);
            byte b7 = (byte)(frame >> 56);
            int retval = b6 & 0x7;
            retval <<= 8;
            retval |= b7;
            return retval;
        }
        private static int DecodeRegInput(ulong frame)
        {
            byte b5 = (byte)(frame >> 40);
            byte b6 = (byte)(frame >> 48);
            int retval = b5;
            retval &= 0x7;
            retval <<= 8;
            retval |= b6;
            return retval;
        }

        private static void Set10Bitvalue(int value_10, int position_0123, ref UInt64 frame)
        {
            /* serialize parts */
            int C0_h8 = value_10 >> 2;
            int C0_l2 = value_10;
            int C1_h6 = value_10 >> 4;
            int C1_l4 = value_10;
            int C2_h4 = value_10 >> 6;
            int C2_l6 = value_10;
            int C3_h2 = value_10 >> 8;
            int C3_l8 = value_10;
            C0_h8 &= 0xFF;
            C0_l2 &= 0x03;
            C1_h6 &= 0x3F;
            C1_l4 &= 0x0F;
            C2_h4 &= 0x0F;
            C2_l6 &= 0x3F;
            C3_h2 &= 0x03;
            C3_l8 &= 0xFF;
            /* serialize it */
            switch (position_0123)
            {
                case 0:
                    frame &= ~(0xFFul);
                    frame &= ~(0x03ul << (8 + 6));
                    frame |= (UInt64)(byte)(C0_h8);
                    frame |= (UInt64)(C0_l2) << (8 + 6);
                    break;
                case 1:
                    frame &= ~(0x3Ful << 8);
                    frame &= ~(0xFul << (16 + 4));
                    frame |= (UInt64)(C1_h6) << 8;
                    frame |= (UInt64)(C1_l4) << (16 + 4);
                    break;
                case 2:
                    frame &= ~(0x0Ful << 16);
                    frame &= ~(0x3Ful << (24 + 2));
                    frame |= (UInt64)(C2_h4) << 16;
                    frame |= (UInt64)(C2_l6) << (24 + 2);
                    break;
                case 3:
                    frame &= ~(0x03ul << 24);
                    frame &= ~(0xFFul << (32));
                    frame |= (UInt64)(C3_h2) << 24;
                    frame |= (UInt64)(C3_l8) << (32);
                    break;
            }
        }
        
        public ErrorCode SetLEDOutput(int dutyCycle, uint ledChannel)
        {
            int retval = CTRE.Native.CAN.GetSendBuffer(CANifier_Control_1_General_20Ms | _baseArbId, ref _cache);
            if (retval != 0)
                return (ErrorCode)retval;
            /* serialize it */
            switch (ledChannel)
            {
                case 0:
                    Set10Bitvalue(dutyCycle, 0, ref _cache);
                    break;
                case 1:
                    Set10Bitvalue(dutyCycle, 1, ref _cache);
                    break;
                case 2:
                    Set10Bitvalue(dutyCycle, 2, ref _cache);
                    break;
            }
            /* save it */
            CTRE.Native.CAN.Send(CANifier_Control_1_General_20Ms | _baseArbId, _cache, 8, 0xFFFFFFFF);
            return SetLastError(retval);
        }
        public ErrorCode SetGeneralOutputs(UInt32 outputsBits, UInt32 isOutputBits)
        {
            /* sterilize inputs */
            outputsBits &= 0x7FF;
            isOutputBits &= 0x7FF;
            /* save values into registers*/
            _regLat = outputsBits;
            _regIsOutput = isOutputBits;
            /* get tx message */
            int retval = CTRE.Native.CAN.GetSendBuffer(CANifier_Control_1_General_20Ms | _baseArbId, ref _cache);
            if (retval != 0)
                return (ErrorCode)retval;
            /* calc bytes 5,6,7 */
            byte b5 = (byte)(_regLat >> 3);
            byte b6 = (byte)((_regLat & 7) << 5);
            b6 |= (byte)(_regIsOutput >> 8);
            byte b7 = (byte)(_regIsOutput);

            _cache &= ~0xFFFFFF0000000000;
            _cache |= ((ulong)b5) << 40;
            _cache |= ((ulong)b6) << 48;
            _cache |= ((ulong)b7) << 56;

            /* save it */
            CTRE.Native.CAN.Send(CANifier_Control_1_General_20Ms | _baseArbId, _cache, 8, 0xFFFFFFFF);
            return SetLastError(retval);

        }
        public ErrorCode SetGeneralOutput(GeneralPin outputPin, bool outputValue, bool outputEnable)
        {
            /* calc bitpos from caller's selected ch */
            uint mask = (uint)1 << (int)outputPin;

            /* update regs based on caller's params */
            if (outputValue)
                _regLat |= mask;
            else
                _regLat &= ~mask;

            if (outputEnable)
                _regIsOutput |= mask;
            else
                _regIsOutput &= ~mask;

            return SetLastError(SetGeneralOutputs(_regLat, _regIsOutput));
        }

        public ErrorCode SetPWMOutput(uint pwmChannel, int dutyCycle)
        {
            /* start transmitting if not done yet */
            EnsurePwmOutputFrameIsTransmitting();

            ErrorCode retval = (ErrorCode)CTRE.Native.CAN.GetSendBuffer(CANifier_Control_2_PwmOutput | _baseArbId, ref _cache);
            if (retval != ErrorCode.OK)
                return retval;
            /* serialize it */
            switch (pwmChannel)
            {
                case 0:
                    Set10Bitvalue(dutyCycle, 0, ref _cache);
                    break;
                case 1:
                    Set10Bitvalue(dutyCycle, 1, ref _cache);
                    break;
                case 2:
                    Set10Bitvalue(dutyCycle, 2, ref _cache);
                    break;
                case 3:
                    Set10Bitvalue(dutyCycle, 3, ref _cache);
                    break;
            }
            /* save it */
            CTRE.Native.CAN.Send(CANifier_Control_2_PwmOutput | _baseArbId, _cache, 8, 0xFFFFFFFF);
            return SetLastError(retval);
        }
        private byte SetBit(byte value, int bitpos)
        {
            int ret =  value | (1 << bitpos);
            return (byte)ret;
        }
        private byte ClrBit(byte value, int bitpos)
        {
            int ret = value & ~(1 << bitpos);
            return (byte)ret;
        }
        private byte SetClrBit(byte value, int bitpos, bool bSet)
        {
            if (bSet) return SetBit(value, bitpos);
            return ClrBit(value, bitpos);
        }

        public ErrorCode EnablePWMOutput(uint pwmChannel, bool bEnable)
        {
            /* start transmitting if not done yet */
            EnsurePwmOutputFrameIsTransmitting();

            int retval = CTRE.Native.CAN.GetSendBuffer(CANifier_Control_2_PwmOutput | _baseArbId, ref _cache);
            if (retval != 0)
                return (ErrorCode)retval;

            byte b7 = (byte)(_cache >> 56);

            /* serialize it */
            switch (pwmChannel)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    b7 = SetClrBit(b7, 4 + (int)pwmChannel, bEnable);
                    break;
            }
            /* build cache */
            _cache = b7;
            _cache <<= 56;
            /* save it */
            CTRE.Native.CAN.Send(CANifier_Control_2_PwmOutput | _baseArbId, _cache, 8, 0xFFFFFFFF);
            return SetLastError(retval);
        }
        public uint GetGeneralInputs()
        {
            int err = CTRE.Native.CAN.Receive(STATUS_2 | _baseArbId, ref _cache, ref _len);

            _regInput = (uint)DecodeRegInput(_cache);

            SetLastError(err);
            return _regInput;
        }

        public ErrorCode GetGeneralInputs(int [] allPins)
        {
            int retval = CTRE.Native.CAN.Receive(STATUS_2 | _baseArbId, ref _cache, ref _len);

            _regInput = (uint)DecodeRegInput(_cache);

            for (int i=0;i< kTotalGeneralPins;++i)
            {
                int mask = 1 << i;
                if (((_regInput & mask) != 0))
                {
                    allPins[i] = 1;
                } else
                {
                    allPins[i] = 0;
                }
            }
            return SetLastError(retval);
        }
        public bool GetGeneralInput(GeneralPin inputPin)
        {
            int retval = CTRE.Native.CAN.Receive(STATUS_2 | _baseArbId, ref _cache, ref _len);
            SetLastError(retval);

            int mask = 1 << (int)inputPin;
            _regInput = (uint)DecodeRegInput(_cache);

            if (((_regInput & mask) != 0))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private int GetPwmInput_X(uint ArbIDField, uint [] waveform)
        {
            uint pulseWidthFrac;
            uint periodRaw;
            int retval = CTRE.Native.CAN.Receive(ArbIDField | _baseArbId, ref _cache, ref _len);
            byte b0 = (byte)(_cache >> 0x00);
            byte b1 = (byte)(_cache >> 0x08);
            byte b2 = (byte)(_cache >> 0x10);
            byte b3 = (byte)(_cache >> 0x18);
            byte b4 = (byte)(_cache >> 0x20);
            byte b5= (byte)(_cache >> 0x28);

            pulseWidthFrac = b0;
            pulseWidthFrac <<= 8;
            pulseWidthFrac |= b1;
            pulseWidthFrac <<= 8;
            pulseWidthFrac |= b2;

            periodRaw = b3;
            periodRaw <<= 8;
            periodRaw |= b4;
            periodRaw <<= 3;
            periodRaw |= (byte)(b5 >> 5);

            waveform[0] = pulseWidthFrac;
            waveform[1] = periodRaw;

            return retval;
        }
        public ErrorCode GetPwmInput(uint pwmChannel, float [] dutyCycleAndPeriod)
        {
            uint[] temp = new uint[2] { 0, 0 };
            int retval = (int)ErrorCode.CAN_INVALID_PARAM;

            float pulseWidthUs;
            float periodUs;

            switch (pwmChannel)
            {
                case 0:
                    retval = GetPwmInput_X(STATUS_3, temp); // ref pulseWidthFrac, ref periodRaw);
                    break;
                case 1:
                    retval = GetPwmInput_X(STATUS_4, temp); //ref pulseWidthFrac, ref periodRaw);
                    break;
                case 2:
                    retval = GetPwmInput_X(STATUS_5, temp); // ref pulseWidthFrac, ref periodRaw);
                    break;
                case 3:
                    retval = GetPwmInput_X(STATUS_6, temp); //ref pulseWidthFrac, ref periodRaw);
                    break;
            }


            /* scaling */
            uint pulseWidthFrac = temp[0], periodRaw = temp[1];
            periodUs = periodRaw * 0.256f; /* convert to microseconds */
            pulseWidthUs = pulseWidthFrac * 0.000244140625f * periodUs;

            dutyCycleAndPeriod[0] = pulseWidthUs;
            dutyCycleAndPeriod[1] = periodUs;

            return SetLastError(retval);
        }


        public float GetBatteryVoltage()
        {
            int err = CTRE.Native.CAN.Receive(STATUS_1 | _baseArbId, ref _cache, ref _len);

            byte b5 = (byte)(_cache >> 40);

            SetLastError(err);
            return b5 * 0.1f + 4f;
        }

        public ErrorCode GetQuadraturePosition(out int pos)
        {
            /* general firm check */
            CheckFirmVers(0, 42, ErrorCode.FeatureRequiresHigherFirm);

            int err = CTRE.Native.CAN.Receive(STATUS_2 | _baseArbId, ref _cache, ref _len);

            byte H = (byte)(_cache);
            byte M = (byte)(_cache >> 0x8);
            byte L = (byte)(_cache >> 0x10);
            int posDiv8 = (int)((_cache >> (0x28 + 7)) & 1);

            int raw = 0;
            raw |= H;
            raw <<= 8;
            raw |= M;
            raw <<= 8;
            raw |= L;

            raw <<= (32 - 24); /* sign extend */
            raw >>= (32 - 24); /* sign extend */

            if (posDiv8 == 1)
                raw *= 8;

            pos = (int)raw;

            return SetLastError(err);
        }
        public ErrorCode SetQuadraturePosition(int newPosition, int timeoutMs)
        {
            /* general firm check */
            CheckFirmVers(0, 42, ErrorCode.FeatureRequiresHigherFirm);

            return ConfigSetParameter(ParamEnum.eQuadraturePosition, newPosition, 0, 0, timeoutMs);
        }
        public ErrorCode GetQuadratureVelocity(out int vel)
        {
            /* general firm check */
            CheckFirmVers(0, 42, ErrorCode.FeatureRequiresHigherFirm);

            int err = CTRE.Native.CAN.Receive(STATUS_2 | _baseArbId, ref _cache, ref _len);

            byte H = (byte)(_cache >> 0x18);
            byte L = (byte)(_cache >> 0x20);
            int velDiv4 = (int)((_cache >> (0x28 + 6)) & 1);

            int raw = 0;
            raw |= H;
            raw <<= 8;
            raw |= L;

            raw <<= (32 - 16); /* sign extend */
            raw >>= (32 - 16); /* sign extend */

            if (velDiv4 == 1)
                raw *= 4;

            vel = (int)raw;

            return SetLastError(err);
        }

        public ErrorCode ConfigVelocityMeasurementPeriod(CANifierVelocityMeasPeriod period, int timeoutMs)
        {
            int param = (int)period;
            return ConfigSetParameter(ParamEnum.eSampleVelocityPeriod, param, 0, 0, timeoutMs);
        }
        public ErrorCode ConfigVelocityMeasurementWindow(int windowSize, int timeoutMs)
        {
            return ConfigSetParameter(ParamEnum.eSampleVelocityWindow, windowSize, 0, 0, timeoutMs);
        }

        public static String ToString(CANifier_LowLevel.GeneralPin gp)
        {
            String sig;
            switch (gp)
            {
                case CANifier_LowLevel.GeneralPin.LIMR: sig = "LIMR"; break;
                case CANifier_LowLevel.GeneralPin.LIMF: sig = "LIMF"; break;
                case CANifier_LowLevel.GeneralPin.QUAD_IDX: sig = "QUAD_IDX"; break;
                case CANifier_LowLevel.GeneralPin.QUAD_B: sig = "QUAD_B"; break;
                case CANifier_LowLevel.GeneralPin.QUAD_A: sig = "QUAD_A"; break;
                case CANifier_LowLevel.GeneralPin.SDA: sig = "SDA"; break;
                case CANifier_LowLevel.GeneralPin.SCL: sig = "SCL"; break;
                case CANifier_LowLevel.GeneralPin.SPI_CS: sig = "SPI_CS"; break;
                case CANifier_LowLevel.GeneralPin.SPI_MISO_PWM2P: sig = "SPI_MISO_PWM2P"; break;
                case CANifier_LowLevel.GeneralPin.SPI_MOSI_PWM1P: sig = "SPI_MOSI_PWM1P"; break;
                case CANifier_LowLevel.GeneralPin.SPI_CLK_PWM0P: sig = "SPI_CLK_PWM0P"; break;
                default: sig = "invalid"; break;
            }
            return sig;
        }

        //----------------------------------------------------------------------------------------------------//


        /*----- getters and setters that use param request/response. These signals are backed up in flash and will survive a power cycle. ---------*/
        /*----- If your application requires changing these values consider using both slots and switch between slot0 <=> slot1. ------------------*/
        /*----- If your application requires changing these signals frequently then it makes sense to leverage this API. --------------------------*/
        /*----- Getters don't block, so it may require several calls to get the latest value. --------------------------*/
        public ErrorCode SetStatusFramePeriod(CANifierStatusFrame frame, uint periodMs, uint timeoutMs = 0)
        {
            int fullId = (int)((int)_baseArbId | (int)frame); /* build ID */

            return base.SetStatusFramePeriod(fullId, (int)periodMs, (int)timeoutMs);
        }
        public ErrorCode GetStatusFramePeriod(CANifierStatusFrame frame, out int periodMs, int timeoutMs)
        {
            int fullId = (int)((int)_baseArbId | (int)frame); /* build ID */

            return base.GetStatusFramePeriod(fullId, out periodMs, timeoutMs);
        }
        public ErrorCode SetControlFramePeriod(CANifierControlFrame frame, int periodMs)
        {
            /* sterilize inputs */
            if (periodMs < 0) { periodMs = 0; }
            if (periodMs > 0xFF) { periodMs = 0xFF; }

            uint fullId = (uint)((int)_baseArbId | (int)frame); /* build ID */

            /* apply the change if frame is transmitting */
            int err = CTRE.Native.CAN.GetSendBuffer(fullId, ref _cache);
            if (err == 0)
            {
                err = CTRE.Native.CAN.Send(fullId, _cache, 8, (uint)periodMs);
            }

            return SetLastError(err);
        }
        //-------------------------------- Device_LowLevel requirements -----------------------//
        protected override void EnableFirmStatusFrame(bool enable)
        {
            SetClrBit(enable ? 0 : 1, (4 * 8) + 0, CANifier_Control_1_General_20Ms); /* DisableFirmStatusFrame */
        }
        public ErrorCode GetLastError()
        {
            return _lastError;
        }

        protected override ErrorCode SetLastError(ErrorCode error)
        {
            _lastError = error;
            return error;
        }

        private ErrorCode SetLastError(int error)
        {
            return SetLastError((ErrorCode)error);
        }

        //------ Faults ----------//
        public ErrorCode GetFaults(CANifierFaults toFill)
        {
            return SetLastError(ErrorCode.FeatureNotSupported);
        }
        public ErrorCode GetStickyFaults(CANifierStickyFaults toFill)
        {
            return SetLastError(ErrorCode.FeatureNotSupported);
        }
        public ErrorCode ClearStickyFaults(int timeoutMs)
        {
            return SetLastError(ErrorCode.FeatureNotSupported);
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

        //------ Utility ----------//
        private void SetClrBit(int bit, int shift, uint baseId)
        {
            /* get the frame */
            int retval = CTRE.Native.CAN.GetSendBuffer(baseId | _baseArbId, ref _cache);
            if (retval != 0) { return; }

            /* clear */
            _cache &= ~(0x1ul << shift);

            /* shift in */
            if (bit != 0)
            {
                _cache |= (UInt64)(1) << (shift);
            }

            /* flush changes */
            CTRE.Native.CAN.Send(baseId | _baseArbId, _cache, 8, 0xFFFFFFFF);
        }
    }
}
