////software pid Pigeon heading => talon (VelClosedLoop, PercentOutput, Voltage).
//using System;
//using Microsoft.SPOT;
//using CTRE.Drive;
//using CTRE.Tasking;
//using CTRE.MotorControllers;

//namespace CTRE.Motion
//{
//    public class ServoGoStraightWithImuSmart : ILoopable
//    {
//        PigeonImu _pidgey;
//        ISmartDrivetrain _driveTrain;
//        Styles.Smart _selectedStyle;
//        ServoParameters _servoParameters = new ServoParameters();

//        float _Y;
//        float _targetHeading;
//        float _headingTolerance;
//        float _maxOutput;

//        bool _isRunning = false;
//        bool _isDone = false;
//        byte _state = 0;

//        /* Servo parameters */
//        public ServoParameters ServoParameters
//        {
//            get { return _servoParameters; }
//        }

//        /** Constructor that uses ServoGoStraightWithImuSmart as an ILoopable */
//        public ServoGoStraightWithImuSmart(PigeonImu pigeonImu, ISmartDrivetrain driveTrain, Styles.Smart selectedStyle, ServoParameters straightParameters, float Y, float targetHeading, float headingTolerance, float maxOutput)
//        {
//            _pidgey = pigeonImu;
//            _driveTrain = driveTrain;
//            _selectedStyle = selectedStyle;

//            //=====================================//
//            _Y = Y;
//            _targetHeading = targetHeading;
//            _headingTolerance = headingTolerance;

//            _servoParameters = straightParameters;

//            _maxOutput = maxOutput;
//            //=====================================//
//        }

//        /** Constructor that uses ServoGoStraightWithImuSmart as an ILoopable */
//        public ServoGoStraightWithImuSmart(PigeonImu pigeonImu, ISmartDrivetrain driveTrain, Styles.Smart selectedStyle)
//        {
//            _pidgey = pigeonImu;
//            _driveTrain = driveTrain;
//            _selectedStyle = selectedStyle;
//        }

//        public bool Set(float Y, float targetHeading, float headingTolerance, float maxOutput)
//        {
//            _maxOutput = maxOutput;
//            return GoStraight(Y, targetHeading, headingTolerance);
//        }

//        /** Return the heading from the Pigeon*/
//        public float GetImuHeading()
//        {
//            float[] YPR = new float[3];
//            _pidgey.GetYawPitchRoll(YPR);
//            return YPR[0];
//        }

//        private bool GoStraight(float Y, float targetHeading, float headingTolerance)
//        {
//            if (_servoParameters.P == 0 && _servoParameters.I == 0 && _servoParameters.D == 0)
//                Debug.Print("HERO: Servo Go Straight With Imu Smart has no PID values, cannot go straight");
//            /* Grab current heading */
//            float currentHeading = GetImuHeading();

//            /* Grab angular rate from the pigeon */
//            float[] XYZ_Dps = new float[3];
//            _pidgey.GetRawGyro(XYZ_Dps);
//            float currentAngularRate = XYZ_Dps[2];

//            /* Grab Pigeon IMU status */
//            bool angleIsGood = (_pidgey.GetState() == PigeonImu.PigeonState.Ready) ? true : false;

//            /* Runs GoStraight if Pigeon IMU is present and in good health, else stop drivetrain */
//            if (angleIsGood == true)
//            {
//                /* Heading PID */
//                float headingError = targetHeading - currentHeading;
//                float X = (headingError) * _servoParameters.P - (currentAngularRate) * _servoParameters.D;
//                X = -X;
//                X = CTRE.Util.Cap(X, _maxOutput);

//                /* Select control mode based on selected style */
//                switch (_selectedStyle)
//                {
//                    case Styles.Smart.PercentOutput:
//                        _driveTrain.Set(Styles.Smart.PercentOutput, Y, X);
//                        break;
//                    case Styles.Smart.Voltage:
//                        _driveTrain.ConfigNominalPercentOutputVoltage(+0.0f, -0.0f);
//                        _driveTrain.ConfigPeakPercentOutputVoltage(+_maxOutput, -_maxOutput);
//                        _driveTrain.Set(Styles.Smart.Voltage, Y, X);
//                        break;
//                    case Styles.Smart.VelocityClosedLoop:
//                        /* MotionMagic/ClosedLoop configured by caller in the drivetrain/motorcontroller level */
//                        _driveTrain.Set(Styles.Smart.VelocityClosedLoop, Y, X);
//                        break;
//                }

//                if (System.Math.Abs(headingError) >= headingTolerance)
//                {
//                    _isRunning = true;
//                }
//                else
//                {
//                    _isRunning = false;
//                }
//            }
//            else if (angleIsGood == false)
//            {
//                _driveTrain.Set(Styles.Basic.PercentOutput, 0, 0);
//                _isRunning = false;
//            }
//            return _isRunning;
//        }

//        /** ILoopable */
//        public void OnStart()
//        {
//            _isDone = false;
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
//                    _pidgey.SetYaw(0.0f);
//                    break;
//                case 1:
//                    GoStraight(_Y, _targetHeading, _headingTolerance);
//                    break;
//            }
//        }
//    }
//}