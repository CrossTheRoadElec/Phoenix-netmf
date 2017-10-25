using System;
using Microsoft.SPOT;
using CTRE.Phoenix.Mechanical;
using CTRE.Phoenix.MotorControl;

namespace CTRE.Phoenix.Drive
{
    public class SensoredTank : Tank, ISensoredDrivetrain
    {
        RemoteSensoredGearbox _left;
        RemoteSensoredGearbox _right;
        RemoteSensoredGearbox[] _gearBoxes;


        /** Encoder heading properties */
        public float DistanceBetweenWheels { get; set; }
        public uint ticksPerRev { get; set; }
        public float wheelRadius { get; set; }
        float ScrubCoefficient = 1;   /* Not sure if I should make this setable */

        /* Sensored Tank constructor (uses two gearboxes)*/
        public SensoredTank(RemoteSensoredGearbox left, RemoteSensoredGearbox right, bool leftInverted, bool rightInverted, float wheelRadius) : base(left, right, leftInverted, rightInverted)
        {
            _left = left;
            _right = right;
            _gearBoxes = new RemoteSensoredGearbox[] { _left, _right };

            if (wheelRadius < 0.01)
                Debug.Print("CTR: Wheel radius must be greater than 0.01");
            this.wheelRadius = wheelRadius;
        }
        public SensoredTank(SensoredGearbox left, SensoredGearbox right, bool leftInverted, bool rightInverted, float wheelRadius)
          : this((RemoteSensoredGearbox)left, (RemoteSensoredGearbox)right, leftInverted, rightInverted, wheelRadius)
        {
        }

        //------ Access motor controller  ----------//

        //------ Set output routines. ----------//
        public void Set(Styles.AdvancedStyle driveStyle, float forward, float turn)
        {
            /* calc the left and right demand */
            float l, r;
            Util.Split_1(forward, turn, out l, out r);
            /* lookup control mode to match caller's selected style */
            ControlMode cm = Styles.Routines.LookupCM(driveStyle);
            /* apply it */
            _left.Set(cm, l);
            _right.Set(cm, r);
        }
        //------ Invert behavior ----------//
        /* this is done in ctors */

        //----- general output shaping ------------------//
        public ErrorCode ConfigClosedloopRamp(float secondsFromNeutralToFull, int timeoutMs = 0)
        {
            /* clear code(s) */
            _lastError.Clear();
            /* call each GB and save error codes */
            foreach (var gb in _gearBoxes)
            {
                /*for this gearbox */
                var errorCode = gb.ConfigClosedloopRamp(secondsFromNeutralToFull, timeoutMs);
                /* save the error for this GB */
                _lastError.Push(errorCode);
            }
            /* return the first/worst one */
            return _lastError.LastError;
        }

        //------ Voltage Compensation ----------//
        /* in parent */

        //------ General Status ----------//
        /* not applicable */

        //------ sensor selection ----------//
        /* done in c'tor */

        //------- sensor status --------- //
        public ErrorCode SetPosition(float sensorPos, int timeoutMs = 0)
        {
            /* clear code(s) */
            _lastError.Clear();
            /* call each GB and save error codes */
            foreach (var gb in _gearBoxes)
            {
                /*for this gearbox */
                var errorCode = gb.SetPosition(sensorPos, timeoutMs);
                /* save the error for this GB */
                _lastError.Push(errorCode);
            }
            /* return the first/worst one */
            return _lastError.LastError;
        }
        public float GetPosition()
        {
            float l = _left.GetPosition();
            float r = _right.GetPosition();
            return (l + r) * 0.5f;
        }
        public float GetVelocity()
        {
            float l = _left.GetVelocity();
            float r = _right.GetVelocity();

            return (l + r) * 0.5f;
        }

        public float GetSensorDerivedAngle()
        {
            float l = _left.GetPosition();
            float r = _right.GetPosition();

            if (wheelRadius < 0.01)
            {
                Debug.Print("CTR: Sensored Tank has too small of a wheel radius, cannot get heading");
                return 0;
            }

            if (ticksPerRev == 0)
            {
                Debug.Print("CTR: Sensored Tank has not set ticks per wheel revolution, cannot get heading");
                return 0;
            }

            if (DistanceBetweenWheels < 0.01)
            {
                Debug.Print("CTR: Sensored Tank has too small of a distance between wheels, cannot get heading");
                return 0;
            }

            float unitsPerTick = (float)(2 * System.Math.PI * wheelRadius) / ticksPerRev;
            float theta = ((r - l) / (DistanceBetweenWheels / unitsPerTick) * (float)(180 / System.Math.PI)) * ScrubCoefficient;

            return theta;
        }
        public float GetSensorDerivedAngularVelocity()
        {
            float l = _left.GetVelocity();
            float r = _right.GetVelocity();

            if (wheelRadius < 0.01)
            {
                Debug.Print("CTR: Sensored Tank has too small of a wheel radius, cannot get heading");
                return 0;
            }

            if (ticksPerRev == 0)
            {
                Debug.Print("CTR: Sensored Tank has not set ticks per wheel revolution, cannot get heading");
                return 0;
            }

            if (DistanceBetweenWheels < 0.01)
            {
                Debug.Print("CTR: Sensored Tank has too small of a distance between wheels, cannot get heading");
                return 0;
            }

            float unitsPerTick = (float)(2 * System.Math.PI * wheelRadius) / ticksPerRev;
            float theta = ((r - l) / (DistanceBetweenWheels / unitsPerTick) * (float)(180 / System.Math.PI)) * ScrubCoefficient;

            return theta;
        }


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

        ////------ Motion Profile Buffer ----------//
        //void ClearMotionProfileTrajectories();
        //ErrorCode GetMotionProfileTopLevelBufferCount();
        //ErrorCode PushMotionProfileTrajectory(Motion.TrajectoryPoint trajPt);
        //bool IsMotionProfileTopLevelBufferFull();
    }
}
