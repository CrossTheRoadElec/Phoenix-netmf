////software pid Distanceservo then...
//// ServoGoStraight: software pid encoder-derived heading => talon (VelClosedLoop, PercentOutput, VoltageComp).
//using System;
//using Microsoft.SPOT;
//using CTRE.Drive;
//using CTRE.Tasking;
//using CTRE.MotorControllers;

//namespace CTRE.Motion
//{
//    public class ServoStraightDistanceSpecial : ILoopable
//    {
//        public enum Style
//        {
//            StraightPID, 
//            MotionMagic,
//        }
//        Style _selectedStyle { get; set; }

//        ISmartDrivetrain _driveTrain;

//        float _targetDistance;
//        float _distanceTolerance;
//        float _maxOutput;

//        bool _isRunning = false;
//        bool _isDone = false;
//        byte _state = 0;
//        byte _isGood = 0;

//        /** Straight servo constuctor that takes a smart drive train */
//        public ServoStraightDistanceSpecial(ISmartDrivetrain driveTrain, Style selectedStyle, float targetDistance, float distanceTolerance, float maxOutput)
//        {
//            _driveTrain = driveTrain;
//            _selectedStyle = selectedStyle;

//            //================================================//
//            _targetDistance = targetDistance;
//            _distanceTolerance = distanceTolerance;
//            _maxOutput = maxOutput;
//            //================================================//
//        }

//        /** Straight servo constuctor that takes a smart drive train */
//        public ServoStraightDistanceSpecial(ISmartDrivetrain driveTrain, Style selectedStyle)
//        {
//            _driveTrain = driveTrain;
//            _selectedStyle = selectedStyle;
//        }

//        public bool Set(float targetHeading, float headingTolerance, float maxOutput)
//        {
//            _maxOutput = maxOutput;

//            return StraightDistance(targetHeading, headingTolerance);
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

//        private bool StraightDistance(float targetDistance, float distanceTolerance)
//        {
//            /* Grab current heading and distance*/
//            float currentDistance = GetEncoderDistance();

//            /* Distance Error */
//            float distanceError = targetDistance - currentDistance;

//            /* StraightDrive moded selected when created within constructor */
//            switch(_selectedStyle)
//            {
//                case Style.MotionMagic:
//                    break;
//                case Style.StraightPID:
//                    break;

//            }


//            if ((System.Math.Abs(distanceError) >= distanceTolerance))
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
//                    bool running = StraightDistance(_targetDistance, _distanceTolerance);

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