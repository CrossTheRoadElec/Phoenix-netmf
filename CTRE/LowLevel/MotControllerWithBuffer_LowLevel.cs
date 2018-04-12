/**  */
/**  */
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using CTRE.Phoenix.MotorControl;
using CTRE.Phoenix.LowLevel;
using CTRE.Phoenix.Motion;

namespace CTRE.Phoenix.LowLevel
{
    using TALON_Control_6_MotProfAddTrajPoint_huff0_t = UInt64;
    using TALON_Control_6_MotProfAddTrajPoint_t = UInt64;

    /** A mot controller with buffer low level. */
    public class MotControllerWithBuffer_LowLevel : MotController_LowLevel
    {
        //const UInt32 STATUS_01 = 0x041400;
        //const UInt32 STATUS_02 = 0x041440;
        //const UInt32 STATUS_03 = 0x041480;
        //const UInt32 STATUS_04 = 0x0414C0;
        //const UInt32 STATUS_05 = 0x041500;
        //const UInt32 STATUS_06 = 0x041540;
        //const UInt32 STATUS_07 = 0x041580;
        //const UInt32 STATUS_08 = 0x0415C0;
        //const UInt32 STATUS_09 = 0x041600;
        //const UInt32 STATUS_10 = 0x041640;
        //const UInt32 STATUS_11 = 0x041680;
        //const UInt32 STATUS_12 = 0x0416C0;
        //const UInt32 STATUS_13 = 0x041700;
        //const UInt32 STATUS_14 = 0x041740;
        //const UInt32 STATUS_15 = 0x041780;

        //const UInt32 CONTROL_1 = 0x040000;
        //const UInt32 CONTROL_2 = 0x040040;
        //const UInt32 CONTROL_3 = 0x040080;
        //const UInt32 CONTROL_5 = 0x040100;
        //const UInt32 CONTROL_6 = 0x040140;

        //const UInt32 PARAM_REQ = 0x041800;
        //const UInt32 PARAM_RESP = 0x041840;
        //const UInt32 PARAM_SET = 0x041880;

        //const UInt32 kParamArbIdValue = PARAM_RESPONSE;
        //const UInt32 kParamArbIdMask = 0xFFFFFFFF;

        /** The float to fxp 10 22. */
        const float FLOAT_TO_FXP_10_22 = (float)0x400000;
        /** The fxp to float 10 22. */
        const float FXP_TO_FLOAT_10_22 = 0.0000002384185791015625f;

        /** The float to fxp 0 8. */
        const float FLOAT_TO_FXP_0_8 = (float)0x100;
        /** The fxp to float 0 8. */
        const float FXP_TO_FLOAT_0_8 = 0.00390625f;

        /* status frame rate types */
        //const int kStatusFrame_General = 0;
        //const int kStatusFrame_Feedback = 1;
        //const int kStatusFrame_Encoder = 2;
        //const int kStatusFrame_AnalogTempVbat = 3;
        //const int kStatusFrame_PulseWidthMeas = 4;
        //const int kStatusFrame_MotionProfile = 5;
        //const int kStatusFrame_MotionMagic = 6;
        /* Motion Profile status bits */
        /** The motion profile flag act traj is valid. */
        const int kMotionProfileFlag_ActTraj_IsValid = 0x1;
        /** The motion profile flag has underrun. */
        const int kMotionProfileFlag_HasUnderrun = 0x2;
        /** The motion profile flag is underrun. */
        const int kMotionProfileFlag_IsUnderrun = 0x4;
        /** The motion profile flag act traj is last. */
        const int kMotionProfileFlag_ActTraj_IsLast = 0x8;
        /** The motion profile flag act traj velocity only. */
        const int kMotionProfileFlag_ActTraj_VelOnly = 0x10;
        /* Motion Profile Set Output */
        /** Motor output is neutral, Motion Profile Executer is not running. */
        const int kMotionProf_Disabled = 0;
        /** Motor output is updated from Motion Profile Executer, MPE will process the buffered points. */
        const int kMotionProf_Enable = 1;

