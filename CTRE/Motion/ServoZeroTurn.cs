//using System;
//using Microsoft.SPOT;
//using CTRE.Drive;
//using CTRE.Tasking;
//using CTRE.MotorControllers;

//namespace CTRE.Motion
//{
//    public class ServoZeroTurn : ILoopable
//    {
//        ISmartDrivetrain _driveTrain;
//        Styles.Smart _selectedStyle;
//        ServoParameters _servoParams = new ServoParameters();
//        Stopwatch _myStopwatch = new Stopwatch();

//        float _targetHeading;
//        float _headingTolerance;
//        float _previousHeading;
//        float _timeElapsed;
//        float _maxOutput;

//        bool _isRunning = false;
//        bool _isDone = false;
//        byte _isGood = 0;
//        byte _state = 0;

//        /* Servo parameters */
//        public ServoParameters ServoParameters
//        {
//            get { return _servoParams; }
//        }

//        /** Constructor */
//        public ServoZeroTurn(ISmartDrivetrain driveTrain, Styles.Smart smartStyle, float targetHeading, float headingTolerance, ServoParameters Params, float maxOutput)
//        {
//            _driveTrain = driveTrain;
//            _selectedStyle = smartStyle;

//            _targetHeading = targetHeading;
//            _headingTolerance = headingTolerance;

//            _maxOutput = maxOutput;

//            _servoParams = Params;
//        }

//        public ServoZeroTurn(ISmartDrivetrain driveTrain, Styles.Smart smartStyle)
//        {
//            _selectedStyle = smartStyle;
//            _driveTrain = driveTrain;
//        }

//        public bool Set(float targetHeading, float headingTolerance, float maxOutput)
//        {
//            _maxOutput = maxOutput;
//            return ZeroTurn(targetHeading, headingTolerance);
//        }

//        public float GetEncoderHeading()
//        {
//            return _driveTrain.GetEncoderHeading();
//        }

//        private bool ZeroTurn(float targetHeading, float headingTolerance)
//        {
//            if (_servoParams.P == 0 && _servoParams.I == 0 && _servoParams.D == 0)
//                Debug.Print("CTR: Servo Zero Turn has no PID values, cannot turn");
//            /* Grab the current heading*/
//            float currentHeading = GetEncoderHeading();

//            /* Find the difference between last heading and current heading */
//            _timeElapsed = _myStopwatch.Duration;
//            float headingRate = currentHeading - _previousHeading;
//            _myStopwatch.Start();

//            /* Heading PID */
//            float headingError = targetHeading - currentHeading;
//            float X = (headingError) * _servoParams.P - (headingRate) * _servoParams.D;
//            X = -X;
//            X = CTRE.Util.Cap(X, _maxOutput);


//            /** Set the output of the drivetrain */
//            /** Set the output of the drivetrain */
//            switch (_selectedStyle)
//            {
//                case Styles.Smart.PercentOutput:
//                    _driveTrain.Set(Styles.Smart.PercentOutput, 0, X);
//                    break;
//                case Styles.Smart.Voltage:
//                    _driveTrain.Set(Styles.Smart.Voltage, 0, X);
//                    break;
//                case Styles.Smart.VelocityClosedLoop:
//                    _driveTrain.Set(Styles.Smart.VelocityClosedLoop, 0, X);
//                    break;
//            }

//            /** Grab the heading to compare next time */
//            _previousHeading = currentHeading;

//            if (System.Math.Abs(headingError) >= headingTolerance )
//            {
//                _isRunning = true;
//            }
//            else
//            {
//                _isRunning = false;
//            }
//            return _isRunning;
//        }

//        /** ILoopable */
//        public void OnStart()
//        {
//            _isDone = false;
//            _isGood = 0;
//            _state = 0;
//        }

//        public void OnStop()
//        {
//            _driveTrain.Set(Styles.Basic.PercentOutput, 0, 0);
//            _isDone = true;
//        }

//        public bool IsDone()
//        {
//            return _isDone;
//        }

//        public void OnLoop()
//        {
//            switch (_state)
//            {
//                case 0:
//                    _driveTrain.SetPosition(0.0f);
//                    _state = 1;
//                    break;
//                case 1:
//                    bool running = ZeroTurn(_targetHeading, _headingTolerance);

//                    if (running == true)
//                        _isGood = 0;
//                    else if (_isGood < 10)
//                        ++_isGood;
//                    else
//                    {
//                        _driveTrain.Set(Styles.Basic.PercentOutput, 0, 0);
//                        _isDone = true;
//                    }
//                    break;
//            }
//        }
//    }
//}