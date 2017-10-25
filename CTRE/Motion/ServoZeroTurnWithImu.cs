//// no encoders, just Pigeon and op
//using System;
//using System.Threading;
//using Microsoft.SPOT;
//using CTRE.Drive;
//using CTRE.Tasking;
//using CTRE.MotorControllers;

//namespace CTRE.Motion
//{
//    public class ServoZeroTurnWithImu : ILoopable
//    {
//        PigeonImu _pidgey;
//        IDrivetrain _driveTrain;
//        Styles.Basic _selectedStyle;
//        ServoParameters _servoParams = new ServoParameters();

//        float _targetHeading;
//        float _headingTolerance;
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
//        public ServoZeroTurnWithImu(PigeonImu pigeonImu, IDrivetrain driveTrain, Styles.Basic selectedStyle, float targetHeading, float headingTolerance, ServoParameters Params, float maxOutput)
//        {
//            _pidgey = pigeonImu;
//            _driveTrain = driveTrain;
//            _selectedStyle = selectedStyle;

//            _targetHeading = targetHeading;
//            _headingTolerance = headingTolerance;

//            _maxOutput = maxOutput;

//            _servoParams = Params;
//        }

//        public ServoZeroTurnWithImu(PigeonImu pigeonImu, IDrivetrain driveTrain, Styles.Basic selectedStyle)
//        {
//            _selectedStyle = selectedStyle;
//            _pidgey = pigeonImu;
//            _driveTrain = driveTrain;
//        }

//        public bool Set(float targetHeading, float headingTolerance, float maxOutput)
//        {
//            _maxOutput = maxOutput;
//            return ZeroTurn(targetHeading, headingTolerance);
//        }

//        public float GetImuHeading()
//        {
//            float[] YPR = new float[3];
//            _pidgey.GetYawPitchRoll(YPR);
//            return YPR[0];
//        }

//        private bool ZeroTurn(float targetHeading, float headingTolerance)
//        {
//            if (_servoParams.P == 0 && _servoParams.I == 0 && _servoParams.D == 0)
//                Debug.Print("CTR: Servo Zero Turn With Imu has no PID values, cannot turn");
//            /* Grab the current heading */
//            float currentHeading = GetImuHeading();

//            /* Grab angular rate from the pigeon */
//            float[] XYZ_Dps = new float[3];
//            _pidgey.GetRawGyro(XYZ_Dps);
//            float currentAngularRate = XYZ_Dps[2];

//            /* Grab Pigeon IMU status */
//            bool angleIsGood = (_pidgey.GetState() == PigeonImu.PigeonState.Ready) ? true : false;

//            /* Runs ZeroTurn if Pigeon IMU is present and in good health, else do nothing */
//            if (angleIsGood == true)
//            {
//                /* Heading PID */
//                float headingError = targetHeading - currentHeading;
//                float X = (headingError) * _servoParams.P - (currentAngularRate) * _servoParams.D;
//                X = -X;
//                X = CTRE.Util.Cap(X, _maxOutput);


//                /** Set the output of the drivetrain */
//                switch (_selectedStyle)
//                {
//                    case Styles.Basic.PercentOutput:
//                        _driveTrain.Set(Styles.Basic.PercentOutput, 0, X);
//                        break;
//                    case Styles.Basic.Voltage:
//                        _driveTrain.Set(Styles.Basic.Voltage, 0, X);
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

//        /* ILoopable */
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
//                    Thread.Sleep(100);
//                    _pidgey.SetYaw(0.0f);
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