        /**
         * Motor output is updated from Motion Profile Executer, MPE will stay processing current
         * trajectory point.
         */

        const int kMotionProf_Hold = 2;

        /** The default control 6 period milliseconds. */
        const int kDefaultControl6PeriodMs = 10;

        /** The cache. */
        private UInt64 _cache;
        /** The length. */
        private UInt32 _len;

        //ErrorCode _lastError = ErrorCode.OK;



       
        //--------------------- Buffering Motion Profile ---------------------------//

        /**
         * To keep buffers from getting out of control, place a cap on the top level buffer.  Calling
         * application can stream addition points as they are fed to Talon. Approx memory footprint is
         * this capacity X 8 bytes.
         */

        const int kMotionProfileTopBufferCapacity = 512;
        /** Buffer for mot prof top data. */
        TrajectoryBuffer _motProfTopBuffer = new TrajectoryBuffer(kMotionProfileTopBufferCapacity);
        /** Flow control for streaming trajectories. */
        Int32 _motProfFlowControl = -1;

        /** The mut mot prof. */
        Object _mutMotProf = new Object();

        /** Frame Period of the motion profile control6 frame. */
        uint _control6PeriodMs = kDefaultControl6PeriodMs;


        //--------------------- Constructors -----------------------------//

        /**
         * Constructor for the CANTalon device.
         *
         * @param   baseArbId       The CAN ID of the Talon SRX.
         * @param   externalEnable  (Optional) pass true to prevent sending enable frames. This can be
         *                          useful when having one device enable the Talon, and another to
         *                          control it.
         */

        public MotControllerWithBuffer_LowLevel(int baseArbId, bool externalEnable = false) : base(baseArbId,externalEnable)
        {

        }

        //public MotControllerCCI(UInt32 deviceId, bool externalEnable = false) 
        //    : base(deviceId, deviceId | STATUS_5, PARAM_REQUEST, PARAM_RESPONSE, PARAM_SET)
        //{
        //    if (false == externalEnable)
        //        CTRE.Native.CAN.Send(CONTROL_1 | _baseArbId, 0x00, 2, 50);
        //    CTRE.Native.CAN.Send(CONTROL_3 | _baseArbId, 0x00, 8, 10);
        //    SetOverrideLimitSwitchEn(1);
        //}

        /**
         * Gets control 6.
         *
         * @return  the tx task that transmits Control6 (motion profile control). If it's not scheduled,
         *          then schedule it.  This is part of firing the MotionProf framing only when needed to
         *          save bandwidth.
         */

        private UInt64 GetControl6()
        {
            int retval = CTRE.Native.CAN.GetSendBuffer(CONTROL_6 | _baseArbId, ref _cache);
            if (retval != 0)
            {
                /* control6 never started, arm it now */
                _cache = 0;
                CTRE.Native.CAN.Send(CONTROL_6 | _baseArbId, _cache, 8, _control6PeriodMs);
                /* sync flow control */
                _motProfFlowControl = 0;
            }
            return _cache;
        }

        /**
         * Calling application can opt to speed up the handshaking between the robot API and the Talon
         * to increase the download rate of the Talon's Motion Profile. Ideally the period should be no
         * more than half the period of a trajectory point.
         *
         * @param   periodMs    The period in milliseconds.
         */

