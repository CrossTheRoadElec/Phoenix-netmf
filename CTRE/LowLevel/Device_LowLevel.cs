using System;
using System.Text;

namespace CTRE.Phoenix.LowLevel
{
    public abstract class Device_LowLevel
    {
        protected uint _baseArbId;
        uint _deviceNumber;
        string _description;

        uint _arbIdStartupFrame;
        uint _arbIdFrameApiStatus;

        UInt64 _cache;
        uint _len;

        UInt32 PARAM_REQUEST;
        UInt32 PARAM_RESPONSE;
        UInt32 PARAM_SET;

        UInt32 kParamArbIdValue;
        UInt32 kParamArbIdMask;

        ResetStats _resetStats = new ResetStats();
        int _firmVers = -1; /* invalid */
        int _failedVersionChecks = 0;

        internal ErrorCodeVar _lastError = ErrorCode.OK;

        System.Collections.Hashtable _sigs_Value = new System.Collections.Hashtable();
        System.Collections.Hashtable _sigs_SubValue = new System.Collections.Hashtable();

        const UInt32 kFullMessageIDMask = 0x1fffffff;
        const float FLOAT_TO_FXP_10_22 = (float)0x400000;
        const float FXP_TO_FLOAT_10_22 = 0.0000002384185791015625f;
        const float FLOAT_TO_FXP_0_8 = (float)0x100;
        const float FXP_TO_FLOAT_0_8 = 0.00390625f;


        //------------------------------------- Constructors -----------------------------------------//
        public Device_LowLevel(uint baseArbId, uint arbIdStartupFrame, uint paramReqId, uint paramRespId, uint paramSetId, uint arbIdFrameApiStatus)
        {
            _baseArbId = baseArbId;
            _deviceNumber = (baseArbId & 0x3F);
            _arbIdStartupFrame = arbIdStartupFrame;
            _arbIdFrameApiStatus = arbIdFrameApiStatus;
            PARAM_REQUEST = paramReqId;
            PARAM_RESPONSE = paramRespId;
            PARAM_SET = paramSetId;

            kParamArbIdValue = PARAM_RESPONSE;
            kParamArbIdMask = 0xFFFFFFFF;
        }

        public uint GetDeviceNumber()
        {
            return _deviceNumber;
        }
        //------------------------------------- Reset and firmware status -----------------------------------------//
        /**
         * Polls for firm frame.
         * @return error code.
         */
        private int GetFirmStatus()
        {
            int retval = CTRE.Native.CAN.Receive(_arbIdFrameApiStatus, ref _cache, ref _len);
            if (retval == 0)
            {
                byte H, L;

                /* decode firm */
                H = (byte)(_cache >> 0);
                L = (byte)(_cache >> 8);

                /* save it */
                _firmVers = H << 8 | L;
            }
            return retval;
        }
        /**
         * Polls status5 frame, which is only transmitted on motor controller boot.
         * @return error code.
         */
        protected int GetStatus5()
        {
            int retval = CTRE.Native.CAN.Receive(_arbIdStartupFrame, ref _cache, ref _len);
            if (retval == 0)
            {
                byte H, L;
                Int32 raw;
                /* frame has been received, therefore motor contorller has reset at least once */
                _resetStats.hasReset = true;
                /* reset count */
                H = (byte)(_cache >> 0);
                L = (byte)(_cache >> 8);
                raw = H << 8 | L;
                _resetStats.resetCount = (int)raw;
                /* reset flags */
                H = (byte)(_cache >> 16);
                L = (byte)(_cache >> 24);
                raw = H << 8 | L;
                _resetStats.resetFlags = (int)raw;
                /* firmVers */
                H = (byte)(_cache >> 32);
                L = (byte)(_cache >> 40);
                raw = H << 8 | L;
                _resetStats.firmVers = (int)raw;
            }
            return retval;
        }
        public int GetResetCount(out Int32 param)
        {
            /* repoll status frame */
            int retval = GetStatus5();
            param = _resetStats.resetCount;
            return retval;
        }
        public int GetResetFlags(out Int32 param)
        {
            /* repoll status frame */
            int retval = GetStatus5();
            param = _resetStats.resetFlags;
            return retval;
        }

