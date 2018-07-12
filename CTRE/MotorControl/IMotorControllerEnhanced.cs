using System;
using Microsoft.SPOT;

/*
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
     * Advanced Motor Controller for CTRE ESCs with advanced features (Talon SRX with 2018 firmware).
     */
    public interface IMotorControllerEnhanced : IMotorController
    {
        //------ Set output routines. ----------//
        /* in parent */

        //------ Invert behavior ----------//
        /* in parent */

        //----- general output shaping ------------------//
        /* in parent */

        //------ Voltage Compensation ----------//
        /* in parent */

        //------ General Status ----------//
        /* in parent */

        //------ sensor selection ----------//
        /* expand the options */
        ErrorCode ConfigSelectedFeedbackSensor(FeedbackDevice feedbackDevice, int pidIdx, int timeoutMs = 0);

        //------ ??? ----------//
        //ErrorCode ConfigSensorIsContinuous(bool isContinuous, int timeoutMs = 0);  /* TODO: figure this out later */
        //ErrorCode ConfigAutoZeroSensor(ZeroSensorCriteria zeroSensorCriteria, int timeoutMs = 0);

        //------- sensor status --------- //
        /* in parent */

        //------ status frame period changes ----------//
        ErrorCode SetStatusFramePeriod(StatusFrameEnhanced frame, int periodMs, int timeoutMs = 0);
        ErrorCode GetStatusFramePeriod(StatusFrameEnhanced frame, out int periodMs, int timeoutMs = 0);

        //----- velocity signal conditionaing ------//
        ErrorCode ConfigVelocityMeasurementPeriod(VelocityMeasPeriod period, int timeoutMs = 0);
        ErrorCode ConfigVelocityMeasurementWindow(int windowSize, int timeoutMs = 0);

        //------ remote limit switch ----------//
        /* in parent */

        //------ local limit switch ----------//
        ErrorCode ConfigForwardLimitSwitchSource(LimitSwitchSource type, LimitSwitchNormal normalOpenOrClose, int timeoutMs = 0);
        ErrorCode ConfigReverseLimitSwitchSource(LimitSwitchSource type, LimitSwitchNormal normalOpenOrClose, int timeoutMs = 0);

        //------ soft limit ----------//
        /* in parent */

        //------ Current Lim ----------//
        ErrorCode ConfigPeakCurrentLimit(int amps, int timeoutMs = 0);
        ErrorCode ConfigPeakCurrentDuration(int milliseconds, int timeoutMs = 0);
        ErrorCode ConfigContinuousCurrentLimit(int amps, int timeoutMs = 0);
        void EnableCurrentLimit(bool enable);

        //------ General Close loop ----------//
        /* in parent */

        //------ Motion Profile Settings used in Motion Magic and Motion Profile ----------//
        /* in parent */

        //------ Motion Profile Buffer ----------//
        /* in parent */

        //------ error ----------//
        /* in parent */

        //------ Faults ----------//
        /* in parent */

        //------ Firmware ----------//
        /* in parent */

        //------ Custom Persistent Params ----------//
        /* in parent */

        //------ Generic Param API, typically not used ----------//
        /* in parent */

        //------ Misc. ----------//
        /* in parent */

        //------ RAW Sensor API ----------//
        /**
         * @retrieve object that can get/set individual RAW sensor values.
         */
        SensorCollection GetSensorCollection();
    }
}
