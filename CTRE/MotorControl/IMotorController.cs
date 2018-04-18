using System;
using Microsoft.SPOT;
using CTRE.Phoenix.LowLevel;

/**
 * @param PhaseSensor 		Select true or false so that sensor is positive 
 *								when Motor Controller LED is green. Dial this in first
 *								regardless of desired direction.
 * @param DesiredDirection 	Select true or false to control what is considered "positive".
 *								An example would be to flip this param until positive
 *								output causes robot to travel forward (not reverse).
 *								This also affects the Talon LED so green becomes forward
 *								on both sides of the drivetrain.
 * @param timeoutMs		Pass nonzero value and check the
 *						the return code to determine if Talon is on the bus.
 *						This is recommended when initializing (on power boot, etc.)
 *						Pass zero to skip this check. This is recommended when 
 *						changing settings inside the normal execution loop 
 *						of the robot, where blocking should be avoided.
 * @return Last Error Code		 @see GetLastError for details.
 */

namespace CTRE.Phoenix.MotorControl
{
    /**
     * Generic Motor Controller for all CTRE ESCs (Cyclone and Talon SRX).
     */
    public interface IMotorController : IFollower //: Signals.IOutputSignal , IFollower, IInvertable, ICANAddressable, IHasRemoteLimitSwitches
    {
        //------ Set output routines. ----------//
        void Set(ControlMode Mode, double demand);
        void Set(ControlMode mode, double demand0, DemandType demand1Type, double demand1);
        void NeutralOutput();
        void SetNeutralMode(NeutralMode neutralMode);

        //------ Invert behavior ----------//
        void SetSensorPhase(bool PhaseSensor);
        void SetInverted(bool invert);
        bool GetInverted();

        //----- general output shaping ------------------//
        ErrorCode ConfigOpenloopRamp(float secondsFromNeutralToFull, int timeoutMs = 0);
        ErrorCode ConfigClosedloopRamp(float secondsFromNeutralToFull, int timeoutMs = 0);
        ErrorCode ConfigPeakOutputForward(float percentOut, int timeoutMs = 0);
        ErrorCode ConfigPeakOutputReverse(float percentOut, int timeoutMs = 0);
        ErrorCode ConfigNominalOutputForward(float percentOut, int timeoutMs = 0);
        ErrorCode ConfigNominalOutputReverse(float percentOut, int timeoutMs = 0);
        ErrorCode ConfigNeutralDeadband(float percentDeadband = Constants.DefaultDeadband, int timeoutMs = 0);

        //------ Voltage Compensation ----------//
        ErrorCode ConfigVoltageCompSaturation(float voltage, int timeoutMs = 0);
        ErrorCode ConfigVoltageMeasurementFilter(int filterWindowSamples, int timeoutMs = 0);
        void EnableVoltageCompensation(bool enable);

        //------ General Status ----------//
        ErrorCode GetBusVoltage(out float param);
        ErrorCode GetMotorOutputPercent(out float param);
        ErrorCode GetMotorOutputVoltage(out float param);
        ErrorCode GetOutputCurrent(out float param);
        ErrorCode GetTemperature(out float param);

        //------ sensor selection ----------//
        ErrorCode ConfigSelectedFeedbackSensor(RemoteFeedbackDevice feedbackDevice, int pidIdx, int timeoutMs = 0);

        //------- sensor status --------- //
        int GetSelectedSensorPosition(int pidIdx);
        int GetSelectedSensorVelocity(int pidIdx);
        ErrorCode SetSelectedSensorPosition(int sensorPos, int pidIdx, int timeoutMs = 0);

        //------ status frame period changes ----------//
        ErrorCode SetControlFramePeriod(ControlFrame frame, int periodMs);
        ErrorCode SetStatusFramePeriod(StatusFrame frame, int periodMs, int timeoutMs = 0);
        ErrorCode GetStatusFramePeriod(StatusFrame frame, out int periodMs, int timeoutMs = 0);

        //----- velocity signal conditionaing ------//
        /* not supported */