        /** return -1 if not available, return 0xXXYY format if available */
        public int GetFirmwareVersion()
        {
            if (_firmVers != -1)
            {
                /* we have it already, return it */
                return _firmVers;
            }
            else if (GetFirmStatus() == 0)
            {
                /* we finally got a frame, shut it down and save it */
                EnableFirmStatusFrame(false);
                return _firmVers;
            }
            else
            {
                /* still waiting for the frame */
                return -1;
            }
        }
        /**
         * @return true iff a reset has occured since last call.
         */
        public bool HasResetOccured()
        {
            /* repoll status frame, ignore return since hasReset is explicitly tracked */
            GetStatus5();
            /* get-then-clear reset flag */
            bool retval = _resetStats.hasReset;
            _resetStats.hasReset = false;
            return retval;
        }

        /**
 * Helpful routine for child classes to report too-old firm
 */
        internal void CheckFirmVers(int minMajor, int minMinor, ErrorCode failCode)
        {
            /* get the firm vers */
            int vers = GetFirmwareVersion();
            /* track the failures */
            if (vers < 0)
            {
                if (_failedVersionChecks < 1000)
                {
                    ++_failedVersionChecks;
                }
            }
            else
            {
                _failedVersionChecks = 0;
            }
            /* compare To */
            int minAllowableFirmInclusive = (char)minMajor;
            minAllowableFirmInclusive <<= 8;
            minAllowableFirmInclusive |= (char)minMinor;
            /* if its not available skip test */
            if (vers >= 0)
            {
                /* tell user if its too old */
                if (vers < minAllowableFirmInclusive)
                {
                    /* get the trace */
                    String trace = ""; //This should be a stack trace - how do we get it in netmf without generating an exception?
                    /* build a message */
                    StringBuilder message = new StringBuilder();
                    message.Append(ToString());
                    message.Append(", firm must be >= ");
                    message.Append(minMajor);
                    message.Append(".");
                    message.Append(minMinor);
                    /* log it */
                    Reporting.Log(failCode, message.ToString(), 0,
                            trace);
                    
                }
            }
            if (_failedVersionChecks > 100)
            {
                /* get the trace */
                String trace = "";
                /* log it */
                Reporting.Log(ErrorCode.FirmVersionCouldNotBeRetrieved,
                        ToString(), 0, trace);
            }
        }

        /** child class has to provide a way to enable/disable firm status */
        protected abstract void EnableFirmStatusFrame(bool enable);

        protected abstract ErrorCode SetLastError(ErrorCode errorCode);

