using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.LowLevel
{
    /**
     * Signal enumeration for generic signal access.
     * Although every signal is enumerated, only use this for traffic that must
     * be solicited.
     * Use the auto generated getters/setters at bottom of this header as much as
     * possible.
     */
    public enum ParamEnum
    {

        // SelectedFeedbackDevice
        // SensorPhase
        // open loop RampThrottle
        // FeedbackDevIsContinuous

        StatusFramePeriod = 300,
        OpenloopRamp = 301,
        ClosedloopRamp = 302,
        OpenloopDeadband = 303,
        ClosedloopDeadband = 304,
        PeakPosOutput = 305,
        NominalPosOutput = 306,
        PeakNegOutput = 307,
        NominalNegOutput = 308,

        ProfileParamSlot_P = 310,
        ProfileParamSlot_I = 311,
        ProfileParamSlot_D = 312,
        ProfileParamSlot_F = 313,
        ProfileParamSlot_IZone = 314,
        ProfileParamSlot_AllowableClosedLoopErr = 315,
        ProfileParamSlot_MaxIAccum = 316,

        ClearPositionOnLimitF = 320,
        ClearPositionOnLimitR = 321,

        SampleVelocityPeriod = 323,
        SampleVelocityWindow = 324,

        FeedbackSensorType = 330,
        SelectedSensorPosition = 331,

        ForwardSoftLimitThreshold = 340,
        ReverseSoftLimitThreshold = 341,

        NominalBatteryVoltage = 350,
        BatteryVoltageFilterSize = 351,
        
        ContinuousCurrentLimitAmps = 360,
        ContinuousCurrentLimitMs = 361,
        PeakCurrentLimitAmps = 362,

        ClosedLoopIAccum = 370,

        CustomParam = 380,

        StickyFaults = 390,

        AnalogPosition = 400,
        QuadraturePosition = 401,
        PulseWidthPosition = 402,

#if true
        eProfileParamSlot_P = 2,
        eProfileParamSlot_I = 3,
        eProfileParamSlot_D = 4,
        eProfileParamSlot_F = 5,
        eProfileParamSlot_IZone = 6,
        eProfileParamSlot_AllowableClosedLoopErr = 111,
        eProfileParamSlot1_AllowableClosedLoopErr = 117,

        eProfileParamSoftLimitForThreshold = 21,
        eProfileParamSoftLimitRevThreshold = 22,
        eProfileParamSoftLimitForEnable = 23,
        eProfileParamSoftLimitRevEnable = 24,
        eOnBoot_BrakeMode = 31,
        eOnBoot_LimitSwitch_Forward_NormallyClosed = 32,
        eOnBoot_LimitSwitch_Reverse_NormallyClosed = 33,
        eOnBoot_LimitSwitch_Forward_Disable = 34,
        eOnBoot_LimitSwitch_Reverse_Disable = 35,

        eRevMotDuringCloseLoopEn = 64,
        eRevFeedbackSensor = 68,
   
        eRampThrottle = 67,
        eLimitSwitchEn = 69,
        eLimitSwitchClosedFor = 70,
        eLimitSwitchClosedRev = 71,
        eBrakeIsEnabled = 76,
        eQuadFilterEn = 91,
        eClearPositionOnIdx = 100,

        //ePeakPosOutput = 104,
        //eNominalPosOutput = 105,
        //ePeakNegOutput = 106,
        //eNominalNegOutput = 107,
        eQuadIdxPolarity = 108,
      
        eAllowPosOverflow = 110,
        eNumberPotTurns = 112,
        eNumberEncoderCPR = 113,

        eProfileParamVcompRate = 116,
        
        eMotMag_Accel = 122,
        eMotMag_VelCruise = 123,
        
        eCurrentLimThreshold = 125,
        eCustomParam = 137,
        ePersStorageSaving = 139,

        eClearPositionOnLimitF = 144,
        eClearPositionOnLimitR = 145,
        eNominalBatteryVoltage = 146,
        eSampleVelocityPeriod = 147,
        eSampleVelocityWindow = 148,

        eMotionProfileHasUnderrunErr = 119,
        eEncPosition = 77,
        eSensorPosition = 73,

        ePwdPosition = 114,
        eEncIndexRiseEvents = 79,
        ePidIaccum = 93,
        eCustomParam0 = 137,
        eCustomParam1 = 138,
#endif
    };
}
