using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using CTRE.Phoenix.Motion;
using CTRE.Phoenix.LowLevel;

namespace CTRE.Phoenix.MotorControl.CAN
{
    public abstract class BaseMotorController : IMotorController
    {
        SensorCollection _sensorColl;
        protected MotControllerWithBuffer_LowLevel _ll;
        private int[] _motionProfStats = new int[11];

        private ControlMode m_controlMode = ControlMode.PercentOutput;
        private ControlMode m_sendMode = ControlMode.PercentOutput;

        int _arbId;
        bool _invert = false;

        // FeedbackDevice m_feedbackDevice = FeedbackDevice.QuadEncoder;

        //--------------------- Constructors -----------------------------//
        /**
         * Constructor for the CANTalon device.
         * @param deviceNumber The CAN ID of the Talon SRX
         * @param externalEnable pass true to prevent sending enable frames.
         *  	This can be useful when having one device enable the Talon, and
         * 		another to control it.
         */
        public BaseMotorController(int arbId, bool externalEnable = false)
        {
            _arbId = arbId;
            _ll = new MotControllerWithBuffer_LowLevel(arbId, externalEnable);
            _sensorColl = new SensorCollection(_ll);
        }
        //------ Set output routines. ----------//
        // ------ Set output routines. ----------//
        /**
         * Sets the appropriate output on the talon, depending on the mode.
         * @param mode The output mode to apply.
         * In PercentOutput, the output is between -1.0 and 1.0, with 0.0 as stopped.
         * In Current mode, output value is in amperes.
         * In Velocity mode, output value is in position change / 100ms.
         * In Position mode, output value is in encoder ticks or an analog value,
         *   depending on the sensor.
         * In Follower mode, the output value is the integer device ID of the talon to
         * duplicate.
         *
         * @param outputValue The setpoint value, as described above.
         *
         *
         *	Standard Driving Example:
         *	_talonLeft.set(ControlMode.PercentOutput, leftJoy);
         *	_talonRght.set(ControlMode.PercentOutput, rghtJoy);
         */
        public void Set(ControlMode mode, double outputValue)
        {
            Set(mode, outputValue, DemandType.Neutral, 0);
        }

        /**
	 * @param mode Sets the appropriate output on the talon, depending on the mode.
	 * @param demand0 The output value to apply.
	 * 	such as advanced feed forward and/or auxiliary close-looping in firmware.
	 * In PercentOutput, the output is between -1.0 and 1.0, with 0.0 as stopped.
	 * In Current mode, output value is in amperes.
	 * In Velocity mode, output value is in position change / 100ms.
	 * In Position mode, output value is in encoder ticks or an analog value,
	 *   depending on the sensor. See
	 * In Follower mode, the output value is the integer device ID of the talon to
	 * duplicate.
	 *
	 * @param demand1Type The demand type for demand1.
	 * Neutral: Ignore demand1 and apply no change to the demand0 output.
	 * AuxPID: Use demand1 to set the target for the auxiliary PID 1.
	 * ArbitraryFeedForward: Use demand1 as an arbitrary additive value to the
	 *	 demand0 output.  In PercentOutput the demand0 output is the motor output,
	 *   and in closed-loop modes the demand0 output is the output of PID0.
	 * @param demand1 Supplmental output value.  Units match the set mode.
	 *
	 *
	 *  Arcade Drive Example:
	 *		_talonLeft.set(ControlMode.PercentOutput, joyForward, DemandType.ArbitraryFeedForward, +joyTurn);
	 *		_talonRght.set(ControlMode.PercentOutput, joyForward, DemandType.ArbitraryFeedForward, -joyTurn);
	 *
	 *	Drive Straight Example:
	 *	Note: Selected Sensor Configuration is necessary for both PID0 and PID1.
	 *		_talonLeft.follow(_talonRght, FollwerType.AuxOutput1);
	 *		_talonRght.set(ControlMode.PercentOutput, joyForward, DemandType.AuxPID, desiredRobotHeading);
	 *
	 *	Drive Straight to a Distance Example:
	 *	Note: Other configurations (sensor selection, PID gains, etc.) need to be set.
	 *		_talonLeft.follow(_talonRght, FollwerType.AuxOutput1);
	 *		_talonRght.set(ControlMode.MotionMagic, targetDistance, DemandType.AuxPID, desiredRobotHeading);
	 */
        public void Set(ControlMode mode, double demand0, DemandType demand1Type, double demand1)
        {
            m_controlMode = mode;
            m_sendMode = mode;
            int work;

            switch (m_controlMode)
            {
                case ControlMode.PercentOutput:
                    // case TimedPercentOutput:
                    _ll.Set(m_sendMode, demand0, demand1, (int)demand1Type);
                    break;
                case ControlMode.Follower:
                    /* did caller specify device ID */
                    if ((0 <= demand0) && (demand0 <= 62))
                    { // [0,62]
                        work = GetBaseID();
                        work >>= 16;
                        work <<= 8;
                        work |= ((int)demand0) & 0xFF;
                    }
                    else
                    {
                        work = (int)demand0;
                    }
                    /* single precision guarantees 16bits of integral precision,
                   * so float/double cast on work is safe */
                    _ll.Set(m_sendMode, (double)work, demand1, (int)demand1Type);
                    break;
                case ControlMode.Velocity:
                case ControlMode.Position:
                case ControlMode.MotionMagic:
                case ControlMode.MotionProfile:
                case ControlMode.MotionProfileArc:
                    _ll.Set(m_sendMode, demand0, demand1, (int)demand1Type);
                    break;
                case ControlMode.Disabled:
                /* fall thru... */
                default:
                    _ll.SetDemand(m_sendMode, 0, 0);
                    break;
            }

        }
        public void NeutralOutput()
        {
            Set(ControlMode.Disabled, 0);
        }
        public void SetNeutralMode(NeutralMode neutralMode)
        {
            _ll.SetNeutralMode(neutralMode);
        }