        //------------------- Session management ----------------------------//
        private ErrorCode OpenSession(ref uint handle)
        {
            handle = 0;
          
            /* bit30 - bit8 must match $000002XX.  Top bit is not masked to get remote frames */
            uint arbId = kParamArbIdValue | GetDeviceNumber();
            int status = CTRE.Native.CAN.OpenStream(ref handle, kParamArbIdMask, arbId);
           
            return (ErrorCode)status;
        }
		/**
		 * If stream has been opened, buffered messages are emptied and processed.
		 * If stream is no open, this routine has no effect.
         * @param handle Handle to stream to empty and process.
		 */
        private void ProcessStreamMessages(uint handle)
        {
            /* process receive messages */
            UInt32 i;
            UInt32 messagesRead = 0;
            UInt32 arbId = 0;
            UInt64 data = 0;
            UInt32 len = 0;
            UInt32 msgsRead = 0;
            /* read out latest bunch of messages */
            if (handle != 0)
            {
                CTRE.Native.CAN.GetStreamSize(handle, ref messagesRead);
            }
            /* loop thru each message of interest */
            for (i = 0; i < messagesRead; ++i)
            {
                CTRE.Native.CAN.ReadStream(handle, ref arbId, ref data, ref len, ref msgsRead);
                if (arbId == (PARAM_RESPONSE | GetDeviceNumber()))
                {
                    /*decode pieces */
                    byte paramEnum_h8 = (byte)(data);
                    byte paramEnum_l4 = (byte)((data >> 12) & 0xF);
                    byte ordinal = (byte)((data >> 8) & 0xF);
                    byte value_3 = (byte)(data >> 0x10);
                    byte value_2 = (byte)(data >> 0x18);
                    byte value_1 = (byte)(data >> 0x20);
                    byte value_0 = (byte)(data >> 0x28);
                    byte subValue = (byte)(data >> 0x38);
                    /* decode sigs */
                    UInt32 value = value_3;
                    value <<= 8;
                    value |= value_2;
                    value <<= 8;
                    value |= value_1;
                    value <<= 8;
                    value |= value_0;
                    UInt32 paramEnum = paramEnum_h8;
                    paramEnum <<= 4;
                    paramEnum |= paramEnum_l4;
                    /* save latest signal */
                    _sigs_Value[paramEnum] = value;
                    _sigs_SubValue[paramEnum] = subValue;
                }
            }
        }
        /**
         * Caller must ensure timeoutMs is nonzero.
         * @param timeoutMs How long to wait for response.
         * @param handle Handle to CAN Rx Stream.
         * @param paramEnum Parameter to look for.
         * @param valueReceived [out] Value received in response frame, or zero if not received.
         * @return nonzero for error code, zero for OK.
         */
        private ErrorCode WaitForParamResponse(int timeoutMs, uint handle, ParamEnum paramEnum, out int valueReceived)
        {
            ErrorCode status = 0;

            /* init the outputs to default values */
            valueReceived = 0;

            /* loop until timeout or receive if caller wants to check */
            while (timeoutMs > 0)
            {
                /* wait a bit */
                System.Threading.Thread.Sleep(1);
                /* process received param events. Caller should have opened 
                    the stream already. */
                ProcessStreamMessages(handle);
                /* see if response was received */
                if (0 == PollForParamResponse(paramEnum, out valueReceived))
                    break; /* leave inner loop */
                           /* decrement */
                --timeoutMs;
            }
            /* if we get here then we timed out */
            if (timeoutMs == 0) { status = ErrorCode.SIG_NOT_UPDATED; }

            return status;
        }
        //------------------------------------- ConfigSet* interface -----------------------------------------//
        /**
         * Send a one shot frame to set an arbitrary signal.
         * Most signals are in the control frame so avoid using this API unless you have
         * to.
         * Use this api for...
         * -A motor controller profile signal eProfileParam_XXXs.  These are backed up
         * in flash.  If you are gain-scheduling then call this periodically.
         * -Default brake and limit switch signals... eOnBoot_XXXs.  Avoid doing this,
         * use the override signals in the control frame.
         * Talon will automatically send a PARAM_RESPONSE after the set, so
         * GetParamResponse will catch the latest value after a couple ms.
         */
        private ErrorCode ConfigSetParameterRaw(ParamEnum paramEnum, int value, byte subValue, int ordinal = 0, int timeoutMs = 0)
        {
            ErrorCode statusTx = ErrorCode.OK; //!< Status code for transmit set request
            ErrorCode statusRx = ErrorCode.OK; //!< Status code for receive (if timeoutMs is nonzero)
            uint handle = 0; //!< Handle to CAN stream (if timeoutMs is nonzero)

            /* sterilize inputs */
            if ((int)paramEnum > 0xFFF) { return ErrorCode.CAN_INVALID_PARAM; }
            if (ordinal > 0xF) { return ErrorCode.CAN_INVALID_PARAM; }

                                                
            /* If we need to monitor for response, create rx stream */
            /* wait for response frame */
            if (timeoutMs > 0)
            {
                /* create a session to get filter rx frames. */
                statusRx = OpenSession(ref handle);
                /* remove stale entry if caller wants to wait for response. */
                _sigs_Value.Remove((uint)paramEnum);
                _sigs_SubValue.Remove((uint)paramEnum);
            }

            /* attempt to send request.  Save status code */
            statusTx = RequestOrSetParam(true, paramEnum, value, subValue, ordinal, 5); /* max tries is 5 */

            /* If caller wants to block,  wait for response frame and save status code */
            if (timeoutMs > 0)
            {
                /* caller wants to get response, check our stream health */
                if (statusRx != 0)
                {
                    /* stream could not be allocated, this error code will likely 
                     * be passed to caller below (assuming tx was succesful) */
                }
                else
                {
                    /* stream open did not report an error, continue to poll for response */
                    int valueReceived;
                    statusRx = WaitForParamResponse(timeoutMs, handle, paramEnum, out valueReceived);
                }
            }
            /* Close stream */
            if (handle != 0) { CTRE.Native.CAN.CloseStream(handle); }
            /* return the transmit code first, if this fails then nothing else matters  */
            if (statusTx != 0)
				return (ErrorCode)statusTx;
            return (ErrorCode)statusRx;
        }