        public ErrorCode ChangeMotionControlFramePeriod(UInt32 periodMs)
        {
            lock (_mutMotProf)
            {
                /* if message is already registered, it will get updated.
                 * Otherwise it will error if it hasn't been setup yet, but that's ok
                 * because the _control6PeriodMs will be used later.
                 * @see GetControl6
                 */
                _control6PeriodMs = periodMs;
                /* apply the change if frame is transmitting */
                int stat = CTRE.Native.CAN.GetSendBuffer(CONTROL_6 | _baseArbId, ref _cache);
                if (stat == 0)
                {
                    /* control6 already started, change frame rate */
                    CTRE.Native.CAN.Send(CONTROL_6 | _baseArbId, _cache, 8, _control6PeriodMs);
                }
                return SetLastError(stat);
            }
        }
        public ErrorCode ConfigMotionProfileTrajectoryPeriod(int durationMs, int timeoutMs)
        {
            return ConfigSetParameter(ParamEnum.eMotionProfileTrajectoryPointDurationMs, durationMs, 0, 0,
                    timeoutMs);
        }

        
        /** Clear the buffered motion profile in both Talon RAM (bottom), and in the API (top). */
        public void ClearMotionProfileTrajectories()
        {
            lock (_mutMotProf)
            {
                /* clear the top buffer */
                _motProfTopBuffer.Clear();
                /* send signal to clear bottom buffer */
                UInt64 frame = GetControl6();
                frame &= 0xFFFFFFFFFFFFF0FF; /* clear Idx */
                _motProfFlowControl = 0; /* match the transmitted flow control */
                CTRE.Native.CAN.Send(CONTROL_6 | _baseArbId, frame, 8, 0xFFFFFFFF);
            }
        }

        /**
         * Retrieve just the buffer count for the api-level (top) buffer. This routine performs no CAN
         * or data structure lookups, so its fast and ideal if caller needs to quickly poll the progress
         * of trajectory points being emptied into Talon's RAM. Otherwise just use
         * GetMotionProfileStatus.
         *
         * @return  number of trajectory points in the top buffer.
         */

        public int GetMotionProfileTopLevelBufferCount()
        {
            lock (_mutMotProf)
            {
                int retval = (int)_motProfTopBuffer.GetNumTrajectories();
                return retval;
            }
        }

        /**
         * Retrieve just the buffer full for the api-level (top) buffer. This routine performs no CAN or
         * data structure lookups, so its fast and ideal if caller needs to quickly poll. Otherwise just
         * use GetMotionProfileStatus.
         *
         * @return  number of trajectory points in the top buffer.
         */

        public bool IsMotionProfileTopLevelBufferFull()
        {
            lock (_mutMotProf)
            {
                if (_motProfTopBuffer.GetNumTrajectories() >= kMotionProfileTopBufferCapacity)
                    return true;
                return false;
            }
        }

        int DurationMsToEnum(int durationMs)
        {
            int[] kDurationMs = MotionProf_DurationMs.List;
            int kNum = kDurationMs.Length;
            int kInclusiveEnd = kNum - 1;
            /* find value in enum list. */
            for (int i = 0; i < kNum; ++i)
            {
                if (durationMs == kDurationMs[i])
                    return i;
            }
            /* if its not support return the slowest one. User will notice the problem this way. */
            return kInclusiveEnd;
        }

        /**
         * Push another trajectory point into the top level buffer (which is emptied into the Talon's
         * bottom buffer as room allows).
         *
         * @param   targPos             servo position in native Talon units (sensor units).
         * @param   targVel             velocity to feed-forward in native Talon units (sensor units per
         *                              100ms).
         * @param   profileSlotSelect   which slot to pull PIDF gains from.  Currently supports 0 or 1.
         * @param   timeDurMs           time in milliseconds of how long to apply this point.
         * @param   velOnly             set to nonzero to signal Talon that only the feed-foward velocity
         *                              should be used, i.e. do not perform PID on position. This is
         *                              equivalent to setting PID gains to zero, but much more efficient
         *                              and synchronized to MP.
         * @param   isLastPoint         set to nonzero to signal Talon to keep processing this trajectory
         *                              point, instead of jumping to the next one when timeDurMs expires.
         *                              Otherwise MP executer will eventually see an empty buffer after
         *                              the last point expires, causing it to assert the IsUnderRun flag.
         *                              However this may be desired if calling application never wants to
         *                              terminate the MP.
         * @param   zeroPos             set to nonzero to signal Talon to "zero" the selected position
         *                              sensor before executing this trajectory point. Typically the
         *                              first point should have this set only thus allowing the remainder
         *                              of the MP positions to be relative to zero.
         *
         * @return  CTR_OKAY if trajectory point push ok. CTR_BufferFull if buffer is full due to
         *          kMotionProfileTopBufferCapacity.
         */