        //------ Invert behavior ----------//
        public void SetSensorPhase(bool PhaseSensor)
        {
            _ll.SetSensorPhase(PhaseSensor);
        }
        public void SetInverted(bool invert)
        {
            _invert = invert; /* cache for getter */
            _ll.SetInverted(_invert);
        }
        public bool GetInverted()
        {
            return _invert;
        }

        public int GetDeviceID()
        {
            return (int)_ll.GetDeviceNumber();
        }

        //----- general output shaping ------------------//
        public ErrorCode ConfigOpenloopRamp(float secondsFromNeutralToFull, int timeoutMs = 0)
        {
            return _ll.ConfigOpenloopRamp(secondsFromNeutralToFull, timeoutMs);
        }
        public ErrorCode ConfigClosedloopRamp(float secondsFromNeutralToFull, int timeoutMs = 0)
        {
            return _ll.ConfigClosedloopRamp(secondsFromNeutralToFull, timeoutMs);
        }
        public ErrorCode ConfigPeakOutputForward(float percentOut, int timeoutMs = 0)
        {
            return _ll.ConfigPeakOutputForward(percentOut, timeoutMs);
        }
        public ErrorCode ConfigPeakOutputReverse(float percentOut, int timeoutMs = 0)
        {
            return _ll.ConfigPeakOutputReverse(percentOut, timeoutMs);
        }
        public ErrorCode ConfigNominalOutputForward(float percentOut, int timeoutMs = 0)
        {
            return _ll.ConfigNominalOutputForward(percentOut, timeoutMs);
        }
        public ErrorCode ConfigNominalOutputReverse(float percentOut, int timeoutMs = 0)
        {
            return _ll.ConfigNominalOutputReverse(percentOut, timeoutMs);
        }
        public ErrorCode ConfigNeutralDeadband(float percentDeadband = Constants.DefaultDeadband, int timeoutMs = 0)
        {
            return _ll.ConfigNeutralDeadband(percentDeadband, timeoutMs);
        }

        //------ Voltage Compensation ----------//
        public ErrorCode ConfigVoltageCompSaturation(float voltage, int timeoutMs = 0)
        {
            return _ll.ConfigVoltageCompSaturation(voltage, timeoutMs);
        }
        public ErrorCode ConfigVoltageMeasurementFilter(int filterWindowSamples, int timeoutMs = 0)
        {
            return _ll.ConfigVoltageMeasurementFilter(filterWindowSamples, timeoutMs);
        }
        public void EnableVoltageCompensation(bool enable)
        {
            _ll.EnableVoltageCompensation(enable);
        }

