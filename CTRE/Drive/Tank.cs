using System;
using Microsoft.SPOT;
using CTRE.Phoenix.Mechanical;
using CTRE.Phoenix.MotorControl;

namespace CTRE.Phoenix.Drive
{
    public class Tank : IDrivetrain
    {
        Gearbox _left;
        Gearbox _right;
        Gearbox[] _gearBoxes;

        internal ErrorCodeVarColl _lastError = new ErrorCodeVarColl();

        //------ Access gearbox ----------//
        public Gearbox LeftGearbox { get { return _left; } }
        public Gearbox RightGearbox { get { return _right; } }
        public IMotorController MasterLeftMotorController { get { return _left.MasterMotorController; } }
        public IMotorController MasterRightMotorController { get { return _right.MasterMotorController; } }

        //--------------------- Constructors -----------------------------//

        /** Tank Drive constructor that takes a left gearbox, right gearbox, and side inverted */
        public Tank(Gearbox left, Gearbox right, bool leftInvert, bool rightInvert)
        {
            _left = left;
            _right = right;

            _left.SetInverted(leftInvert);
            _right.SetInverted(rightInvert);

            _gearBoxes = new Gearbox[] { _left, _right };
        }

        public Tank(IMotorController m1, IMotorController m2, bool leftInvert, bool rightInvert)
        {
            /* Create 2 single motor gearboxes */
            Gearbox temp1 = new Gearbox(m1);
            Gearbox temp2 = new Gearbox(m2);

            _left = temp1;
            _right = temp2;

            _left.SetInverted(leftInvert);
            _right.SetInverted(rightInvert);

            _gearBoxes = new Gearbox[] { _left, _right };
        }

        //------ Set output routines. ----------//
        public void Set(Styles.BasicStyle driveStyle, float forward, float turn)
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
        public void NeutralOutput()
        {
            Set(Styles.BasicStyle.PercentOutput, 0, 0);
        }

        //------ Invert behavior ----------//
        /* this is done in ctors */


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
