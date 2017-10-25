using System;
using Microsoft.SPOT;
using CTRE.Phoenix.Motion;
using CTRE.Phoenix.MotorControl;

namespace CTRE.Phoenix.Drive
{
    public interface ISensoredDrivetrain : IDrivetrain
    {
        //------ Access motor controller  ----------//
        //new IMotorControllerEnhanced MasterLeftMotorController { get; } 
        //new IMotorControllerEnhanced MasterRightMotorController { get; }

        //------ Set output routines. ----------//
        void Set(Styles.AdvancedStyle style, float forward, float turn);
		/* rest in parent */

        //------ Invert behavior ----------//
        /* this is done in ctors */

        //----- general output shaping ------------------//
        ErrorCode ConfigClosedloopRamp(float secondsFromNeutralToFull, int timeoutMs = 0);

        //------ Voltage Compensation ----------//
        /* in parent */

        //------ General Status ----------//
        /* not applicable */

        //------ sensor selection ----------//
        /* done in c'tor */

        //------- sensor status --------- //
        ErrorCode SetPosition(float sensorPos, int timeoutMs = 0);
        float GetPosition();
        float GetVelocity();
        float GetSensorDerivedAngle();
        float GetSensorDerivedAngularVelocity();

        //----- velocity signal conditionaing ------//
        /* not applicable for now */

        //------ remote limit switch ----------//
        //------ local limit switch ----------//
        //------ soft limit ----------//
        /* not applicable */

        //------ Current Lim ----------//
        /* not required, subclasses will do this */

        //------ General Close loop ----------//
        /* caller can get the masters */

        //------ Motion Profile Settings used in Motion Magic and Motion Profile ----------//
		/* TODO, how to handle left vs right */
        //ErrorCode SetMotionCruiseVelocity(int sensorUnitsPer100ms, int timeoutMs = 0);
        //ErrorCode SetMotionAcceleration(int sensorUnitsPer100msPerSec, int timeoutMs = 0);

        //------ Motion Profile Buffer ----------//
        //void ClearMotionProfileTrajectories();
        //ErrorCode GetMotionProfileTopLevelBufferCount();
        //ErrorCode PushMotionProfileTrajectory(Motion.TrajectoryPoint trajPt);
        //bool IsMotionProfileTopLevelBufferFull();
		///* in parent */
    }
}