        public ErrorCode ConfigSetParameter(ParamEnum paramEnum, float value, byte subValue, int ordinal = 0, int timeoutMs = 0)
        {
            Int32 rawbits = 0;
            switch (paramEnum)
            {
                case ParamEnum.eProfileParamSlot_P: /* unsigned 10.22 fixed pt value */
                case ParamEnum.eProfileParamSlot_I:
                case ParamEnum.eProfileParamSlot_D:
                    {
                        UInt32 urawbits;
                        if (value > 1023) /* bounds check doubles that are outside u10.22 */
                            value = 1023;
                        else if (value < 0)
                            value = 0;
                        urawbits = (UInt32)(value * FLOAT_TO_FXP_10_22); /* perform unsign arithmetic */
                        rawbits = (int)urawbits; /* copy bits over.  SetParamRaw just stuffs into CAN frame with no sense of signedness */
                    }
                    break;
                case ParamEnum.eProfileParamSlot_F: /* signed 10.22 fixed pt value */
                    if (value > 512) /* bounds check doubles that are outside s10.22 */
                        value = 512;
                    else if (value < -512)
                        value = -512;
                    rawbits = (Int32)(value * FLOAT_TO_FXP_10_22);
                    break;
                case ParamEnum.eNominalBatteryVoltage:
                    rawbits = (int)(value * 256f);
                    /* negative or zero disables feature */
                    if (rawbits < 0)
                        rawbits = 0;
                    /* max value is 255.0V */
                    if (rawbits > 0xFF00)
                        rawbits = 0xFF00;
                    break;
                case ParamEnum.eProfileParamSlot_PeakOutput:
                    if (value > 1.0f) /* bounds check doubles that are outside u10.22 */
                        value = 1.0f;
                    else if (value < 0)
                        value = 0;
                    rawbits = (int)(value * 1023.0);
                    break;
                case ParamEnum.eSelectedSensorCoefficient:
                    rawbits = (int)(value * 65536.0);
                    break;
                default: /* everything else is integral */
                    rawbits = (Int32)value;
                    break;
            }
            return ConfigSetParameterRaw(paramEnum, rawbits, subValue, ordinal, timeoutMs);
        }
        //------------------------------------- ConfigGet* interface -----------------------------------------//
        /**
         * Blocking read of a given parameter.  Virtual declared so that child classes can override with float return.
         * Child classes will understand how to decode raw integral type into human readable floating point values.
         * @param paramEnum Enumerated parameter to read.
         * @param paramEnum Enumerated parameter to read.
         * 
         */
        /** private get config with all params, status frame requires this */
        private ErrorCode ConfigGetParameter(ParamEnum paramEnum, int valueToSend, out int valueReceived, byte subValue, int ordinal, int timeoutMs)
        {
            ErrorCode statusTx = ErrorCode.OK; //!< Status code for transmit set request
            ErrorCode statusRx = ErrorCode.OK; //!< Status code for receive (if timeoutMs is nonzero)
            uint handle = 0; //!< Handle to CAN stream (if timeoutMs is nonzero)

            /* init the outputs */
            valueReceived = 0;

            /* sterilize inputs */
            if ((int)paramEnum > 0xFFF) { return ErrorCode.CAN_INVALID_PARAM; }
            if (ordinal > 0xF) { return ErrorCode.CAN_INVALID_PARAM; }

            if (timeoutMs < Constants.GetParamTimeoutMs) /* we must have a minimum wait */
                timeoutMs = Constants.GetParamTimeoutMs;

            /* create a session to get filter rx frames. */
            statusRx = OpenSession(ref handle);

            /* remove stale entry if caller wants to wait for response. */
            _sigs_Value.Remove((uint)paramEnum);
            _sigs_SubValue.Remove((uint)paramEnum);
      
            /* attempt to request.  Save status code */
            statusTx = RequestOrSetParam(false, paramEnum, valueToSend, subValue, ordinal, 5); /* max tries is 5 */


            /* caller wants to get response, check our stream health */
            if (statusRx != 0)
            {
                /* stream could not be allocated, this error code will likely 
                 * be passed to caller below (assuming tx was succesful) */
            }
            else
            {
                /* stream open did not report an error, continue to poll for response */
                statusRx = WaitForParamResponse(timeoutMs, handle, paramEnum, out valueReceived);
            }
           

            /* Close stream */
            if (handle != 0) { CTRE.Native.CAN.CloseStream(handle); }

            /* return the transmit code first, if this fails then nothing else matters  */
            if (statusTx != 0)
                return (ErrorCode)statusTx;
            return (ErrorCode)statusRx;
        }