        //------ remote limit switch ----------//
        ErrorCode ConfigForwardLimitSwitchSource(RemoteLimitSwitchSource type, LimitSwitchNormal normalOpenOrClose, int deviceID, int timeoutMs = 0);
        ErrorCode ConfigReverseLimitSwitchSource(RemoteLimitSwitchSource type, LimitSwitchNormal normalOpenOrClose, int deviceID, int timeoutMs = 0);
        void OverrideLimitSwitchesEnable(bool enable);

        //------ local limit switch ----------//
        /* not supported */

        //------ soft limit ----------//
        ErrorCode ConfigForwardSoftLimitThreshold(int forwardSensorLimit, int timeoutMs = 0);
        ErrorCode ConfigReverseSoftLimitThreshold(int reverseSensorLimit, int timeoutMs = 0);


        void OverrideSoftLimitsEnable(bool enable);

        //------ Current Lim ----------//
		/* not supported */

        //------ General Close loop ----------//
        ErrorCode Config_kP(int slotIdx, float value, int timeoutMs = 0);
        ErrorCode Config_kI(int slotIdx, float value, int timeoutMs = 0);
        ErrorCode Config_kD(int slotIdx, float value, int timeoutMs = 0);
        ErrorCode Config_kF(int slotIdx, float value, int timeoutMs = 0);
        ErrorCode Config_IntegralZone(int slotIdx, int izone, int timeoutMs = 0);
        ErrorCode ConfigAllowableClosedloopError(int slotIdx, int allowableCloseLoopError, int timeoutMs = 0);
        ErrorCode ConfigMaxIntegralAccumulator(int slotIdx, float iaccum, int timeoutMs = 0);

        ErrorCode SetIntegralAccumulator(float iaccum = 0, int timeoutMs = 0);

        ErrorCode ConfigClosedLoopPeakOutput(int slotIdx, float percentOut, int timeoutMs);
        ErrorCode ConfigClosedLoopPeriod(int slotIdx, int loopTimeMs, int timeoutMs);
        ErrorCode ConfigAuxPIDPolarity(bool invert, int timeoutMs);

        int GetClosedLoopError(int pidIdx);
        float GetIntegralAccumulator(int pidIdx);
        float GetErrorDerivative(int pidIdx);

        void SelectProfileSlot(int slotIdx, int pidIdx);

        //------ Motion Profile Settings used in Motion Magic and Motion Profile ----------//
        ErrorCode ConfigMotionCruiseVelocity(int sensorUnitsPer100ms, int timeoutMs = 0);
        ErrorCode ConfigMotionAcceleration(int sensorUnitsPer100msPerSec, int timeoutMs = 0);

        //------ Motion Profile Buffer ----------//
        void ClearMotionProfileTrajectories();
        int GetMotionProfileTopLevelBufferCount();
        ErrorCode PushMotionProfileTrajectory(Motion.TrajectoryPoint trajPt);
        bool IsMotionProfileTopLevelBufferFull();
        void ProcessMotionProfileBuffer();
        void GetMotionProfileStatus(Motion.MotionProfileStatus statusToFill);
        void ClearMotionProfileHasUnderrun(int timeoutMs = 0);

        //------ error ----------//
        ErrorCode GetLastError();

        //------ Faults ----------//
        ErrorCode GetFaults(Faults toFill);
        ErrorCode GetStickyFaults(Faults toFill);
        ErrorCode ClearStickyFaults();

        //------ Firmware ----------//
        int GetFirmwareVersion();
        bool HasResetOccured();

        //------ Custom Persistent Params ----------//
        ErrorCode ConfigSetCustomParam(int newValue, int paramIndex, int timeoutMs = 0);
        ErrorCode ConfigGetCustomParam(out int readValue, int paramIndex, int timeoutMs = Constants.GetParamTimeoutMs);

        //------ Generic Param API, typically not used ----------//
        ErrorCode ConfigSetParameter(ParamEnum param, float value, byte subValue, int ordinal = 0, int timeoutMs = 0);
        ErrorCode ConfigGetParameter(ParamEnum paramEnum, out float value, int ordinal = 0, int timeoutMs = Constants.GetParamTimeoutMs);


        //------ Misc. ----------//
        int GetBaseID();

        // ----- Follower ------//
        /* in parent interface */
    }
}