        //------ General Status ----------//
        public float GetBusVoltage()
        {
            float retval;
            _ll.GetBusVoltage(out retval);
            return retval;
        }
        public float GetMotorOutputPercent()
        {
            float retval;
            _ll.GetMotorOutputPercent(out retval);
            return retval;
        }
        public float GetMotorOutputVoltage()
        {
            float v, p;
            v = GetBusVoltage();
            p = GetMotorOutputPercent();

            float param = v * p;
            return param;
        }
        public float GetOutputCurrent()
        {
            float retval;
            _ll.GetOutputCurrent(out retval);
            return retval;
        }
        public float GetTemperature()
        {
            float retval;
            _ll.GetTemperature(out retval);
            return retval;
        }

        //------ sensor selection ----------//
        public ErrorCode ConfigSelectedFeedbackSensor(RemoteFeedbackDevice feedbackDevice, int pidIdx, int timeoutMs = 0)
        {
            return _ll.ConfigSelectedFeedbackSensor((FeedbackDevice)feedbackDevice, pidIdx, timeoutMs);
        }
        public ErrorCode ConfigSelectedFeedbackSensor(FeedbackDevice feedbackDevice, int pidIdx, int timeoutMs = 0)
        {
            return _ll.ConfigSelectedFeedbackSensor(feedbackDevice, pidIdx, timeoutMs);
        }

        public ErrorCode ConfigSelectedFeedbackCoefficient(float coefficient, int pidIdx, int timeoutMs)
        {
            return _ll.ConfigSelectedFeedbackCoefficient(coefficient, pidIdx, timeoutMs);
        }

        /**
	 * Select what remote device and signal to assign to Remote Sensor 0 or Remote Sensor 1.
	 * After binding a remote device and signal to Remote Sensor X, you may select Remote Sensor X
	 * as a PID source for closed-loop features.
	 *
	 * @param deviceID
 	 *            The CAN ID of the remote sensor device.
	 * @param remoteSensorSource
	 *            The remote sensor device and signal type to bind.
	 * @param remoteOrdinal
	 *            0 for configuring Remote Sensor 0
	 *            1 for configuring Remote Sensor 1
	 * @param timeoutMs
	 *            Timeout value in ms. If nonzero, function will wait for
	 *            config success and report an error if it times out.
	 *            If zero, no blocking or checking is performed.
	 * @return Error Code generated by function. 0 indicates no error.
	 */
        public ErrorCode ConfigRemoteFeedbackFilter(int deviceID, RemoteSensorSource remoteSensorSource, int remoteOrdinal,
                int timeoutMs)
        {
            return _ll.ConfigRemoteFeedbackFilter(deviceID, remoteSensorSource, remoteOrdinal,
                    timeoutMs);
        }
        /**
         * Select what sensor term should be bound to switch feedback device.
         * Sensor Sum = Sensor Sum Term 0 - Sensor Sum Term 1
         * Sensor Difference = Sensor Diff Term 0 - Sensor Diff Term 1
         * The four terms are specified with this routine.  Then Sensor Sum/Difference
         * can be selected for closed-looping.
         *
         * @param sensorTerm Which sensor term to bind to a feedback source.
         * @param feedbackDevice The sensor signal to attach to sensorTerm.
         * @param timeoutMs
         *            Timeout value in ms. If nonzero, function will wait for
         *            config success and report an error if it times out.
         *            If zero, no blocking or checking is performed.
         * @return Error Code generated by function. 0 indicates no error.
         */
        public ErrorCode ConfigSensorTerm(SensorTerm sensorTerm, FeedbackDevice feedbackDevice, int timeoutMs)
        {
            return _ll.ConfigSensorTerm(sensorTerm, feedbackDevice, timeoutMs);
        }

        //------- sensor status --------- //
        public int GetSelectedSensorPosition(int pidIdx)
        {
            int retval;
            _ll.GetSelectedSensorPosition(out retval, pidIdx);
            return retval;
        }
        public int GetSelectedSensorVelocity(int pidIdx)
        {
            int retval;
            _ll.GetSelectedSensorVelocity(out retval, pidIdx);
            return retval;
        }
        public ErrorCode SetSelectedSensorPosition(int sensorPos, int pidIdx, int timeoutMs = 0)
        {
            return _ll.SetSelectedSensorPosition(sensorPos, pidIdx, timeoutMs);
        }

