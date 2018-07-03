////software pid Distanceservo then...
//// ServoGoStraight: software pid encoder-derived heading => talon (VelClosedLoop, PercentOutput, VoltageComp).
//using System;
//using Microsoft.SPOT;
//using CTRE.Phoenix.Drive;
//using CTRE.Phoenix.Tasking;
//using CTRE.Phoenix.MotorControllers;

//namespace CTRE.Phoenix.Motion
//{
//    public class ServoStraightDistance : ILoopable
//    {
//        ISmartDrivetrain _driveTrain;
//        Styles.Smart _selectedStyle;
//        ServoParameters _straightServoParams = new ServoParameters();
//        ServoParameters _distanceServoParams = new ServoParameters();
//        ServoGoStraight StraightDrive;
//        Stopwatch _myStopWatch = new Stopwatch();

//        float _targetDistance;
//        float _targetHeading;
//        float _distanceTolerance;
//        float _headingTolerance;
//        float _previousDistance;
//        float _timeElapsed;
//        float _maxOutput;

//        bool _isRunning = false;
//        bool _isDone = false;
//        byte _state = 0;
//        byte _isGood = 0;

//        /** Servo Parameters */
//        public ServoParameters StraightServoParameters
//        {
//            get { return _straightServoParams; }
//        }
//        public ServoParameters DistanceServoParameters
//        {
//            get { return _distanceServoParams; }
//        }

//        /** Straight servo constuctor that takes a smart drive train */
//        public ServoStraightDistance(ISmartDrivetrain driveTrain, Styles.Smart selectedStyle, ServoParameters turnParams, ServoParameters distanceParams, float targetHeading, float targetDistance, float headingTolerance, float distanceTolerance, float maxOutput)
//        {
//            _driveTrain = driveTrain;
//            _selectedStyle = selectedStyle;

//            /* Construct a ServoGoStraight based on sytle selected */
//            if (_selectedStyle == Styles.Smart.Voltage)
//                StraightDrive = new ServoGoStraight(_driveTrain, Styles.Smart.Voltage);
//            else if (_selectedStyle == Styles.Smart.PercentOutput)
//                StraightDrive = new ServoGoStraight(_driveTrain, Styles.Smart.PercentOutput);
//            else if(_selectedStyle == Styles.Smart.VelocityClosedLoop)
//                StraightDrive = new ServoGoStraight(_driveTrain, Styles.Smart.VelocityClosedLoop);

//            //================================================//
//            _targetHeading = targetHeading;
//            _targetDistance = targetDistance;
//            _headingTolerance = headingTolerance;
//            _distanceTolerance = distanceTolerance;

//            _distanceServoParams = distanceParams;
//            StraightDrive.ServoParameters.P = turnParams.P;
//            StraightDrive.ServoParameters.I = turnParams.I;
//            StraightDrive.ServoParameters.D = turnParams.D;

//            _maxOutput = maxOutput;
//            //================================================//
//        }

//        /** Straight servo constuctor that takes a smart drive train */
//        public ServoStraightDistance(ISmartDrivetrain driveTrain, Styles.Smart selectedStyle)
//        {
//            _driveTrain = driveTrain;
//            _selectedStyle = selectedStyle;

//            /* Construct a ServoGoStraight based on sytle selected */
//            if (_selectedStyle == Styles.Smart.Voltage)
//                StraightDrive = new ServoGoStraight(_driveTrain, Styles.Smart.Voltage);
//            else if (_selectedStyle == Styles.Smart.PercentOutput)
//                StraightDrive = new ServoGoStraight(_driveTrain, Styles.Smart.PercentOutput);
//            else if (_selectedStyle == Styles.Smart.VelocityClosedLoop)
//                StraightDrive = new ServoGoStraight(_driveTrain, Styles.Smart.VelocityClosedLoop);
//        }

//        public bool Set(float targetHeading, float targetDistance, float headingTolerance, float distanceTolerance, float maxOutput)
//        {
//            StraightDrive.ServoParameters.P = _straightServoParams.P;
//            StraightDrive.ServoParameters.I = _straightServoParams.I;
//            StraightDrive.ServoParameters.D = _straightServoParams.D;

//            _maxOutput = maxOutput;

//            return StraightDistance(targetHeading, targetDistance, headingTolerance, distanceTolerance);
//        }

//        /** Return the heading from the encoders */
//        public float GetEncoderHeading()
//        {
//            return _driveTrain.GetEncoderHeading();
//        }

//        /** Return the encoder distance of the drive train */
//        public float GetEncoderDistance()
//        {
//            return _driveTrain.GetDistance();
//        }

//        private bool StraightDistance(float targetHeading, float targetDistance, float headingTolerance, float distanceTolerance)
//        {
//            if (_straightServoParams.P == 0 && _straightServoParams.I == 0 && _straightServoParams.D == 0)
//                Debug.Print("HERO: Servo Straight Distance has no straight PID values, cannot go straight");
//            if (_distanceServoParams.P == 0 && _distanceServoParams.I == 0 && _distanceServoParams.D == 0)
//                Debug.Print("HERO: Servo Straight Distance has no distance PID values, cannot go forward");
//            /* Grab current heading and distance*/
//            float currentDistance = GetEncoderDistance();

//            /* Find the error between the target and current value */
//            _timeElapsed = _myStopWatch.Duration;
//            float distanceRate = ((currentDistance - _previousDistance) / (_timeElapsed));
//            _myStopWatch.Start();

//            /* Distance PID */
//            float distanceError = targetDistance - currentDistance;
//            float Y = (distanceError) * _distanceServoParams.P - (distanceRate) * _distanceServoParams.D;
//            Y = CTRE.Util.Cap(Y, _maxOutput);

//            /* StraightDrive moded selected when created within constructor */
//            if (_selectedStyle == Styles.Smart.Voltage)
//            {
//                _driveTrain.ConfigNominalPercentOutputVoltage(+0, -0);
//                _driveTrain.ConfigPeakPercentOutputVoltage(+_maxOutput, -_maxOutput);
//            }
//            bool headingCheck = StraightDrive.Set(Y, targetHeading, headingTolerance, _maxOutput);

//            _previousDistance = currentDistance;

//            if ((System.Math.Abs(distanceError) >= distanceTolerance) || (headingCheck == true))
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
//            _isRunning = false;
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
//                    bool running = StraightDistance(_targetHeading, _targetDistance, _headingTolerance, _distanceTolerance);

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