        public ErrorCode PushMotionProfileTrajectory(
        double position, double velocity, double turn,
        int profileSlotSelect0, int profileSlotSelect1, bool isLastPoint, bool zeroPos, int durationMs)
        {

            ErrorCode retval = ErrorCode.OK;

            int turnUnits = (int)(turn);
            int targPos = (int)position;
            int targVel = (int)velocity;

            /* sterilize and check inputs */
            if (profileSlotSelect0 < 0)
            {
                profileSlotSelect0 = 0;
                retval = ErrorCode.InvalidParamValue;
            }
            if (profileSlotSelect0 > 3)
            {
                profileSlotSelect0 = 3;
                retval = ErrorCode.InvalidParamValue;
            }
            if (profileSlotSelect1 < 0)
            {
                profileSlotSelect1 = 0;
                retval = ErrorCode.InvalidParamValue;
            }
            if (profileSlotSelect1 > 1)
            {
                profileSlotSelect1 = 1;
                retval = ErrorCode.InvalidParamValue;
            }

            byte b0 = (byte)profileSlotSelect0;
            b0 <<= 2;
            b0 |= (byte)(zeroPos ? 1 : 0);
            b0 <<= 1;
            b0 |= (byte)DurationMsToEnum(durationMs);

            byte headingH = (byte)((turnUnits >> 8) & 0x3F);
            byte headingL = (byte)(turnUnits & 0xFF);

            byte b1 = (byte)(isLastPoint ? 1 : 0);
            b1 <<= 1;
            b1 |= (byte)profileSlotSelect1;
            b1 <<= 1;
            b1 |= headingH;

            byte b2 = headingL;
            byte b3 = (byte)(targVel >> 0x08);
            byte b4 = (byte)(targVel & 0xFF);
            byte b5 = (byte)(targPos >> 0x10);
            byte b6 = (byte)(targPos >> 0x08);
            byte b7 = (byte)(targPos & 0xFF);

            TALON_Control_6_MotProfAddTrajPoint_huff0_t traj = 0;
            traj |= b7;
            traj <<= 8;
            traj |= b6;
            traj <<= 8;
            traj |= b5;
            traj <<= 8;
            traj |= b4;
            traj <<= 8;
            traj |= b3;
            traj <<= 8;
            traj |= b2;
            traj <<= 8;
            traj |= b1;
            traj <<= 8;
            traj |= b0;

            lock (_mutMotProf)
            {
                if (_motProfTopBuffer.GetNumTrajectories() >= kMotionProfileTopBufferCapacity)
                    return ErrorCode.CAN_OVERFLOW;
                _motProfTopBuffer.Push(traj);
            }
            return ErrorCode.OK;
        }

        /**
         * Increment our flow control to manage streaming to the Talon.
         *      f(x) = { 1,   x = 15,
         *               x+1,  x &lt; 15
         *             }
         *
         * @param   idx Zero-based index of the.
         *
         * @return  An int.
         */

        private int MotionProf_IncrementSync(int idx)
        {
            return ((idx >= 3) ? 1 : 0) + ((idx + 1) & 0xF);
        }

        

        /**
         * Caller is either pushing a new motion profile point, or is calling the Process buffer
         * routine.  In either case check our flow control to see if we need to start sending control6.
         */

        private void ReactToMotionProfileCall()
        {
            if (_motProfFlowControl < 0)
            {
                /* we have not yet armed the periodic frame.  We do this lazilly to
                 * save bus utilization since most Talons on the bus probably are not
                 * MP'ing.
                 */
                ClearMotionProfileTrajectories(); /* this moves flow control so only fires
                                         once if ever */
            }
        }

        /**
         * This must be called periodically to funnel the trajectory points from the API's top level
         * buffer to the Talon's bottom level buffer.  Recommendation is to call this twice as fast as
         * the executation rate of the motion profile. So if MP is running with 20ms trajectory points,
         * try calling this routine every 10ms.  All motion profile functions are thread-safe through
         * the use of a mutex, so there is no harm in having the caller utilize threading.
         */