        public ErrorCode ConfigGetParameter(ParamEnum paramEnum, out int value, int ordinal = 0, int timeoutMs = Constants.GetParamTimeoutMs)
        {
            ErrorCode retval = ConfigGetParameter(paramEnum, 0, out value, 0x00, ordinal, timeoutMs);
            return retval;
        }
        private float calcSecondsFromNeutralToFull(int percPer10Ms) {
            /* if percPer10Ms is zero(or negative) that means disable ramp */
            if (percPer10Ms <= 0.0f) {
                return 0.0f;
            }
            /* ramp is enabled*/
            return 1.0f / ( (float) percPer10Ms / 1023.0f * 100.0f);
        }

        public ErrorCode ConfigGetParameter(ParamEnum paramEnum, out float value, int ordinal = 0, int timeoutMs = Constants.GetParamTimeoutMs)
        {
            int valueInt;
            ErrorCode retval = ConfigGetParameter(paramEnum, 0, out valueInt, 0x00, ordinal, timeoutMs);
            /* scaling */
            switch (paramEnum)
            {
                case ParamEnum.eProfileParamSlot_P: /* 10.22 fixed pt value */
                case ParamEnum.eProfileParamSlot_I:
                case ParamEnum.eProfileParamSlot_D:
                case ParamEnum.eProfileParamSlot_F:
                    value = ((float)valueInt) * FXP_TO_FLOAT_10_22;
                    break;
                case ParamEnum.eNominalBatteryVoltage:
                    value = ((float)valueInt) * FXP_TO_FLOAT_0_8;
                    break;
                case ParamEnum.eProfileParamSlot_PeakOutput:
                    value = ((float)valueInt) * 1.0f / 1023.0f;
                    break;
                case ParamEnum.eOpenloopRamp:
                    value = calcSecondsFromNeutralToFull(valueInt);
                    break;
                case ParamEnum.eClosedloopRamp:
                    value = calcSecondsFromNeutralToFull(valueInt);
                    break;
                case ParamEnum.ePeakPosOutput:
                    value = ((float) valueInt) * 1.0f / 1023.0f;
                    break;
                case ParamEnum.eNominalPosOutput:
                    value = ((float) valueInt) * 1.0f / 1023.0f;
                    break;
                case ParamEnum.ePeakNegOutput:
                    value = ((float) valueInt) * 1.0f / 1023.0f;
                    break;
                case ParamEnum.eNominalNegOutput:
                    value = ((float) valueInt) * 1.0f / 1023.0f;
                    break;
                case ParamEnum.eNeutralDeadband:
                    value = ((float) valueInt) * 1.0f / 1023.0f;
                    break;
                case ParamEnum.eSelectedSensorCoefficient:
                    value = ((float)valueInt) * 1.0f / 65536.0f;
                    break;
                default: /* everything else is integral */
                    value = (float)valueInt;
                    break;
            }
            return retval;
        }