        //------ status frame period changes ----------//
        public ErrorCode SetControlFramePeriod(ControlFrame frame, int periodMs)
        {
            return _ll.SetControlFramePeriod(frame, periodMs);
        }
        public ErrorCode SetStatusFramePeriod(StatusFrame frame, int periodMs, int timeoutMs = 0)
        {
            return _ll.SetStatusFramePeriod(frame, periodMs, timeoutMs);
        }
        public ErrorCode SetStatusFramePeriod(StatusFrameEnhanced frame, int periodMs, int timeoutMs = 0)
        {
            return _ll.SetStatusFramePeriod(frame, periodMs, timeoutMs);
        }
        public ErrorCode GetStatusFramePeriod(StatusFrame frame, out int periodMs, int timeoutMs = 0)
        {
            return _ll.GetStatusFramePeriod(frame, out periodMs, timeoutMs);
        }
        public ErrorCode GetStatusFramePeriod(StatusFrameEnhanced frame, out int periodMs, int timeoutMs = 0)
        {
            return _ll.GetStatusFramePeriod(frame, out periodMs, timeoutMs);
        }

        //----- velocity signal conditionaing ------//
        public ErrorCode ConfigVelocityMeasurementPeriod(VelocityMeasPeriod period, int timeoutMs = 0)
        {
            return _ll.ConfigVelocityMeasurementPeriod(period, timeoutMs);
        }
        public ErrorCode ConfigVelocityMeasurementWindow(int windowSize, int timeoutMs = 0)
        {
            return _ll.ConfigVelocityMeasurementWindow(windowSize, timeoutMs);
        }


        //------ remote limit switch ----------//
        public ErrorCode ConfigForwardLimitSwitchSource(RemoteLimitSwitchSource type, LimitSwitchNormal normalOpenOrClose, int deviceID, int timeoutMs = 0)
        {
            var cciType = LimitSwitchRoutines.Promote(type);
            return _ll.ConfigForwardLimitSwitchSource(cciType, normalOpenOrClose, deviceID, timeoutMs);
        }

        public ErrorCode ConfigReverseLimitSwitchSource(RemoteLimitSwitchSource type, LimitSwitchNormal normalOpenOrClose, int deviceID, int timeoutMs = 0)
        {
            var cciType = LimitSwitchRoutines.Promote(type);
            return _ll.ConfigReverseLimitSwitchSource(cciType, normalOpenOrClose, deviceID, timeoutMs);
        }
        public void OverrideLimitSwitchesEnable(bool enable)
        {
            _ll.OverrideLimitSwitchesEnable(enable);
        }

        //------ local limit switch ----------//
        public ErrorCode ConfigForwardLimitSwitchSource(LimitSwitchSource type, LimitSwitchNormal normalOpenOrClose, int timeoutMs = 0)
        {
            return _ll.ConfigForwardLimitSwitchSource(type, normalOpenOrClose, 0, timeoutMs);
        }
        public ErrorCode ConfigReverseLimitSwitchSource(LimitSwitchSource type, LimitSwitchNormal normalOpenOrClose, int timeoutMs = 0)
        {
            return _ll.ConfigReverseLimitSwitchSource(type, normalOpenOrClose, 0, timeoutMs);
        }


        //------ soft limit ----------//
        public ErrorCode ConfigForwardSoftLimitThreshold(int forwardSensorLimit, int timeoutMs = 0)
        {
            return _ll.ConfigForwardSoftLimit(forwardSensorLimit, timeoutMs);
        }

        public ErrorCode ConfigReverseSoftLimitThreshold(int reverseSensorLimit, int timeoutMs = 0)
        {
            return _ll.ConfigReverseSoftLimit(reverseSensorLimit, timeoutMs);
        }

        public void ConfigForwardSoftLimitEnable(bool enable, int timeoutMs = 0)
        {
            _ll.ConfigForwardSoftLimitEnable(enable, timeoutMs);
        }

        public void ConfigReverseSoftLimitEnable(bool enable, int timeoutMs = 0)
        {
            _ll.ConfigReverseSoftLimitEnable(enable, timeoutMs);
        }

        public void OverrideSoftLimitsEnable(bool enable)
        {
            _ll.OverrideSoftLimitsEnable(enable);
        }

        //------ Current Lim ----------//
        /* not available in base */