        public void ProcessMotionProfileBuffer()
        {
            ReactToMotionProfileCall();
            /* get the latest status frame */
            int retval = CTRE.Native.CAN.Receive(STATUS_09 | _baseArbId, ref _cache, ref _len);
            /* lock */
            lock (_mutMotProf)
            {
                int NextID = (int)((_cache >> (0x8 + 5)) & 0x3);
                /* calc what we expect to receive */
                if (_motProfFlowControl == NextID)
                {
                    /* Talon has completed the last req */
                    if (_motProfTopBuffer.IsEmpty())
                    {
                        /* nothing to do */
                    }
                    else
                    {
                        /* get the latest control frame */
                        UInt64 toFill = GetControl6();
                        UInt64 front = _motProfTopBuffer.Front();
                        toFill |= front;
                        _motProfTopBuffer.Pop();
                        _motProfFlowControl = MotionProf_IncrementSync(_motProfFlowControl);
                        /* insert latest flow control */
                        ulong val = (ulong)_motProfFlowControl;
                        val &= 0x3;
                        val <<= 6;
                        toFill &= 0xFFFFFFFFFFFFFF3F;
                        toFill |= val;
                        CTRE.Native.CAN.Send(CONTROL_6 | _baseArbId, toFill, 8, 0xFFFFFFFF);
                    }
                }
                else
                {
                    /* still waiting on Talon */
                }
            }
        }

        /**
         * Retrieve all status information. Since this all comes from one CAN frame, its ideal to have
         * one routine to retrieve the frame once and decode everything.
         *
         * @param [out] flags               bitfield for status bools. Starting with least significant
         *                                  bit: IsValid, HasUnderrun, IsUnderrun, IsLast, VelOnly.
         *                                  
         *                                  IsValid  set when MP executer is processing a trajectory
         *                                  point, and that point's status is instrumented with IsLast,
         *                                  VelOnly, targPos, targVel.  However if MP executor is not
         *                                  processing a trajectory point, then this flag is false, and
         *                                  the instrumented signals will be zero. HasUnderrun  is set
         *                                  anytime the MP executer is ready to pop another trajectory
         *                                  point from the Talon's RAM, but the buffer is empty.  It can
         *                                  only be cleared by using
         *                                  SetParam(eMotionProfileHasUnderrunErr,0);
         *                                  IsUnderrun  is set when the MP executer is ready for another
         *                                  point, but the buffer is empty, and cleared when the MP
         *                                  executer does not need another point. HasUnderrun shadows
         *                                  this registor when this register gets set, however
         *                                  HasUnderrun stays asserted until application has process it,
         *                                  and IsUnderrun auto-clears when the condition is resolved.
         *                                  IsLast  is set/cleared based on the MP executer's current
         *                                  trajectory point's IsLast value.  This assumes IsLast was set
         *                                  when PushMotionProfileTrajectory was used to insert the
         *                                  currently processed trajectory point. VelOnly  is set/cleared
         *                                  based on the MP executer's current trajectory point's VelOnly
         *                                  value.
         * @param [out] profileSlotSelect   The currently processed trajectory point's selected slot.
         *                                  This can differ in the currently selected slot used for
         *                                  Position and Velocity servo modes.
         * @param [out] targPos             The currently processed trajectory point's position in native
         *                                  units.  This param is zero if IsValid is zero.
         * @param [out] targVel             The currently processed trajectory point's velocity in native
         *                                  units.  This param is zero if IsValid is zero.
         * @param [out] topBufferRem        The remaining number of points in the top level buffer.
         * @param [out] topBufferCnt        The number of points in the top level buffer to be sent to
         *                                  Talon.
         * @param [out] btmBufferCnt        The number of points in the bottom level buffer inside Talon.
         * @param [out] outputEnable        The output enable.
         *
         * @return  CTR error code.
         */

