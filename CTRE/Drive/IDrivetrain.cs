using System;
using Microsoft.SPOT;
using CTRE.Phoenix.MotorControl;

namespace CTRE.Phoenix.Drive
{
    public interface IDrivetrain
    {
        //------ Access motor controller  ----------//
        IMotorController MasterLeftMotorController { get; }
        IMotorController MasterRightMotorController { get; }
        //------ Set output routines. ----------//
        void Set(Styles.BasicStyle style, float forward, float turn);
        void NeutralOutput();

        //------ Invert behavior ----------//
        /* this is done in ctors */

        //----- general output shaping ------------------//
        ErrorCode ConfigOpenloopRamp(float secondsFromNeutralToFull, int timeoutMs = 0);
        ErrorCode ConfigPeakOutput(float forwardPercentOut, float reversePercentOut, int timeoutMs = 0);
        ErrorCode ConfigNominalOutput(float forwardPercentOut, float reversePercentOut, int timeoutMs = 0);
        ErrorCode ConfigOpenLoopNeutralDeadband(float percentDeadband = Constants.DefaultDeadband, int timeoutMs = 0);
        ErrorCode ConfigClosedLoopNeutralDeadband(float percentDeadband = 0, int timeoutMs = 0);

        //------ Voltage Compensation ----------//
        ErrorCode ConfigVoltageCompSaturation(float voltage, int timeoutMs = 0);
        ErrorCode ConfigVoltageMeasurementFilter(int filterWindowSamples, int timeoutMs = 0);
        void EnableVoltageCompensation(bool enable);

        //------ General Status ----------//
        /* not applicable */

        //------ sensor selection ----------//
        /* not sensored */

        //------- sensor status --------- //
        /* not sensored */

        //----- velocity signal conditionaing ------//
        /* not sensored */

        //------ remote limit switch ----------//
        //------ local limit switch ----------//
        //------ soft limit ----------//
        /* not applicable */

        //------ Current Lim ----------//
        /* not applicable */

        //------ General Close loop ----------//
        /* caller can get the masters */

        //------ Motion Profile Settings used in Motion Magic and Motion Profile ----------//
        /* not sensored */

        //------ Motion Profile Buffer ----------//
        /* not sensored */
    }
}