        //------------------------------------- framing functions -----------------------------------------//
        /**
         * Asks TALON to immedietely respond with signal value.  This API is only used
         * for signals that are not sent periodically.
         * This can be useful for reading params that rarely change like Limit Switch
         * settings and PIDF values.
          * @param paramEnum parameter to request.
          * @param value 
          * @param subValue
          * @param ordinal
         */
        private ErrorCode RequestOrSetParam(bool bIsSet, ParamEnum paramEnum, int value, byte subValue, int ordinal, int retryMax)
        {
            if (ordinal < 0x0) { return ErrorCode.CAN_INVALID_PARAM; }
            if (ordinal > 0xF) { return ErrorCode.CAN_INVALID_PARAM; }

            byte paramEnum_h8 = (byte)((int)paramEnum >> 4);
            byte paramEnum_l4 = (byte)((int)paramEnum & 0xF);

            ulong frame = 0;
            frame = subValue;
            frame <<= 8;
            frame |= 0x00;
            frame <<= 8;
            frame |= (byte)(value >> 0x00);
            frame <<= 8;
            frame |= (byte)(value >> 0x08);
            frame <<= 8;
            frame |= (byte)(value >> 0x10);
            frame <<= 8;
            frame |= (byte)(value >> 0x18);
            frame <<= 8;
            frame |= (byte)(ordinal | (paramEnum_l4 << 4));
            frame <<= 8;
            frame |= (byte)(paramEnum_h8);

            uint baseId = bIsSet ? PARAM_SET : PARAM_REQUEST;

            ErrorCode status = ErrorCode.OK;
            do
            {
                status = (ErrorCode)CTRE.Native.CAN.Send(baseId | GetDeviceNumber(), frame, 8, 0);
                /* If transmit successful, exit while loop */
                if (status == 0)
                    break;

                --retryMax;
            } while (retryMax >= 0);

            return status;
        }
        private static Int32 Sterilize(UInt32 value)
        {
            Int32 retval;
            /* test top bit */
            if ((value & 0x80000000) == 0)
            {
                /* positive or zero - do nothing */
                retval = (Int32)value;
            }
            else
            {
                /* negative */
                long temp = (UInt32)value; /* copy bottom 32 bits */

                /* sign extend */
                temp <<= 32;
                temp >>= 32;

                /* back to caller */
                retval = (Int32)temp;
            }
            /* send fixed value back to caller */
            return retval;
        }
        /**
         * Checks cached CAN frames and updating solicited signals.
         */
        int PollForParamResponse(ParamEnum paramEnum, out Int32 rawBits)
        {
            int retval = 0;
            /* grab the solicited signal value */
            if (_sigs_Value.Contains((uint)paramEnum) == false)
            {
                retval = (int)ErrorCode.SIG_NOT_UPDATED;
                rawBits = 0; /* default value if signal was not received */
            }
            else
            {
                
                Object value = _sigs_Value[(uint)paramEnum];
                uint temp = (uint)value;
                rawBits = Sterilize(temp);
            }
            return retval;
        }

        //------------------------------------- Status Frame Rate -----------------------------------------//
        /**
        * Change the periodMs of a TALON's status frame.  See kStatusFrame_* enums for
        * what's available.
        */
        protected ErrorCode SetStatusFramePeriod(int statusArbID, int periodMs, int timeoutMs = 0)
        {
            ErrorCode retval = ErrorCode.OK;
            /* bounds check the period */
            if (periodMs < 1)
                periodMs = 1;
            else if (periodMs > 255)
                periodMs = 255;
            byte period = (byte)periodMs;

            /* if lookup was succesful, send set-request out */
            if (retval == (int)ErrorCode.OK)
            {
                /* paramEnum is updated, sent it out */
                retval = ConfigSetParameter(ParamEnum.eStatusFramePeriod, (int)statusArbID, period, 0, timeoutMs);
            }
            return retval;
        }
        protected ErrorCode GetStatusFramePeriod(int statusArbID, out int periodMs, int timeoutMs = Constants.GetParamTimeoutMs)
        {
            return ConfigGetParameter(ParamEnum.eStatusFramePeriod, statusArbID, out periodMs, 0x00, 0, timeoutMs);
        }

        //------ Custom Persistent Params ----------//
        protected ErrorCode ConfigSetCustomParam(int value,
                int paramIndex, int timeoutMs)
        {
            return ConfigSetParameter(ParamEnum.eCustomParam, value, 0, paramIndex, timeoutMs);
        }
        protected ErrorCode ConfigGetCustomParam(out int value,
                int paramIndex, int timeoutMs)
        {
            return ConfigGetParameter(ParamEnum.eCustomParam, out value, paramIndex, timeoutMs);
        }

        //------ Description API ----------//
        /** child class should call this once to set the description */
        protected void SetDescription(string description) {
	        _description = description;
        }
        public override string ToString() {
	        return _description;
        }
        
    }
}
