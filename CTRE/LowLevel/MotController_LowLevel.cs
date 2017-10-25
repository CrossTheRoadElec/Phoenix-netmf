/**
 * @brief CAN TALON SRX driver.
 *
 * The TALON SRX is designed to instrument all runtime signals periodically.
 * The default periods are chosen to support 16 TALONs with 10ms update rate
 * for control (throttle or setpoint).  However these can be overridden with
 * SetStatusFramePeriod. @see SetStatusFrameRate
 * The getters for these unsolicited signals are auto generated at the bottom
 * of this module.
 *
 * Likewise most control signals are sent periodically using the fire-and-forget
 * CAN API.  The setters for these unsolicited signals are auto generated at the
 * bottom of this module.
 *
 * Signals that are not available in an unsolicited fashion are the Close Loop
 * gains.  For teams that have a single profile for their TALON close loop they
 * can use either the webpage to configure their TALONs once or set the PIDF,
 * Izone, CloseLoopRampRate, etc... once in the robot application.  These
 * parameters are saved to flash so once they are loaded in the TALON, they
 * will persist through power cycles and mode changes.
 *
 * For teams that have one or two profiles to switch between, they can use the
 * same strategy since there are two slots to choose from and the
 * ProfileSlotSelect is periodically sent in the 10 ms control frame.
 *
 * For teams that require changing gains frequently, they can use the soliciting
 * API to get and set those parameters.  Most likely they will only need to set
 * them in a periodic fashion as a function of what motion the application is
 * attempting.  If this API is used, be mindful of the CAN utilization reported
 * in the driver station.
 *
 * If calling application has used the config routines to configure the
 * selected feedback sensor, then all positions are measured in floating point
 * precision rotations.  All sensor velocities are specified in floating point
 * precision RPM.
 * @see ConfigPotentiometerTurns
 * @see ConfigEncoderCodesPerRev
 * HOWEVER, if calling application has not called the config routine for
 * selected feedback sensor, then all getters/setters for position/velocity use
 * the native engineering units of the Talon SRX firm (just like in 2015).
 * Signals explained below.
 *
 * Encoder position is measured in encoder edges.  Every edge is counted
 * (similar to roboRIO 4X mode).  Analog position is 10 bits, meaning 1024
 * ticks per rotation (0V => 3.3V).  Use SetFeedbackDeviceSelect to select
 * which sensor type you need.  Once you do that you can use GetSensorPosition()
 * and GetSensorVelocity().  These signals are updated on CANBus every 20ms (by
 * default).  If a relative sensor is selected, you can zero (or change the
 * current value) using SetSensorPosition.
 *
 * Analog Input and quadrature position (and velocity) are also explicitly
 * reported in GetEncPosition, GetEncVel, GetAnalogInWithOv, GetAnalogInVel.
 * These signals are available all the time, regardless of what sensor is
 * selected at a rate of 100ms.  This allows easy instrumentation for "in the
 * pits" checking of all sensors regardless of modeselect.  The 100ms rate is
 * overridable for teams who want to acquire sensor data for processing, not
 * just instrumentation.  Or just select the sensor using
 * SetFeedbackDeviceSelect to get it at 20ms.
 *
 * Velocity is in position ticks / 100ms.
 *
 * All output units are in respect to duty cycle (throttle) which is -1023(full
 * reverse) to +1023 (full forward).  This includes demand (which specifies
 * duty cycle when in duty cycle mode) and rampRamp, which is in throttle units
 * per 10ms (if nonzero).
 *
 * Pos and velocity close loops are calc'd as
 *   err = target - posOrVel.
 *   iErr += err;
 *   if(   (IZone!=0)  and  abs(err) > IZone)
 *       ClearIaccum()
 *   output = P X err + I X iErr + D X dErr + F X target
 *   dErr = err - lastErr
 * P, I, and D gains are always positive. F can be negative.
 * Motor direction can be reversed using SetRevMotDuringCloseLoopEn if
 * sensor and motor are out of phase. Similarly feedback sensor can also be
 * reversed (multiplied by -1) if you prefer the sensor to be inverted.
 *
 * P gain is specified in throttle per error tick.  For example, a value of 102
 * is ~9.9% (which is 102/1023) throttle per 1 ADC unit(10bit) or 1 quadrature
 * encoder edge depending on selected sensor.
 *
 * I gain is specified in throttle per integrated error. For example, a value
 * of 10 equates to ~0.99% (which is 10/1023) for each accumulated ADC unit
 * (10 bit) or 1 quadrature encoder edge depending on selected sensor.
 * Close loop and integral accumulator runs every 1ms.
 *
 * D gain is specified in throttle per derivative error. For example a value of
 * 102 equates to ~9.9% (which is 102/1023) per change of 1 unit (ADC or
 * encoder) per ms.
 *
 * I Zone is specified in the same units as sensor position (ADC units or
 * quadrature edges).  If pos/vel error is outside of this value, the
 * integrated error will auto-clear...
 *   if(   (IZone!=0)  and  abs(err) > IZone)
 *       ClearIaccum()
 * ...this is very useful in preventing integral windup and is highly
 * recommended if using full PID to keep stability low.
 *
 * CloseLoopRampRate is in throttle units per 1ms.  Set to zero to disable
 * ramping.  Works the same as RampThrottle but only is in effect when a close
 * loop mode and profile slot is selected.
 *
 */
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

    public class MotController_LowLevel : Device_LowLevel
    {
        protected const UInt32 STATUS_01 = 0x041400;
        const UInt32 STATUS_02 = 0x041440;
        const UInt32 STATUS_03 = 0x041480;
        const UInt32 STATUS_04 = 0x0414C0;
        const UInt32 STATUS_05 = 0x041500;
        const UInt32 STATUS_06 = 0x041540;
        const UInt32 STATUS_07 = 0x041580;
        const UInt32 STATUS_08 = 0x0415C0;
        protected const UInt32 STATUS_09 = 0x041600;
        const UInt32 STATUS_10 = 0x041640;
        const UInt32 STATUS_11 = 0x041680;
        const UInt32 STATUS_12 = 0x0416C0;
        const UInt32 STATUS_13 = 0x041700;
        const UInt32 STATUS_14 = 0x041740;
        const UInt32 STATUS_15 = 0x041780;

        const UInt32 CONTROL_1 = 0x040000;
        const UInt32 CONTROL_2 = 0x040040;
        const UInt32 CONTROL_3 = 0x040080;
        const UInt32 CONTROL_5 = 0x040100;
        protected const UInt32 CONTROL_6 = 0x040140;

        const UInt32 PARAM_REQ = 0x041800;
        const UInt32 PARAM_RESP = 0x041840;
        const UInt32 PARAM_SET = 0x041880;

        const float FLOAT_TO_FXP_10_22 = (float)0x400000;
        const float FXP_TO_FLOAT_10_22 = 0.0000002384185791015625f;

        const float FLOAT_TO_FXP_0_8 = (float)0x100;
        const float FXP_TO_FLOAT_0_8 = 0.00390625f;

        /* Motion Profile status bits */
        const int kMotionProfileFlag_ActTraj_IsValid = 0x1;
        const int kMotionProfileFlag_HasUnderrun = 0x2;
        const int kMotionProfileFlag_IsUnderrun = 0x4;
        const int kMotionProfileFlag_ActTraj_IsLast = 0x8;
        const int kMotionProfileFlag_ActTraj_VelOnly = 0x10;

        /* Motion Profile Set Output */
        // Motor output is neutral, Motion Profile Executer is not running.
        const int kMotionProf_Disabled = 0;
        // Motor output is updated from Motion Profile Executer, MPE will
        // process the buffered points.
        const int kMotionProf_Enable = 1;
        // Motor output is updated from Motion Profile Executer, MPE will
        // stay processing current trajectory point.
        const int kMotionProf_Hold = 2;

        const int kDefaultControl6PeriodMs = 10;

        private UInt64 _cache;
        private UInt32 _len;

        ErrorCode _lastError = ErrorCode.OK;

        //--------------------- Constructors -----------------------------//
        /**
         * Constructor for the CANTalon device.
         * @param deviceNumber The CAN ID of the Talon SRX
         * @param externalEnable pass true to prevent sending enable frames.
         *  	This can be useful when having one device enable the Talon, and
         * 		another to control it.
         */
        public MotController_LowLevel(int baseArbId, bool externalEnable = false)
            : base((uint)baseArbId, (uint)baseArbId | STATUS_05, (uint)baseArbId | PARAM_REQ, (uint)baseArbId | PARAM_RESP, (uint)baseArbId | PARAM_SET, (uint)baseArbId | STATUS_15)
        {
            if (false == externalEnable)
                CTRE.Native.CAN.Send(CONTROL_2 | (uint)_baseArbId, 0x00, 2, 50);
            CTRE.Native.CAN.Send(CONTROL_3 | (uint)_baseArbId, 0x00, 8, 10);
        }

        //------ Set output routines. ----------//
        public void SetDemand(ControlMode mode, int demand0, int demand1)
        {
            /* check for ship firm */
            CheckFirm(0x0100, "This is ship firmware and needs to be updated.");

            /* get the frame */
            int retval = CTRE.Native.CAN.GetSendBuffer(CONTROL_3 | _baseArbId, ref _cache);
            if (retval != 0) { return; }

            /* unpack */
            byte d0_h8 = (byte)(demand0 >> 0x10);
            byte d0_m8 = (byte)(demand0 >> 0x08);
            byte d0_l8 = (byte)(demand0);
            byte d1_h8 = (byte)(demand1 >> 10);
            byte d1_m8 = (byte)(demand1 >> 2);
            byte d1_l2 = (byte)(demand1 & 0x03);
            int mode_4b = (int)mode & 0xf;

            /* clear */
            _cache &= ~(0xFFul << 0x00);    /* demand0 */
            _cache &= ~(0xFFul << 0x08);    /* demand0 */
            _cache &= ~(0xFFul << 0x10);    /* demand0 */
            _cache &= ~(0xFFul << 0x18);    /* demand1 */
            _cache &= ~(0xFFul << 0x20);    /* demand1 */
            _cache &= ~(0xE0ul << 0x28);    /* demand1 */
            _cache &= ~(0x0Ful << 0x28);    /* mode_4b */

            /* shift in */
            _cache |= (UInt64)(d0_h8) << 0x00;
            _cache |= (UInt64)(d0_m8) << 0x08;
            _cache |= (UInt64)(d0_l8) << 0x10;
            _cache |= (UInt64)(d1_h8) << 0x18;
            _cache |= (UInt64)(d1_m8) << 0x20;
            _cache |= (UInt64)(d1_l2) << (0x28 + 6);
            _cache |= (UInt64)(mode_4b) << (0x28);

            /* flush changes */
            CTRE.Native.CAN.Send(CONTROL_3 | _baseArbId, _cache, 8, 0xFFFFFFFF);
        }

        public void SelectDemandType(bool enable)
        {
            SetClrBit(enable ? 1 : 0, 0x30 + 6, CONTROL_3); /* SelectDemandType */
        }

        public void SetNeutralMode(NeutralMode neutralMode)
        {
            byte sig_b2 = (byte)((int)neutralMode & 3);
            SetClrSmallVal(sig_b2, 2, 6, 0, CONTROL_3); /* OverrideBrakeEn */
        }

        //------ Invert behavior ----------//
        public void SetSensorPhase(bool PhaseSensor)
        {
            int aBit = PhaseSensor ? 1 : 0;
            SetClrSmallVal(aBit, 1, 7, 7, CONTROL_3);
        }

        public void SetInverted(bool invert)
        {
            int aBit = invert ? 1 : 0;
            SetClrSmallVal(aBit, 1, 7, 6, CONTROL_3);
        }
        //----- private utility ------------------//
        private int CalcPercPer10Ms(float secondsFromNeutralToFull)
        {
            /* if seconds is zero(or negative) that means disable ramp */
            if (secondsFromNeutralToFull <= 0) { return 0; }
            /* user wants to enable a ramp*/
            int percPer10ms;
            /* user wants to disable */
            percPer10ms = (int)(1023 / (secondsFromNeutralToFull * 100));
            /* if ramps is super slow, step size falls to zero, ensure we are as slow as can be */
            if (percPer10ms == 0) { percPer10ms = 1; }
            /* return [1,+inf] percent per 10ms*/
            return percPer10ms;

        }
        private int CalcMotorOutput(float percentOut)
        {
            return (int)(1023 * percentOut);
        }
        private byte CalcMotorDeadband(float percentOut)
        {
            int retval = (int)(2049 * percentOut);
            if (retval > byte.MaxValue)
                return byte.MaxValue;
            return (byte)retval;
        }
        private int CalcVoltage_8_8(float voltage)
        {
            return (int)(256.0 * voltage);
        }
        int BoundAboveOne(int param)
        {
            if (param < 1) { return 1; }
            return param;
        }

        //----- general output shaping ------------------//
        public ErrorCode ConfigOpenloopRamp(float secondsFromNeutralToFull, int timeoutMs)
        {
            int ramp = CalcPercPer10Ms(secondsFromNeutralToFull);
            return ConfigSetParameter(ParamEnum.OpenloopRamp, ramp, 0, 0, timeoutMs);
        }
        public ErrorCode ConfigClosedloopRamp(float secondsFromNeutralToFull, int timeoutMs)
        {
            int ramp = CalcPercPer10Ms(secondsFromNeutralToFull);
            return ConfigSetParameter(ParamEnum.ClosedloopRamp, ramp, 0, 0, timeoutMs);
        }
        public ErrorCode ConfigPeakOutputForward(float percentOut, int timeoutMs)
        {
            int param = CalcMotorOutput(percentOut);
            return ConfigSetParameter(ParamEnum.PeakPosOutput, param, 0, 0, timeoutMs);
        }
        public ErrorCode ConfigPeakOutputReverse(float percentOut, int timeoutMs)
        {
            int param = CalcMotorOutput(percentOut);
            return ConfigSetParameter(ParamEnum.PeakNegOutput, param, 0, 0, timeoutMs);
        }
        public ErrorCode ConfigNominalOutputForward(float percentOut, int timeoutMs)
        {
            int param = CalcMotorOutput(percentOut);
            return ConfigSetParameter(ParamEnum.NominalPosOutput, param, 0, 0, timeoutMs);
        }
        public ErrorCode ConfigNominalOutputReverse(float percentOut, int timeoutMs)
        {
            int param = CalcMotorOutput(percentOut);
            return ConfigSetParameter(ParamEnum.NominalNegOutput, param, 0, 0, timeoutMs);
        }
        public ErrorCode ConfigOpenLoopNeutralDeadband(float percentDeadband, int timeoutMs)
        {
            byte param = CalcMotorDeadband(percentDeadband);
            return ConfigSetParameter(ParamEnum.OpenloopDeadband, param, 0, 0, timeoutMs);
        }
        public ErrorCode ConfigClosedLoopNeutralDeadband(float percentDeadband, int timeoutMs)
        {
            byte param = CalcMotorDeadband(percentDeadband);
            return ConfigSetParameter(ParamEnum.ClosedloopDeadband, param, 0, 0, timeoutMs);
        }

        //------ Voltage Compensation ----------//
        public ErrorCode ConfigVoltageCompSaturation(float voltage, int timeoutMs)
        {
            int param = CalcVoltage_8_8(voltage);
            return ConfigSetParameter(ParamEnum.NominalBatteryVoltage, param, 0, 0, timeoutMs);
        }
        public ErrorCode ConfigVoltageMeasurementFilter(int filterWindowSamples, int timeoutMs)
        {
            int param = BoundAboveOne(filterWindowSamples);
            return ConfigSetParameter(ParamEnum.BatteryVoltageFilterSize, param, 0, 0, timeoutMs);
        }
        public void EnableVoltageCompensation(bool enable)
        {
            SetClrBit(enable ? 1 : 0, 5 * 8 + 4, CONTROL_3); /* EnableVoltageCompen */
        }

        //------ General Status ----------//
        public ErrorCode GetBusVoltage(out float param)
        {
            int retval = CTRE.Native.CAN.Receive(STATUS_04 | _baseArbId, ref _cache, ref _len);
            byte L = (byte)(_cache >> 48);
            Int32 raw = 0;
            raw |= L;
            param = 0.05F * raw + 4F;
            return SetLastError(retval);
        }
        public ErrorCode GetMotorOutputPercent(out float param)
        {
            int retval = CTRE.Native.CAN.Receive(STATUS_01 | _baseArbId, ref _cache, ref _len);
            byte H = (byte)(_cache >> 24);
            byte L = (byte)(_cache >> 32);
            H &= 0x7;
            L &= 0xff;
            Int32 raw = 0;
            raw |= H;
            raw <<= 8;
            raw |= L;
            raw <<= (32 - 11);
            raw >>= (32 - 11);
            param = (int)raw;
            return SetLastError(retval);
        }
        public ErrorCode GetOutputCurrent(out float param)
        {
            int retval = CTRE.Native.CAN.Receive(STATUS_02 | _baseArbId, ref _cache, ref _len);
            byte H = (byte)(_cache >> 40);
            byte L = (byte)(_cache >> 48);
            H &= 0xff;
            L &= 0xc0;
            Int32 raw = 0;
            raw |= H;
            raw <<= 8;
            raw |= L;
            raw >>= 6;
            param = 0.125F * raw + 0F;
            return SetLastError(retval);
        }
        public ErrorCode GetTemperature(out float param)
        {
            int retval = CTRE.Native.CAN.Receive(STATUS_04 | _baseArbId, ref _cache, ref _len);
            byte L = (byte)(_cache >> 40);
            Int32 raw = 0;
            raw |= L;
            param = 0.645161290322581F * raw + -50F;
            return SetLastError(retval);
        }
        //------ sensor selection ----------//
        public ErrorCode ConfigSelectedFeedbackSensor(FeedbackDevice feedbackDevice, int timeoutMs)
        {
            int param = (int)feedbackDevice;
            return ConfigSetParameter(ParamEnum.FeedbackSensorType, param, 0, 0, timeoutMs);
        }
        public ErrorCode ConfigRemoteFeedbackFilter(int arbId, int peripheralIdx, int reserved, int timeoutMs)
        {
            throw new NotImplementedException();
        }
        //------- sensor status --------- //
        public ErrorCode GetSelectedSensorPosition(out int param)
        {
            int err = CTRE.Native.CAN.Receive(STATUS_02 | _baseArbId, ref _cache, ref _len);
            byte H = (byte)(_cache >> 0);
            byte M = (byte)(_cache >> 8);
            byte L = (byte)(_cache >> 16);
            param = 0;
            param |= H;
            param <<= 8;
            param |= M;
            param <<= 8;
            param |= L;
            param <<= (32 - 24); /* sign extend */
            param >>= (32 - 24); /* sign extend */
            return SetLastError(err);
        }

        public ErrorCode GetSelectedSensorVelocity(out int param)
        {
            int err = CTRE.Native.CAN.Receive(STATUS_02 | _baseArbId, ref _cache, ref _len);
            byte H = (byte)(_cache >> 24);
            byte L = (byte)(_cache >> 32);
            int velDiv4 = (int)((_cache >> 60) & 1);
            param = 0;
            param |= H;
            param <<= 8;
            param |= L;
            param <<= (32 - 16); /* sign extend */
            param >>= (32 - 16); /* sign extend */
            if (velDiv4 == 1)
                param *= 4;
            return SetLastError(err);
        }

        public ErrorCode SetSelectedSensorPosition(int sensorPos, int timeoutMs)
        {
            int param = (int)sensorPos;
            return ConfigSetParameter(ParamEnum.SelectedSensorPosition, param, 0, 0, timeoutMs);
        }
        //------ status frame period changes ----------//
        public ErrorCode SetControlFramePeriod(ControlFrame frame, int periodMs)
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
        public ErrorCode SetStatusFramePeriod(StatusFrame frame, int periodMs, int timeoutMs)
        {
            int fullId = (int)((int)_baseArbId | (int)frame); /* build ID */

            return base.SetStatusFramePeriod(fullId, periodMs, timeoutMs);
        }
        public ErrorCode SetStatusFramePeriod(StatusFrameEnhanced frame, int periodMs, int timeoutMs)
        {
            int fullId = (int)((int)_baseArbId | (int)frame); /* build ID */

            return base.SetStatusFramePeriod(fullId, periodMs, timeoutMs);
        }
        public ErrorCode GetStatusFramePeriod(StatusFrame frame, out int periodMs, int timeoutMs)
        {
            int fullId = (int)((int)_baseArbId | (int)frame); /* build ID */

            return base.GetStatusFramePeriod(fullId, out periodMs, timeoutMs);
        }
        public ErrorCode GetStatusFramePeriod(StatusFrameEnhanced frame, out int periodMs, int timeoutMs)
        {
            int fullId = (int)((int)_baseArbId | (int)frame); /* build ID */

            return base.GetStatusFramePeriod(fullId, out periodMs, timeoutMs);
        }
        //----- velocity signal conditionaing ------//
        public ErrorCode ConfigVelocityMeasurementPeriod(VelocityMeasPeriod period, int timeoutMs)
        {
            int param = (int)period;
            return ConfigSetParameter(ParamEnum.SampleVelocityPeriod, param, 0, 0, timeoutMs);
        }

        public ErrorCode ConfigVelocityMeasurementWindow(int windowSize, int timeoutMs)
        {
            int param = (int)windowSize;
            return ConfigSetParameter(ParamEnum.SampleVelocityWindow, param, 0, 0, timeoutMs);
        }
        //------ ALL limit switch ----------//
        public ErrorCode ConfigForwardLimitSwitchSource(LimitSwitchSource type, LimitSwitchNormal normalOpenOrClose, int deviceIDIfApplicable, int timeoutMs)
        {
            throw new NotImplementedException();
        }

        public ErrorCode ConfigReverseLimitSwitchSource(LimitSwitchSource type, LimitSwitchNormal normalOpenOrClose, int deviceIDIfApplicable, int timeoutMs)
        {
            throw new NotImplementedException();
        }
        public void EnableLimitSwitches(bool enable)
        {
            SetClrBit(enable ? 0 : 1, 0x30 + 7, CONTROL_3); /* LimitSwitchDisable */
        }
        //------ soft limit ----------//
        public ErrorCode ConfigForwardSoftLimit(int forwardSensorLimit, int timeoutMs)
        {
            int param = (int)forwardSensorLimit;
            return ConfigSetParameter(ParamEnum.ForwardSoftLimitThreshold, param, 0, 0, timeoutMs);
        }
        public ErrorCode ConfigReverseSoftLimit(int reverseSensorLimit, int timeoutMs)
        {
            int param = (int)reverseSensorLimit;
            return ConfigSetParameter(ParamEnum.ReverseSoftLimitThreshold, param, 0, 0, timeoutMs);
        }
        public void EnableSoftLimits(bool enable)
        {
            SetClrBit((!enable) ? 1 : 0, 8 * 5 + 5, CONTROL_3); /* DisableSoftLimits */
        }

        //------ Current Lim ----------//
        public ErrorCode ConfigPeakCurrentLimit(int amps, int timeoutMs)
        {
            int param = BoundAboveOne(amps);
            return ConfigSetParameter(ParamEnum.PeakCurrentLimitAmps, param, 0, 0, timeoutMs);
        }
        public ErrorCode ConfigPeakCurrentDuration(int milliseconds, int timeoutMs)
        {
            int param = BoundAboveOne(milliseconds);
            return ConfigSetParameter(ParamEnum.ContinuousCurrentLimitMs, param, 0, 0, timeoutMs);
        }
        public ErrorCode ConfigContinuousCurrentLimit(int amps, int timeoutMs)
        {
            int param = BoundAboveOne(amps);
            return ConfigSetParameter(ParamEnum.ContinuousCurrentLimitAmps, param, 0, 0, timeoutMs);
        }
        public void EnableCurrentLimit(bool enable)
        {
            SetClrBit(enable ? 1 : 0, 0x38 + 4, CONTROL_3); /* EnCurrentLimit */
        }

        //------ General Close loop ----------//
        public ErrorCode Config_kP(int slotIdx, float value, int timeoutMs)
        {
            return ConfigSetParameter(ParamEnum.ProfileParamSlot_P, value, 0x00, slotIdx, timeoutMs);
        }
        public ErrorCode Config_kI(int slotIdx, float value, int timeoutMs)
        {
            return ConfigSetParameter(ParamEnum.ProfileParamSlot_I, value, 0x00, slotIdx, timeoutMs);
        }
        public ErrorCode Config_kD(int slotIdx, float value, int timeoutMs)
        {
            return ConfigSetParameter(ParamEnum.ProfileParamSlot_D, value, 0x00, slotIdx, timeoutMs);
        }
        public ErrorCode Config_kF(int slotIdx, float value, int timeoutMs)
        {
            return ConfigSetParameter(ParamEnum.ProfileParamSlot_F, value, 0x00, slotIdx, timeoutMs);
        }
        public ErrorCode Config_IntegralZone(int slotIdx, int izone, int timeoutMs)
        {
            return ConfigSetParameter(ParamEnum.ProfileParamSlot_IZone, izone, 0x00, slotIdx, timeoutMs);
        }
        public ErrorCode ConfigAllowableClosedloopError(int slotIdx, int allowableCloseLoopError, int timeoutMs)
        {
            return ConfigSetParameter(ParamEnum.ProfileParamSlot_AllowableClosedLoopErr, allowableCloseLoopError, 0x00, slotIdx, timeoutMs);
        }
        public ErrorCode ConfigMaxIntegralAccumulator(int slotIdx, float iaccum = 0, int timeoutMs = 0)
        {
            return ConfigSetParameter(ParamEnum.ProfileParamSlot_MaxIAccum, iaccum, 0x00, slotIdx, timeoutMs);
        }

        public ErrorCode SetIntegralAccumulator(float iaccum = 0, int timeoutMs = 0)
        {
            return ConfigSetParameter(ParamEnum.ClosedLoopIAccum, iaccum, 0x00, 0x00, timeoutMs);
        }

        public ErrorCode GetClosedLoopError(out int error, int pidIdx = 0)
        {
            uint statusID = (pidIdx == 0) ? STATUS_13 : STATUS_14;
            int err = CTRE.Native.CAN.Receive(statusID | _baseArbId, ref _cache, ref _len);
            byte H = (byte)(_cache >> 0x00);
            byte M = (byte)(_cache >> 0x08);
            byte L = (byte)(_cache >> 0x10);
            int param;
            param = 0;
            param |= H;
            param <<= 8;
            param |= M;
            param <<= 8;
            param |= L;
            param <<= (32 - 24); /* sign extend */
            param >>= (32 - 24); /* sign extend */
            error = param;
            return SetLastError(err);
        }
        public ErrorCode GetIntegralAccumulator(out float iaccum, int pidIdx = 0)
        {
            uint statusID = (pidIdx == 0) ? STATUS_13 : STATUS_14;
            int err = CTRE.Native.CAN.Receive(statusID | _baseArbId, ref _cache, ref _len);
            byte H = (byte)(_cache >> 0x18);
            byte M = (byte)(_cache >> 0x20);
            byte L = (byte)(_cache >> 0x28);
            int param;
            param = 0;
            param |= H;
            param <<= 8;
            param |= M;
            param <<= 8;
            param |= L;
            param <<= (32 - 24); /* sign extend */
            param >>= (32 - 24); /* sign extend */
            iaccum = param;
            return SetLastError(err);
        }
        public ErrorCode GetErrorDerivative(out float derivError, int pidIdx = 0)
        {
            uint statusID = (pidIdx == 0) ? STATUS_13 : STATUS_14;
            int err = CTRE.Native.CAN.Receive(statusID | _baseArbId, ref _cache, ref _len);
            byte H = (byte)(_cache >> 0x30);
            byte L = (byte)(_cache >> 0x38);
            int param;
            param = 0;
            param |= H;
            param <<= 8;
            param |= L;
            param <<= (32 - 16); /* sign extend */
            param >>= (32 - 16); /* sign extend */
            derivError = param;
            return SetLastError(err);
        }
        public void SelectProfileSlot(int slotIdx)
        {
            SetClrSmallVal(slotIdx, 4, 8, 0, CONTROL_3);
        }
        //------ Motion Profile Settings used in Motion Magic and Motion Profile ----------//

        public ErrorCode ConfigMotionCruiseVelocity(int sensorUnitsPer100ms, int timeoutMs)
        {
            return ConfigSetParameter(ParamEnum.eMotMag_VelCruise, sensorUnitsPer100ms, 0, 0, timeoutMs);
        }
        public ErrorCode ConfigMotionAcceleration(int sensorUnitsPer100msPerSec, int timeoutMs)
        {
            return ConfigSetParameter(ParamEnum.eMotMag_Accel, sensorUnitsPer100msPerSec, 0, 0, timeoutMs);
        }

        //------ Motion Profile Buffer ----------//
        /* implemented in child class */


        //------ Faults ----------//
        public ErrorCode GetFaults(Faults toFill)
        {
            int retval = CTRE.Native.CAN.Receive(STATUS_01 | _baseArbId, ref _cache, ref _len);
            toFill.OverTemp = (((_cache >> 52) & 1) == 1);
            toFill.UnderVoltage = (((_cache >> 51) & 1) == 1);
            toFill.ForwardLimitSwitch = (((_cache >> 50) & 1) == 1);
            toFill.ReverseLimitSwitch = (((_cache >> 49) & 1) == 1);
            toFill.HardwareFailure = (((_cache >> 48) & 1) == 1);
            toFill.ForwardSoftLimit = (((_cache >> 28) & 1) == 1);
            toFill.ReverseSoftLimit = (((_cache >> 27) & 1) == 1);
            toFill.MsgOverflow = false;
            toFill.ResetDuringEn = false;
            return SetLastError(retval);
        }
        public ErrorCode GetStickyFaults(Faults toFill)
        {
            int retval = CTRE.Native.CAN.Receive(STATUS_02 | _baseArbId, ref _cache, ref _len);
            toFill.OverTemp = (((_cache >> 53) & 1) == 1);
            toFill.UnderVoltage = (((_cache >> 52) & 1) == 1);
            toFill.ForwardLimitSwitch = (((_cache >> 51) & 1) == 1);
            toFill.ReverseLimitSwitch = (((_cache >> 50) & 1) == 1);
            toFill.HardwareFailure = false;
            toFill.ForwardSoftLimit = (((_cache >> 49) & 1) == 1);
            toFill.ReverseSoftLimit = (((_cache >> 48) & 1) == 1);
            toFill.MsgOverflow = false;
            toFill.ResetDuringEn = false;
            return SetLastError(retval);
        }
        public ErrorCode ClearStickyFaults(int timeoutMs = 0)
        {
            return ConfigSetParameter(ParamEnum.StickyFaults, 0, 0, 0, timeoutMs);
        }

        //------ Custom Persistent Params ----------//
        public ErrorCode ConfigSetCustomParam(int value, int paramIndex, int timeoutMs)
        {
            return ConfigSetParameter(ParamEnum.CustomParam, value, 0, paramIndex, timeoutMs);
        }
        public ErrorCode ConfigGetCustomParam(out int value, int paramIndex, int timeoutMs)
        {
            return ConfigGetParameter(ParamEnum.CustomParam, out value, paramIndex, timeoutMs);
        }

        //------ Generic Param API, typically not used ----------//
        //public ErrorCode ConfigSetParameter(ParamEnum param, float value, byte subValue = 0, int ordinal = 0, int timeoutMs = 0)
        //{
        //    return base.ConfigSetParameter(param, value, 0, 0, timeoutMs);
        //}

        //------ Motor Controller Sensor Collection API ---------//
        public ErrorCode GetAnalogInWithOv(out int param)
        {
            int retval = CTRE.Native.CAN.Receive(STATUS_04 | _baseArbId, ref _cache, ref _len);
            byte H = (byte)(_cache >> 0);
            byte M = (byte)(_cache >> 8);
            byte L = (byte)(_cache >> 16);
            Int32 raw = 0;
            raw |= H;
            raw <<= 8;
            raw |= M;
            raw <<= 8;
            raw |= L;
            raw <<= (32 - 24); /* sign extend */
            raw >>= (32 - 24); /* sign extend */
            param = (int)raw;
            return SetLastError(retval);
        }
        public ErrorCode GetAnalogInVel(out int param)
        {
            int retval = CTRE.Native.CAN.Receive(STATUS_04 | _baseArbId, ref _cache, ref _len);
            byte H = (byte)(_cache >> 24);
            byte L = (byte)(_cache >> 32);
            Int32 raw = 0;
            raw |= H;
            raw <<= 8;
            raw |= L;
            raw <<= (32 - 16); /* sign extend */
            raw >>= (32 - 16); /* sign extend */
            param = (int)raw;
            return SetLastError(retval);
        }

        /**
         * Get the position of whatever is in the analog pin of the Talon, regardless of
         * whether it is actually being used for feedback.
         *
         * @returns The value (0 - 1023) on the analog pin of the Talon.
         */
        public ErrorCode GetQuadraturePosition(out int param)
        {
            int retval = CTRE.Native.CAN.Receive(STATUS_03 | _baseArbId, ref _cache, ref _len);
            byte H = (byte)(_cache >> 0);
            byte M = (byte)(_cache >> 8);
            byte L = (byte)(_cache >> 16);
            Int32 raw = 0;
            raw |= H;
            raw <<= 8;
            raw |= M;
            raw <<= 8;
            raw |= L;
            raw <<= (32 - 24); /* sign extend */
            raw >>= (32 - 24); /* sign extend */
            param = (int)raw;
            return SetLastError(retval);
        }

        /**
         * Get the position of whatever is in the analog pin of the Talon, regardless of
         * whether it is actually being used for feedback.
         *
         * @returns The value (0 - 1023) on the analog pin of the Talon.
         */
        public ErrorCode GetQuadratureVelocity(out int param)
        {
            int retval = CTRE.Native.CAN.Receive(STATUS_03 | _baseArbId, ref _cache, ref _len);
            byte H = (byte)(_cache >> 24);
            byte L = (byte)(_cache >> 32);
            Int32 raw = 0;
            raw |= H;
            raw <<= 8;
            raw |= L;
            raw <<= (32 - 16); /* sign extend */
            raw >>= (32 - 16); /* sign extend */
            param = (int)raw;
            return SetLastError(retval);
        }
        public ErrorCode GetPulseWidthPosition(out int param)
        {
            int retval = CTRE.Native.CAN.Receive(STATUS_08 | _baseArbId, ref _cache, ref _len);
            byte H = (byte)(_cache >> 0);
            byte M = (byte)(_cache >> 8);
            byte L = (byte)(_cache >> 16);
            Int32 raw = 0;
            raw |= H;
            raw <<= 8;
            raw |= M;
            raw <<= 8;
            raw |= L;
            raw <<= (32 - 24); /* sign extend */
            raw >>= (32 - 24); /* sign extend */
            param = (int)raw;
            return SetLastError(retval);
        }
        public ErrorCode GetPulseWidthVelocity(out int param)
        {
            int retval = CTRE.Native.CAN.Receive(STATUS_08 | _baseArbId, ref _cache, ref _len);
            byte H = (byte)(_cache >> 48);
            byte L = (byte)(_cache >> 56);
            Int32 raw = 0;
            raw |= H;
            raw <<= 8;
            raw |= L;
            raw <<= (32 - 16); /* sign extend */
            raw >>= (32 - 16); /* sign extend */
            param = (int)raw;
            return SetLastError(retval);
        }
        public ErrorCode GetPulseWidthRiseToFallUs(out int param)
        {
            const int BIT12 = 1 << 12;
            int temp = 0;
            int periodUs = 0;
            /* first grab our 12.12 position */
            ErrorCode retval1 = GetPulseWidthPosition(out temp);
            /* mask off number of turns */
            temp &= 0xFFF;
            /* next grab the waveform period. This value
             * will be zero if we stop getting pulses **/
            ErrorCode retval2 = GetPulseWidthRiseToRiseUs(out periodUs);
            /* now we have 0.12 position that is scaled to the waveform period.
                    Use fixed pt multiply to scale our 0.16 period into us.*/
            param = (temp * periodUs) / BIT12;
            /* pass the first error code */
            if (retval1 == 0)
                retval1 = retval2;
            return SetLastError(retval1);
        }
        public ErrorCode GetPulseWidthRiseToRiseUs(out int param)
        {
            int retval = CTRE.Native.CAN.Receive(STATUS_08 | _baseArbId, ref _cache, ref _len);
            byte H = (byte)(_cache >> 32);
            byte L = (byte)(_cache >> 40);
            Int32 raw = 0;
            raw |= H;
            raw <<= 8;
            raw |= L;
            raw <<= (32 - 16); /* sign extend */
            raw >>= (32 - 16); /* sign extend */
            param = (int)raw;
            return SetLastError(retval);
        }
        public ErrorCode GetPinStateQuadA(out int param)
        {
            int retval = CTRE.Native.CAN.Receive(STATUS_03 | _baseArbId, ref _cache, ref _len);
            byte L = (byte)(_cache >> 63);
            param = (L & 1);
            return SetLastError(retval);
        }
        /**
         * @return IO level of QUADB pin.
         */
        public ErrorCode GetPinStateQuadB(out int param)
        {
            int retval = CTRE.Native.CAN.Receive(STATUS_03 | _baseArbId, ref _cache, ref _len);
            byte L = (byte)(_cache >> 62);
            param = (L & 1);
            return SetLastError(retval);
        }
        /**
         * @return IO level of QUAD Index pin.
         */
        public ErrorCode GetPinStateQuadIdx(out int param)
        {
            int retval = CTRE.Native.CAN.Receive(STATUS_03 | _baseArbId, ref _cache, ref _len);
            byte L = (byte)(_cache >> 61);
            param = (L & 1);
            return SetLastError(retval);
        }
        /**
         * @return '1' iff forward limit switch is closed, 0 iff switch is open.
         * This function works regardless if limit switch feature is enabled.
         */
        public ErrorCode IsFwdLimitSwitchClosed(out int param)
        {
            throw new NotImplementedException();
        }
        /**
         * @return '1' iff reverse limit switch is closed, 0 iff switch is open.
         * This function works regardless if limit switch feature is enabled.
         */
        public ErrorCode IsRevLimitSwitchClosed(out int param)
        {
            throw new NotImplementedException();
        }


        //------ Utility ----------//
        public void SetClrBit(int bit, int shift, uint baseId)
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
        public void SetClrSmallVal(int value, int bitTotal, int byteIdx, int bitShift_LE, uint baseId)
        {
            UInt64 valu = (UInt64)value;
            /* get the frame */
            int retval = CTRE.Native.CAN.GetSendBuffer(baseId | _baseArbId, ref _cache);
            if (retval != 0) { return; }

            /* make the mask */
            UInt64 mask = 1;
            mask <<= bitTotal;
            --mask;

            /* shfit byte mask and byt value to correct spot within byte */
            mask <<= bitShift_LE;
            valu <<= bitShift_LE;

            /* clear mask within byte*/
            _cache &= ~(mask << (byteIdx * 8));

            /* shift in value within byte */
            _cache |= valu << (byteIdx * 8);

            /* flush changes */
            CTRE.Native.CAN.Send(baseId | _baseArbId, _cache, 8, 0xFFFFFFFF);
        }

        //------ Misc ----------//
        void CheckFirm(int minFirm, string message)
        {
            /* get the firm vers */
            int vers = GetFirmwareVersion();
            /* if its not available skip test */
            if (vers < 0) { return; }
            /* tell user if its too old */
            if (vers < minFirm) { Reporting.ConsolePrint(message); }
        }
        //-------------------------------- Device_LowLevel requirements -----------------------//
        protected override void EnableFirmStatusFrame(bool enable)
        {
            SetClrBit(enable ? 0 : 1, 7 * 8 + 5, CONTROL_3); /* DisableFirmStatusFrame */
        }
        protected override ErrorCode SetLastError(ErrorCode errorCode)
        {
            _lastError = errorCode;
            return _lastError;
        }
        public ErrorCode GetLastError()
        {
            return _lastError;
        }

        protected ErrorCode SetLastError(int errorCode)
        {
            _lastError = (ErrorCode)errorCode;
            return _lastError;
        }
    }
}