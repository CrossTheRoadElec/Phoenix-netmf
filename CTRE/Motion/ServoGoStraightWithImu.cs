////software pid Pigeon heading => talon (PercentOutput, Voltage).
//using System;
//using Microsoft.SPOT;
//using CTRE.Phoenix.Drive;
//using CTRE.Phoenix.Tasking;
//using CTRE.Phoenix.MotorControl;

//namespace CTRE.Motion
//{
//    public class ServoGoStraightWithImu : ILoopable
//    {
//        PigeonImu _pidgey;
//        IDrivetrain _driveTrain;
//        Styles.Basic _selectedStyle;
//        ServoParameters _servoParams = new ServoParameters();

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
//            get { return _servoParams; }
//        }

//        /** Go Straight using the IMU */
//        public ServoGoStraightWithImu(PigeonImu pigeonImu, IDrivetrain driveTrain, Styles.Basic selectedStyle, ServoParameters parameters, float Y, float targetHeading, float headingTolerance, float maxOutput)
//        {
//            _pidgey = pigeonImu;
//            _driveTrain = driveTrain;
//            _selectedStyle = selectedStyle;

//            //=====================================//
//            _Y = Y;
//            _targetHeading = targetHeading;
//            _headingTolerance = headingTolerance;

//            _servoParams = parameters;

//            _maxOutput = maxOutput;
//            //=====================================//
//        }

//        /** Go Straight using the IMU */
//        public ServoGoStraightWithImu(PigeonImu pigeonImu, IDrivetrain driveTrain, Styles.Basic selectedStyle)
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
//            if (_servoParams.P == 0 && _servoParams.I == 0 && _servoParams.D == 0)
//                Debug.Print("CTR: Servo Go Straight With Imu has no PID values, cannot go straight");
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
//                float X = (headingError) * _servoParams.P - (currentAngularRate) * _servoParams.D;
//                X = Util.Cap(X, _maxOutput);
//                X = -X;

//                /* Select control mode based on selected style */
//                switch (_selectedStyle)
//                {
//                    case Styles.Basic.PercentOutput:
//                        _driveTrain.Set(Styles.Basic.PercentOutput, Y, X);
//                        break;
//                    case Styles.Basic.Voltage:
//                        _driveTrain.ConfigNominalPercentOutputVoltage(+0.0f, -0.0f);
//                        _driveTrain.ConfigPeakPercentOutputVoltage(+_maxOutput, -_maxOutput);
//                        _driveTrain.Set(Styles.Basic.Voltage, Y, X);
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
//            switch(_state)
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