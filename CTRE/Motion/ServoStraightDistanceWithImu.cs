////software pid Distanceservo then...
//// ServoGoStraight: software pid Pidgy heading => talon (VelClosedLoop, PercentOutput, VoltageComp).
//using System;
//using System.Threading;
//using Microsoft.SPOT;
//using CTRE.Drive;
//using CTRE.Tasking;
//using CTRE.MotorControllers;

//namespace CTRE.Motion
//{
//    public class ServoStraightDistanceWithImu : ILoopable
//    {
//        PigeonImu _pidgey;
//        ISmartDrivetrain _driveTrain;
//        Styles.Smart _selectedStyle;
//        ServoParameters _straightServoParameters = new ServoParameters();
//        ServoParameters _distanceServoParameters = new ServoParameters();
//        ServoGoStraightWithImuSmart StraightDrive;
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

//        /** Servo parameters */
//        public ServoParameters StraightServoParameters
//        {
//            get { return _straightServoParameters; }
//        }
//        public ServoParameters DistanceServoParameters
//        {
//            get { return _distanceServoParameters; }
//        }

//        /** Constructor that uses ServoStraightDistanceWithImu as an ILoopable */
//        public ServoStraightDistanceWithImu(PigeonImu pigeonImu, ISmartDrivetrain drivetrain, Styles.Smart selectedStyle, ServoParameters straightParameters, ServoParameters distanceParameters, float targetHeading, float targetDistance, float headingTolerance, float distanceTolerance, float maxOutput)
//        {
//            _pidgey = pigeonImu;
//            _driveTrain = drivetrain;
//            _selectedStyle = selectedStyle;

//            /* Construct a ServoGoStraightWithImu based on style */
//            if (_selectedStyle == Styles.Smart.Voltage)
//                StraightDrive = new ServoGoStraightWithImuSmart(_pidgey, _driveTrain, Styles.Smart.Voltage);
//            else if (_selectedStyle == Styles.Smart.PercentOutput)
//                StraightDrive = new ServoGoStraightWithImuSmart(_pidgey, _driveTrain, Styles.Smart.PercentOutput);
//            else if (_selectedStyle == Styles.Smart.VelocityClosedLoop)
//                StraightDrive = new ServoGoStraightWithImuSmart(_pidgey, _driveTrain, Styles.Smart.VelocityClosedLoop);

//            //============================================================//
//            _targetHeading = targetHeading;
//            _targetDistance = targetDistance;
//            _headingTolerance = headingTolerance;
//            _distanceTolerance = distanceTolerance;

//            _distanceServoParameters = distanceParameters;
//            StraightDrive.ServoParameters.P = straightParameters.P;
//            StraightDrive.ServoParameters.I = straightParameters.I;
//            StraightDrive.ServoParameters.D = straightParameters.D;

//            _maxOutput = maxOutput;
//            //============================================================//
//        }

//        /** Constructor that uses ServoStraightDistanceWithImu as an ILoopable */
//        public ServoStraightDistanceWithImu(PigeonImu pigeonImu, ISmartDrivetrain drivetrain, Styles.Smart selectedStyle)
//        {
//            _pidgey = pigeonImu;
//            _driveTrain = drivetrain;
//            _selectedStyle = selectedStyle;

//            /* Construct a ServoGoStraightWithImu based on style */
//            if (_selectedStyle == Styles.Smart.Voltage)
//                StraightDrive = new ServoGoStraightWithImuSmart(_pidgey, _driveTrain, Styles.Smart.Voltage);
//            else if (_selectedStyle == Styles.Smart.PercentOutput)
//                StraightDrive = new ServoGoStraightWithImuSmart(_pidgey, _driveTrain, Styles.Smart.PercentOutput);
//            else if (_selectedStyle == Styles.Smart.VelocityClosedLoop)
//                StraightDrive = new ServoGoStraightWithImuSmart(_pidgey, _driveTrain, Styles.Smart.VelocityClosedLoop);
//        }


//        /** Sets target heading/distance along with tolerances and updates PID gains */
//        public bool Set(float targetHeading, float targetDistance, float headingTolerance, float distanceTolerance, float maxOutput)
//        {
//            StraightDrive.ServoParameters.P = _straightServoParameters.P;
//            StraightDrive.ServoParameters.I = _straightServoParameters.I;
//            StraightDrive.ServoParameters.D = _straightServoParameters.D;

//            _maxOutput = maxOutput;

//            return StraightDistance(targetHeading, targetDistance, headingTolerance, distanceTolerance);
//        }

//        /** Return the heading from the Pigeon */
//        public float GetImuHeading()
//        {
//            float[] YPR = new float[3];
//            _pidgey.GetYawPitchRoll(YPR);
//            return YPR[0];
//        }

//        /** Return the encoder distance from the DriveTrain */
//        public float GetEncoderDistance()
//        {
//            return _driveTrain.GetDistance();
//        }

//        /** ServoStraightDistanceWithImu processing */
//        private bool StraightDistance(float targetHeading, float targetDistance, float headingTolerance, float distanceTolerance)
//        {
//            if (_straightServoParameters.P == 0 && _straightServoParameters.I == 0 && _straightServoParameters.D == 0)
//                Debug.Print("CTR: Servo Straight Distance With Imu has no straight PID values, cannot go straight");
//            if (_distanceServoParameters.P == 0 && _distanceServoParameters.I == 0 && _distanceServoParameters.D == 0)
//                Debug.Print("CTR: Servo Straight Distance With Imuhas no distance PID values, cannot go forward");
//            /* Grab current distance */
//            float currentDistance = GetEncoderDistance();

//            /* Grab the positionRate and elapsed time, must be done anytime we use D gain */
//            _timeElapsed = (_myStopWatch.Duration);
//            float positionRate = ((currentDistance - _previousDistance) / (_timeElapsed));
//            _myStopWatch.Start();

//            /* Distance PID */
//            float distanceError = targetDistance - currentDistance;
//            float Y = (distanceError) * _distanceServoParameters.P - (positionRate) * _distanceServoParameters.D;   //We want to PID object here
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
//                case 0: /* Init */
//                    _driveTrain.SetPosition(0.0f);
//                    _pidgey.SetYaw(0.0f);
//                    _state = 1;
//                    break;
//                case 1: /* Process */
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