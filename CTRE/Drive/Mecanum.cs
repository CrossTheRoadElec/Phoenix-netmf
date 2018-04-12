/* 4x basic mecanum */
using System;
using Microsoft.SPOT;
using CTRE.Phoenix.Mechanical;
using CTRE.Phoenix.MotorControl;

namespace CTRE.Phoenix.Drive
{
    public class Mecanum : IDrivetrain
    {
        Gearbox _1; // LF
        Gearbox _2; // LR
        Gearbox _3; // RF
        Gearbox _4; // RR
        Gearbox[] _gearBoxes;

        internal ErrorCodeVarColl _lastError = new ErrorCodeVarColl();

        //------ Access gearbox ----------//
        public Gearbox LeftGearbox { get { return _1; } }
        public Gearbox RightGearbox { get { return _3; } }
        public IMotorController MasterLeftMotorController { get { return _1.MasterMotorController; } }
        public IMotorController MasterRightMotorController { get { return _3.MasterMotorController; } }

        //--------------------- Constructors -----------------------------//
        /** Contstructor that takes 4 SimpleMotorcontrollers */
        public Mecanum(IMotorController m1, IMotorController m2, IMotorController m3, IMotorController m4)
        {
            /* Creat 4 single motor gearboxes */
            Gearbox temp1 = new Gearbox(m1);
            Gearbox temp2 = new Gearbox(m2);
            Gearbox temp3 = new Gearbox(m3);
            Gearbox temp4 = new Gearbox(m4);

            _1 = temp1;
            _2 = temp2;
            _3 = temp3;
            _4 = temp4;


            _gearBoxes = new Gearbox[] { _1, _2, _3, _4 };
        }

        /** Contstructor that takes 4 Gearboxes */
        public Mecanum(Gearbox m1, Gearbox m2, Gearbox m3, Gearbox m4)
        {
            _1 = m1;
            _2 = m2;
            _3 = m3;
            _4 = m4;

            _gearBoxes = new Gearbox[] { _1, _2, _3, _4 };
        }

        //---------------- Set output routines. --------------//

        /**
         * Uses forward, strafe, and turn (Mecanum drive)
         * 
         * @param   forward     Y direction of robot
         * @param   strafe      X direction of robot
         * @param   turn        twist of the robot (arch)
         */
        public void Set(Styles.BasicStyle driveStyle, float forward, float turn, float strafe)
        {
            float leftFrnt_throt = (forward + strafe + turn); // left front moves positive for forward, strafe-right, turn-right
            float leftRear_throt = (forward - strafe + turn); // left rear moves positive for forward, strafe-left, turn-right
            float rghtFrnt_throt = (forward - strafe - turn); // right front moves positive for forward, strafe-left, turn-left
            float rghtRear_throt = (forward + strafe - turn); // right rear moves positive for forward, strafe-right, turn-left
            /* lookup control mode to match caller's selected style */
            ControlMode cm = Styles.Routines.LookupCM(driveStyle);
            /* apply it */
            _1.Set(cm, leftFrnt_throt);
            _2.Set(cm, leftRear_throt);
            _3.Set(cm, rghtFrnt_throt);
            _4.Set(cm, rghtRear_throt);
        }
        /**
         * Uses forward, strafe, and turn (Mecanum drive)
         * 
         * @param   forward     Y direction of robot
         * @param   strafe      X direction of robot
         */
        public void Set(Styles.BasicStyle driveStyle, float forward, float turn)
        {
            Set(driveStyle, forward, turn, 0);
        }
        public void NeutralOutput()
        {
            Set(Styles.BasicStyle.PercentOutput, 0, 0, 0);
        }
        //------ Invert behavior ----------//
        /* TODO: this is done in ctors */