        //------ General Close loop ----------//
        public ErrorCode Config_kP(int slotIdx, float value, int timeoutMs = 0)
        {
            return _ll.Config_kP(slotIdx, value, timeoutMs);
        }
        public ErrorCode Config_kI(int slotIdx, float value, int timeoutMs = 0)
        {
            return _ll.Config_kI(slotIdx, value, timeoutMs);
        }
        public ErrorCode Config_kD(int slotIdx, float value, int timeoutMs = 0)
        {
            return _ll.Config_kD(slotIdx, value, timeoutMs);
        }
        public ErrorCode Config_kF(int slotIdx, float value, int timeoutMs = 0)
        {
            return _ll.Config_kF(slotIdx, value, timeoutMs);
        }
        public ErrorCode Config_IntegralZone(int slotIdx, int izone, int timeoutMs = 0)
        {
            return _ll.Config_IntegralZone(slotIdx, izone, timeoutMs);
        }
        public ErrorCode ConfigAllowableClosedloopError(int slotIdx, int allowableCloseLoopError, int timeoutMs = 0)
        {
            return _ll.ConfigAllowableClosedloopError(slotIdx, allowableCloseLoopError, timeoutMs);
        }
        public ErrorCode ConfigMaxIntegralAccumulator(int slotIdx, float iaccum , int timeoutMs = 0)
        {
            return _ll.ConfigMaxIntegralAccumulator(slotIdx, iaccum, timeoutMs);
        }

        /**
	     * Sets the peak closed-loop output.  This peak output is slot-specific and
	     *   is applied to the output of the associated PID loop.
	     * This setting is seperate from the generic Peak Output setting.
	     *
	     * @param slotIdx
	     *            Parameter slot for the constant.
	     * @param percentOut
	     *            Peak Percent Output from 0 to 1.  This value is absolute and
	     *						the magnitude will apply in both forward and reverse directions.
	     * @param timeoutMs
	     *            Timeout value in ms. If nonzero, function will wait for
	     *            config success and report an error if it times out.
	     *            If zero, no blocking or checking is performed.
	     * @return Error Code generated by function. 0 indicates no error.
	     */
        public ErrorCode ConfigClosedLoopPeakOutput(int slotIdx, float percentOut, int timeoutMs)
        {
            return _ll.ConfigClosedLoopPeakOutput(slotIdx, percentOut, timeoutMs);
        }

        /**
	     * Sets the loop time (in milliseconds) of the PID closed-loop calculations.
	     * Default value is 1 ms.
	     *
	     * @param slotIdx
	     *            Parameter slot for the constant.
	     * @param loopTimeMs
	     *            Loop timing of the closed-loop calculations.  Minimum value of
	     *						1 ms, maximum of 64 ms.
	     * @param timeoutMs
	     *            Timeout value in ms. If nonzero, function will wait for
	     *            config success and report an error if it times out.
	     *            If zero, no blocking or checking is performed.
	     * @return Error Code generated by function. 0 indicates no error.
	     */
        public ErrorCode ConfigClosedLoopPeriod(int slotIdx, int loopTimeMs, int timeoutMs)
        {
            return _ll.ConfigClosedLoopPeriod(slotIdx, loopTimeMs, timeoutMs);
        }

        /**
	     * Configures the Polarity of the Auxiliary PID (PID1).
	     *
	     * Standard Polarity:
	     *    Primary Output = PID0 + PID1
	     *    Auxiliary Output = PID0 - PID1
	     *
	     * Inverted Polarity:
	     *    Primary Output = PID0 - PID1
	     *    Auxiliary Output = PID0 + PID1
	     *
	     * @param invert
	     *            If true, use inverted PID1 output polarity.
	     * @param timeoutMs
	     *            Timeout value in ms. If nonzero, function will wait for config
	     *            success and report an error if it times out. If zero, no
	     *            blocking or checking is performed.
	     * @return Error Code
	     */
        public ErrorCode ConfigAuxPIDPolarity(bool invert, int timeoutMs)
        {
            return ConfigSetParameter(ParamEnum.ePIDLoopPolarity, invert ? 1 : 0, 0, 1, timeoutMs);
        }

        public ErrorCode SetIntegralAccumulator(float iaccum = 0, int timeoutMs = 0)
        {
            return _ll.SetIntegralAccumulator(iaccum, timeoutMs);
        }

        public int GetClosedLoopError(int pidIdx)
        {
            int closedLoopError;
            _ll.GetClosedLoopError(out closedLoopError, pidIdx);
            return closedLoopError;
        }
        public float GetIntegralAccumulator(int pidIdx)
        {
            float iaccum;
            _ll.GetIntegralAccumulator(out iaccum, pidIdx);
            return iaccum;
        }
        public float GetErrorDerivative(int pidIdx)
        {
            float derror;
            _ll.GetErrorDerivative(out derror, pidIdx);
            return derror;
        }
        /**
         * SRX has two available slots for PID.
         * @param slotIdx one or zero depending on which slot caller wants.
         */
        public void SelectProfileSlot(int slotIdx, int pidIdx)
        {
            _ll.SelectProfileSlot(slotIdx, pidIdx);
        }