        public ErrorCode GetMotionProfileStatus(out int topBufferRem, out int topBufferCnt, out int btmBufferCnt,
                                        out bool hasUnderrun, out bool isUnderrun, out bool activePointValid,
                                        out bool isLast, out int profileSlotSelect0, out int outputEnable,
                                        out int timeDurMs, out int profileSlotSelect1)
        {
            /* get the latest status frame */
            int retval = CTRE.Native.CAN.Receive(STATUS_09 | _baseArbId, ref _cache, ref _len);

            /* clear signals in case we never received an update, caller should check
	         * return
	         */
            profileSlotSelect0 = 0;
            btmBufferCnt = 0;

            /* these signals are always available */
            topBufferCnt = (int)_motProfTopBuffer.GetNumTrajectories();
            topBufferRem = kMotionProfileTopBufferCapacity - (int)_motProfTopBuffer.GetNumTrajectories();

            activePointValid = (_cache & 1) == 1 ? true : false;
            hasUnderrun = ((_cache >> 6) & 1) == 1 ? true : false;
            isUnderrun = ((_cache >> 7) & 1) == 1 ? true : false;
            isLast = ((_cache >> 3) & 1) == 1 ? true : false;
            btmBufferCnt = (byte)(_cache >> 0x10);
            profileSlotSelect0 = (int)((_cache >> 1) & 0x3);
            profileSlotSelect1 = (int)((_cache >> (0x8 + 3)) & 0x3);
            timeDurMs = (byte)(_cache >> 0x18);
            //bufferIsFull  = rx->BufferIsFull; // since we have the btmCount, we don't need this.

            switch ((SetValueMotionProfile)((_cache >> 4) & 0x3))
            {
                case SetValueMotionProfile.Disable:
                case SetValueMotionProfile.Enable:
                case SetValueMotionProfile.Hold:
                    outputEnable = (int)((_cache >> 4) & 0x3);
                    break;
                default:
                    /* do now allow invalid values for sake of user-facing enum types */
                    outputEnable = (int)kMotionProf_Disabled;
                    break;
            }

            return SetLastError(retval);
        }

        /**
         * Clear the hasUnderrun flag in Talon's Motion Profile Executer when MPE is ready for another
         * point, but the low level buffer is empty.
         * 
         * Once the Motion Profile Executer sets the hasUnderrun flag, it stays set until Robot
         * Application clears it with this routine, which ensures Robot Application gets a chance to
         * instrument or react.  Caller could also check the isUnderrun flag which automatically clears
         * when fault condition is removed.
         *
         * @param   timeoutMs   (Optional) The timeout in milliseconds.
         */

        public void ClearMotionProfileHasUnderrun(int timeoutMs = 0)
        {
            ConfigSetParameter(ParamEnum.eMotionProfileHasUnderrunErr, 0, 0, 0, timeoutMs);
        }

        /**
         * Gets motion profile status.
         *
         * @param   statusToFill    The status to fill.
         */

        public void GetMotionProfileStatus(Motion.MotionProfileStatus statusToFill)
        {
            int temp;

            GetMotionProfileStatus(out statusToFill.topBufferRem,
                                    out statusToFill.topBufferCnt,
                                    out statusToFill.btmBufferCnt,
                                    out statusToFill.hasUnderrun,
                                    out statusToFill.isUnderrun,
                                    out statusToFill.activePointValid,
                                    out statusToFill.isLast,
                                    out statusToFill.profileSlotSelect,
                                    out temp,
                                    out statusToFill.timeDurMs,
                                    out statusToFill.profileSlotSelect1);

            statusToFill.outputEnable = (SetValueMotionProfile)temp;
        }

        /**
         * Pushes a motion profile trajectory.
         *
         * @param   trajPt  The traj point.
         *
         * @return  An ErrorCode.
         */

        public ErrorCode PushMotionProfileTrajectory(Motion.TrajectoryPoint trajPt)
        {
            return PushMotionProfileTrajectory((double)trajPt.position,
                                                    (double)trajPt.velocity,
                                                    (double)trajPt.headingDeg,
                                                    (int)trajPt.profileSlotSelect0,
                                                    (int)trajPt.profileSlotSelect1,
                                                    trajPt.isLastPoint,
                                                    trajPt.zeroPos,
                                                    (int)trajPt.timeDur);
        }
    }
}