        //----- general output shaping ------------------//
        public ErrorCode ConfigOpenloopRamp(float secondsFromNeutralToFull, int timeoutMs = 0)
        {
            /* clear code(s) */
            _lastError.Clear();
            /* call each GB and save error codes */
            foreach (var gb in _gearBoxes)
            {
                /*for this gearbox */
                var errorCode = gb.ConfigOpenloopRamp(secondsFromNeutralToFull, timeoutMs);
                /* save the error for this GB */
                _lastError.Push(errorCode);
            }
            /* return the first/worst one */
            return _lastError.LastError;
        }
        public ErrorCode ConfigPeakOutput(float forwardPercentOut, float reversePercentOut, int timeoutMs = 0)
        {
            /* clear code(s) */
            _lastError.Clear();
            /* call each GB and save error codes */
            foreach (var gb in _gearBoxes)
            {
                /*for this gearbox */

                var errorCode1 = gb.ConfigPeakOutputForward(forwardPercentOut, timeoutMs);
                /* save the error for this GB */
                _lastError.Push(errorCode1);

                var errorCode2 = gb.ConfigPeakOutputReverse(reversePercentOut, timeoutMs);
                /* save the error for this GB */
                _lastError.Push(errorCode2);
            }
            /* return the first/worst one */
            return _lastError.LastError;
        }

        public ErrorCode ConfigNominalOutput(float forwardPercentOut, float reversePercentOut, int timeoutMs = 0)
        {
            /* clear code(s) */
            _lastError.Clear();
            /* call each GB and save error codes */
            foreach (var gb in _gearBoxes)
            {
                /*for this gearbox */

                var errorCode1 = gb.ConfigNominalOutputForward(forwardPercentOut, timeoutMs);
                /* save the error for this GB */
                _lastError.Push(errorCode1);

                var errorCode2 = gb.ConfigNominalOutputReverse(reversePercentOut, timeoutMs);
                /* save the error for this GB */
                _lastError.Push(errorCode2);
            }
            /* return the first/worst one */
            return _lastError.LastError;
        }

        public ErrorCode ConfigOpenLoopNeutralDeadband(float percentDeadband = Constants.DefaultDeadband, int timeoutMs = 0)
        {
            /* clear code(s) */
            _lastError.Clear();
            /* call each GB and save error codes */
            foreach (var gb in _gearBoxes)
            {
                /*for this gearbox */
                var errorCode = gb.ConfigNeutralDeadband(percentDeadband, timeoutMs);
                /* save the error for this GB */
                _lastError.Push(errorCode);
            }
            /* return the first/worst one */
            return _lastError.LastError;
        }

        public ErrorCode ConfigClosedLoopNeutralDeadband(float percentDeadband = Constants.DefaultDeadband, int timeoutMs = 0)
        {
            /* clear code(s) */
            _lastError.Clear();
            /* call each GB and save error codes */
            foreach (var gb in _gearBoxes)
            {
                /*for this gearbox */
                var errorCode = gb.ConfigNeutralDeadband(percentDeadband, timeoutMs);
                /* save the error for this GB */
                _lastError.Push(errorCode);
            }
            /* return the first/worst one */
            return _lastError.LastError;
        }

        //------ Voltage Compensation ----------//
        public ErrorCode ConfigVoltageCompSaturation(float voltage, int timeoutMs = 0)
        {
            /* clear code(s) */
            _lastError.Clear();
            /* call each GB and save error codes */
            foreach (var gb in _gearBoxes)
            {
                /*for this gearbox */
                var errorCode = gb.ConfigVoltageCompSaturation(voltage, timeoutMs);
                /* save the error for this GB */
                _lastError.Push(errorCode);
            }
            /* return the first/worst one */
            return _lastError.LastError;
        }
        public ErrorCode ConfigVoltageMeasurementFilter(int filterWindowSamples, int timeoutMs = 0)
        {
            /* clear code(s) */
            _lastError.Clear();
            /* call each GB and save error codes */
            foreach (var gb in _gearBoxes)
            {
                /*for this gearbox */
                var errorCode = gb.ConfigVoltageMeasurementFilter(filterWindowSamples, timeoutMs);
                /* save the error for this GB */
                _lastError.Push(errorCode);
            }
            /* return the first/worst one */
            return _lastError.LastError;
        }
        public void EnableVoltageCompensation(bool enable)
        {
            /* call each GB and save error codes */
            foreach (var gb in _gearBoxes)
            {
                /*for this gearbox */
                gb.EnableVoltageCompensation(enable);
            }
        }

        //------ General Status ----------//
        //------ sensor selection ----------//
        /* not applicable */

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