        /**
	     * Gets the current target of a given closed loop.
	     *
	     * @param pidIdx
	     *            0 for Primary closed-loop. 1 for auxiliary closed-loop.
	     * @return The closed loop target.
	     */
        public int GetClosedLoopTarget(int pidIdx)
        {
            int value;
            _ll.GetClosedLoopTarget(out value, pidIdx);
            return value;
        }

        /**
	     * Gets the active trajectory target position using
	     * MotionMagic/MotionProfile control modes.
	     *
	     * @return The Active Trajectory Position in sensor units.
	     */
        public int GetActiveTrajectoryPosition()
        {
            int sensorUnits;
            _ll.GetActiveTrajectoryPosition(out sensorUnits);
            return sensorUnits;
        }

        /**
         * Gets the active trajectory target velocity using
         * MotionMagic/MotionProfile control modes.
         *
         * @return The Active Trajectory Velocity in sensor units per 100ms.
         */
        public int GetActiveTrajectoryVelocity()
        {
            int sensorUnitsPer100Ms;
            _ll.GetActiveTrajectoryVelocity(out sensorUnitsPer100Ms);
            return sensorUnitsPer100Ms;
        }

        /**
         * Gets the active trajectory target heading using
         * MotionMagicArc/MotionProfileArc control modes.
         *
         * @return The Active Trajectory Heading in degreees.
         */
        public double GetActiveTrajectoryHeading()
        {
            double turnUnits;
            _ll.GetActiveTrajectoryHeading(out turnUnits);
            return turnUnits;
        }

        //------ Motion Profile Settings used in Motion Magic and Motion Profile ----------//
        public ErrorCode ConfigMotionCruiseVelocity(int sensorUnitsPer100ms, int timeoutMs = 0)
        {
            return _ll.ConfigMotionCruiseVelocity(sensorUnitsPer100ms, timeoutMs);
        }
        public ErrorCode ConfigMotionAcceleration(int sensorUnitsPer100msPerSec, int timeoutMs = 0)
        {
            return _ll.ConfigMotionAcceleration(sensorUnitsPer100msPerSec, timeoutMs);
        }

        //------ Motion Profile Buffer ----------//
        public void ClearMotionProfileTrajectories()
        {
            _ll.ClearMotionProfileTrajectories();
        }
        public int GetMotionProfileTopLevelBufferCount()
        {
            return _ll.GetMotionProfileTopLevelBufferCount();
        }
        public bool IsMotionProfileTopLevelBufferFull()
        {
            return _ll.IsMotionProfileTopLevelBufferFull();
        }
        public void ProcessMotionProfileBuffer()
        {
            _ll.ProcessMotionProfileBuffer();
        }
        public void GetMotionProfileStatus(Motion.MotionProfileStatus statusToFill)
        {
            _ll.GetMotionProfileStatus(statusToFill);
        }
        public ErrorCode PushMotionProfileTrajectory(Motion.TrajectoryPoint trajPt)
        {
            return _ll.PushMotionProfileTrajectory(trajPt);
        }
        public void ClearMotionProfileHasUnderrun(int timeoutMs = 0)
        {
            _ll.ClearMotionProfileHasUnderrun(timeoutMs);
        }
        /**
	     * Calling application can opt to speed up the handshaking between the robot
	     * API and the controller to increase the download rate of the controller's Motion
	     * Profile. Ideally the period should be no more than half the period of a
	     * trajectory point.
	     *
	     * @param periodMs
	     *            The transmit period in ms.
	     * @return Error Code generated by function. 0 indicates no error.
	     */
        public ErrorCode ChangeMotionControlFramePeriod(int periodMs)
        {
            return _ll.ChangeMotionControlFramePeriod((uint)periodMs);
        }
        /**
	     * When trajectory points are processed in the motion profile executer, the MPE determines
	     * how long to apply the active trajectory point by summing baseTrajDurationMs with the
	     * timeDur of the trajectory point (see TrajectoryPoint).
	     *
	     * This allows general selection of the execution rate of the points with 1ms resolution,
	     * while allowing some degree of change from point to point.
	     * @param baseTrajDurationMs The base duration time of every trajectory point.
	     * 							This is summed with the trajectory points unique timeDur.
	     * @param timeoutMs
	     *            Timeout value in ms. If nonzero, function will wait for
	     *            config success and report an error if it times out.
	     *            If zero, no blocking or checking is performed.
	     * @return Error Code generated by function. 0 indicates no error.
	     */
        public ErrorCode ConfigMotionProfileTrajectoryPeriod(int baseTrajDurationMs, int timeoutMs)
        {
            return _ll.ConfigMotionProfileTrajectoryPeriod(baseTrajDurationMs, timeoutMs);
        }
        //------ error ----------//
        public ErrorCode GetLastError()
        {
            return _ll.GetLastError();
        }

