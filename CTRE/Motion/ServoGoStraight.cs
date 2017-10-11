//software pid encoder-derived heading => talon (VelClosedLoop, PercentOutput, Voltage).
using System;
using Microsoft.SPOT;
using CTRE.Phoenix.Drive;
using CTRE.Phoenix.Tasking;
using CTRE.Phoenix.MotorControllers;

namespace CTRE.Phoenix.Motion
{
    public class ServoGoStraight  : ILoopable
    {
        ISmartDrivetrain _driveTrain;
        Styles.Smart _selectedStyle;
        ServoParameters _servoParams = new ServoParameters();
        Stopwatch _myStopWatch = new Stopwatch();

        float _Y;
        float _targetHeading;
        float _headingTolerance;
        float _previousHeading;
        float _timeElapsed;
        float _maxOutput;

        bool _isRunning = false;
        bool _isDone = false;
        byte _state = 0;

        /* Servo parameters */
        public ServoParameters ServoParameters
        {
            get { return _servoParams; }
        }

        /** Used whenever a new instance is created, AutonManuevers */
        public ServoGoStraight(ISmartDrivetrain driveTrain, Styles.Smart selectedStyle, ServoParameters Params, float Y, float targetHeading, float headingTolerance, float maxOutput)
        {
            _driveTrain = driveTrain;
            _selectedStyle = selectedStyle;

            //=====================================//
            _Y = Y;
            _targetHeading = targetHeading;
            _headingTolerance = headingTolerance;

            _servoParams = Params;

            _maxOutput = maxOutput;
            //=====================================//
        }

        /** Used whenever a new instance is created, AutonManuevers */
        public ServoGoStraight(ISmartDrivetrain driveTrain, Styles.Smart selectedStyle)
        {
            _driveTrain = driveTrain;
            _selectedStyle = selectedStyle;
        }

        public bool Set(float Y , float targetHeading, float  headingTolerance, float maxOutput)
        {
            _maxOutput = maxOutput;
            return GoStraight(Y, targetHeading, headingTolerance);
        }

        /** Returns the encoder heading from the drivetrain */
        public float GetEncoderHeading()
        {
            return _driveTrain.GetEncoderHeading();
        }

        private bool GoStraight(float Y, float targetHeading, float headingTolerance)
        {
            if (_servoParams.P == 0 && _servoParams.I == 0 && _servoParams.D == 0)
                Debug.Print("CTR: Servo Go Straight has no PID values, cannot go straight");
            /* Grab encoder heading */
            float currentHeading = GetEncoderHeading();

            /* Find angular rate from the encoders */
            _timeElapsed = _myStopWatch.Duration;
            float correctionRate = ((currentHeading - _previousHeading) / _timeElapsed);
            _myStopWatch.Start();

            /* Heading PID */
            float headingError = targetHeading - currentHeading;
            float X = (headingError) * _servoParams.P - (correctionRate) * _servoParams.D;
            X = Util.Cap(X, _maxOutput);
            X = -X;

            /* Select control mode based on selected style */
            switch (_selectedStyle)
            {
                case Styles.Smart.PercentOutput:
                    _driveTrain.Set(Styles.Smart.PercentOutput, Y, X);
                    break;
                case Styles.Smart.Voltage:
                    _driveTrain.ConfigNominalPercentOutputVoltage(+0.0f, -0.0f);
                    _driveTrain.ConfigPeakPercentOutputVoltage(+_maxOutput, -_maxOutput);
                    _driveTrain.Set(Styles.Smart.Voltage, Y, X);
                    break;
                case Styles.Smart.VelocityClosedLoop:
                    _driveTrain.Set(Styles.Smart.VelocityClosedLoop, Y, X);
                    break;
            }
            _previousHeading = currentHeading;

            if (System.Math.Abs(headingError) >= headingTolerance)
            {
                _isRunning = true;
            }
            else
            {
                _isRunning = false;
            }
            return _isRunning;
        }

        /** ILoopable */
        public void OnStart()
        {
            _isDone = false;
            _state = 0;
        }

        public void OnStop()
        {
            _driveTrain.Set(Styles.Basic.PercentOutput, 0, 0);
            _isRunning = false;
            _isDone = true;
        }

        public bool IsDone()
        {
            return _isDone;
        }

        public void OnLoop()
        {
            switch (_state)
            {
                case 0:
                    _driveTrain.SetPosition(0.0f);
                    break;
                case 1:
                    GoStraight(_Y, _targetHeading, _headingTolerance);
                    break;
            }
        }
    }
}