        //------ Faults ----------//
        public ErrorCode GetFaults(Faults toFill)
        {
            return _ll.GetFaults(toFill);
        }
        public ErrorCode GetStickyFaults(Faults toFill)
        {
            return _ll.GetStickyFaults(toFill);
        }
        public ErrorCode ClearStickyFaults()
        {
            return _ll.ClearStickyFaults();
        }

        //------ Firmware ----------//
        public int GetFirmwareVersion()
        {
            return _ll.GetFirmwareVersion();
        }
        public bool HasResetOccured()
        {
            return _ll.HasResetOccured();
        }

        //------ Custom Persistent Params ----------//
        public ErrorCode ConfigSetCustomParam(int newValue, int paramIndex, int timeoutMs = 0)
        {
            return _ll.ConfigSetCustomParam(newValue, paramIndex, timeoutMs);
        }
        public ErrorCode ConfigGetCustomParam(out int readValue, int paramIndex, int timeoutMs = Constants.GetParamTimeoutMs)
        {
            return _ll.ConfigGetCustomParam(out readValue, paramIndex, timeoutMs);
        }

        //------ Generic Param API, typically not used ----------//
        public ErrorCode ConfigSetParameter(ParamEnum param, float value, byte subValue, int ordinal = 0, int timeoutMs = 0)
        {
            return _ll.ConfigSetParameter(param, value, subValue, ordinal, timeoutMs);

        }
        public ErrorCode ConfigGetParameter(ParamEnum param, out float value, int ordinal = 0, int timeoutMs = Constants.GetParamTimeoutMs)
        {
            ErrorCode retval = _ll.ConfigGetParameter(param, out value, ordinal, timeoutMs);

            return retval;
        }
        //------ Misc. ----------//
        public int GetBaseID()
        {
            return _arbId;
        }
        public ControlMode GetControlMode()
        {
            return m_controlMode;
        }
        // ----- Follower ------//
        /**
         * Set the control mode and output value so that this motor controller will
         * follow another motor controller. Currently supports following Victor SPX
         * and Talon SRX.
         *
         * @param masterToFollow
         *						Motor Controller object to follow.
         * @param followerType
         *						Type of following control.  Use AuxOutput1 to follow the master
         *						device's auxiliary output 1.
         *						Use PercentOutput for standard follower mode.
         */
        public void Follow(IMotorController masterToFollow, FollowerType followerType)
        {
            int id32 = masterToFollow.GetBaseID();
            int id24 = id32;
            id24 >>= 16;
            id24 = (short)id24;
            id24 <<= 8;
            id24 |= (id32 & 0xFF);
            Set(ControlMode.Follower, id24);

            switch (followerType)
            {
                case FollowerType.PercentOutput:
                    Set(ControlMode.Follower, (double)id24);
                    break;
                case FollowerType.AuxOutput1:
                    /* follow the motor controller, but set the aux flag
                   * to ensure we follow the processed output */
                    Set(ControlMode.Follower, (double)id24, DemandType.AuxPID, 0);
                    break;
                default:
                    NeutralOutput();
                    break;
            }
        }
        /**
         * Set the control mode and output value so that this motor controller will
         * follow another motor controller. Currently supports following Victor SPX
         * and Talon SRX.
         */
        public void Follow(IMotorController masterToFollow)
        {
            Follow(masterToFollow, FollowerType.PercentOutput);
        }
        public void ValueUpdated()
        {
            //do nothing
        }

        /**
	 * @return object that can get/set individual raw sensor values.
	 */
        public SensorCollection GetSensorCollection()
        {
            return _sensorColl;
        }
    }
}