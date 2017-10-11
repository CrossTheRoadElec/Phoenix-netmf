/**
 * Example demonstrating the position closed-loop servo.
 * Tested with Logitech F350 USB Gamepad inserted into HERO.
 * 
 * Use the mini-USB cable to deploy/debug.
 *
 * Be sure to select the correct feedback sensor using SetFeedbackDevice() below.
 *
 * After deploying/debugging this to your HERO, first use the left Y-stick 
 * to throttle the Talon manually.  This will confirm your hardware setup.
 * Be sure to confirm that when the Talon is driving forward (green) the 
 * position sensor is moving in a positive direction.  If this is not the cause
 * flip the boolena input to the SetSensorDirection() call below.
 *
 * Once you've ensured your feedback device is in-phase with the motor,
 * use the button shortcuts to servo to target positions.  
 *
 * Tweak the PID gains accordingly.
 */
using System;
using System.Threading;
using Microsoft.SPOT;
using System.Text;

namespace Pigeon_IMU_Class
{
    public class Program
    {
        int kToPrintMin = 0;
        int kToPrintMaxInclus = 0;

        private int GetButtonFromBits(uint btnBits)
        {
            for(int i=0;i<16;++i)
            {
                if (((btnBits >> i) & 1) == 1)
                    return (i+1);
            }
            return -1;
        }
        private String ToString(float[] parameters)
        {
            String retval = "";
            foreach (var value in parameters)
            {
                retval += value;
                retval += ",";
            }
            return retval;
        }
        private String ToString(short[] parameters)
        {
            String retval = "";
            foreach (var value in parameters)
            {
                retval += value;
                retval += ",";
            }
            return retval;
        }
        public void RunForever()
        {
            int toPrint = 0;
            int loops = 0;
          
            CTRE.TalonSrx _talon = new CTRE.TalonSrx(0);   /* make a talon with deviceId 0 */

            CTRE.PigeonImu _pigeon;

            /** Use a USB gamepad plugged into the HERO */
            CTRE.Gamepad _gamepad = new CTRE.Gamepad(CTRE.UsbHostDevice.GetInstance());

            CTRE.GamepadValues _gamepadValues = new CTRE.GamepadValues();
            uint _oldBtns = 0;

            StringBuilder _sb = new StringBuilder();

            /** choose pigeon connected to Talon or on CAN Bus */
            if (true)
            {
                _pigeon = new CTRE.PigeonImu(0); /* Pigeon is on CAN Bus with device ID 0 */
            }
            else
            {
                _pigeon = new CTRE.PigeonImu(_talon); /* Pigeon is connected to _talon via Gadgeteer cable.*/
            }

            /* set frame rates */
            _pigeon.SetStatusFrameRateMs(CTRE.PigeonImu.StatusFrameRate.BiasedStatus_4_Mag, 17);
            _pigeon.SetStatusFrameRateMs(CTRE.PigeonImu.StatusFrameRate.CondStatus_1_General, 217);
            _pigeon.SetStatusFrameRateMs(CTRE.PigeonImu.StatusFrameRate.CondStatus_11_GyroAccum, 9999);
            _pigeon.SetStatusFrameRateMs(CTRE.PigeonImu.StatusFrameRate.CondStatus_9_SixDeg_YPR, 0);

            _pigeon.SetAccumZAngle(217);

            /* loop forever */
            while (true)
            {
                int ret = 0;

                /* signals for motion */
                float[] ypr = new float[3];
                float[] wxyz = new float[4];
                float compassHeading, magnitudeMicroTeslas;
                float tempC;
                CTRE.PigeonImu.PigeonState state;
                Int16[] rmxyz = new Int16[3];
                Int16[] bmxyz = new Int16[3];
                Int16[] baxyz = new short[3];
                Int16 baNorm;
                float[] xyz_dps = new float[3];
                float [] tilts_XZ_YZ_XY = new float[3];
                UInt32 timeSec;
                float fusedHeading;

                CTRE.PigeonImu.FusionStatus fusionStatus = new CTRE.PigeonImu.FusionStatus();
                CTRE.PigeonImu.GeneralStatus generalStatus = new CTRE.PigeonImu.GeneralStatus();


                /* get sigs */
                ret |= _pigeon.GetYawPitchRoll(ypr);
                ret |= _pigeon.Get6dQuaternion(wxyz);
                compassHeading = _pigeon.GetCompassHeading(); ret |= _pigeon.GetLastError();
                magnitudeMicroTeslas = _pigeon.GetCompassFieldStrength(); ret |= _pigeon.GetLastError();
                tempC = _pigeon.GetTemp(); ret |= _pigeon.GetLastError();
                state  = _pigeon.GetState(); ret |= _pigeon.GetLastError();
                ret |= _pigeon.GetRawMagnetometer(rmxyz );
                ret |= _pigeon.GetBiasedMagnetometer(bmxyz);
                ret |= _pigeon.GetBiasedAccelerometer(baxyz, out baNorm);
                ret |= _pigeon.GetRawGyro( xyz_dps);
                ret |= _pigeon.GetAccelerometerAngles(tilts_XZ_YZ_XY);
                timeSec = _pigeon.GetUpTime();
                fusedHeading = _pigeon.GetFusedHeading(fusionStatus);
                ret |= _pigeon.GetGeneralStatus(generalStatus);

                /* get buttons */
                _gamepad.GetAllValues(ref _gamepadValues);

                if (_gamepadValues.pov == 0) // up
                    _pigeon.EnableTemperatureCompensation(true);
                if (_gamepadValues.pov == 4) // dn
                    _pigeon.EnableTemperatureCompensation(false);


                if (_oldBtns != _gamepadValues.btns)
                {
                    int btnIdx = GetButtonFromBits(_gamepadValues.btns);
                    switch (btnIdx)
                    {
                        case 4:
                            if(toPrint < kToPrintMaxInclus) { ++toPrint; }
                            break;
                        case 2:
                            if (toPrint > kToPrintMin) { --toPrint; }
                            break;

                        case 5: // top left
                            _pigeon.SetYaw(100);
                            _pigeon.SetFusedHeading(100);
                            break;
                        case 7: // btm left
                            break;

                        case 6: // top right
                            _pigeon.SetYawToCompass();
                            _pigeon.SetFusedHeadingToCompass();
                            break;
                        case 8: // btm right
                            _pigeon.AddYaw(45);
                            _pigeon.AddYaw(45);
                            break;

                        case 10: // enter mag cal start btn
                            _pigeon.EnterCalibrationMode(CTRE.PigeonImu.CalibrationMode.Temperature);
                            break;
                        case 9: // accel 0,0,
                            _pigeon.EnterCalibrationMode(CTRE.PigeonImu.CalibrationMode.Temperature);
                            break;
                    }

                    _oldBtns = _gamepadValues.btns;
                }

                if (++loops >= 20)
                {
                    loops = 0;
                    
                    /* build row */
                    switch (toPrint)
                    {
                        case 0: _sb.Append("YPR:"); _sb.Append(ToString(ypr)); break;
                        case 1: _sb.Append("FusedHeading:"); _sb.Append(fusedHeading); _sb.Append(" deg, "); _sb.Append(fusionStatus.description); break;
                        case 2: _sb.Append("Compass:"); _sb.Append(compassHeading); _sb.Append(" deg, "); _sb.Append(magnitudeMicroTeslas); _sb.Append("uT"); break;
                        case 3: _sb.Append("tempC:"); _sb.Append(tempC); _sb.AppendLine(); break;
                        case 4: _sb.Append("PigeonState:"); _sb.Append(state); _sb.AppendLine(); break;
                        case 5: _sb.Append("BiasedM:"); _sb.Append(ToString(bmxyz)); break;
                        case 6: _sb.Append("RawM:"); _sb.Append(ToString(rmxyz)); break;
                        case 7: _sb.Append("BiasedAccel:"); _sb.Append(ToString(baxyz)); break;
                        case 8: _sb.Append("GyroDps:"); _sb.Append(ToString(xyz_dps)); break;
                        case 9: _sb.Append("AccelTilts:"); _sb.Append(ToString(tilts_XZ_YZ_XY)); break;
                        case 10: _sb.Append("Quat:"); _sb.Append(ToString(wxyz)); break;
                        case 11: _sb.Append("UpTime:"); _sb.Append(timeSec); break;
                        case 12: _sb.Append("GS:"); _sb.Append(generalStatus.description); break;
                        case 13:    _sb.Append("tempCompensationCount:"); _sb.Append(generalStatus.tempCompensationCount);
                            _sb.Append("  tempC:"); _sb.Append(generalStatus.tempC);
                            _sb.Append("  noMotionBiasCount:"); _sb.Append(generalStatus.noMotionBiasCount);
                            break;
                    }

                    

                    kToPrintMin = 0;
                    kToPrintMaxInclus = 13;

                    //_sb.Append(yaw); _sb.Append(","); _sb.Append(pitch); _sb.Append(","); _sb.Append(roll); _sb.AppendLine();
                    //_sb.Append(w); _sb.Append(","); _sb.Append(x); _sb.Append(","); _sb.Append(y); _sb.Append(","); _sb.Append(z); _sb.AppendLine();
                    //_sb.Append(compassHeading); _sb.Append(","); _sb.Append(magnitudeMicroTeslas); _sb.AppendLine();
                    //_sb.Append(tempC); _sb.AppendLine();
                    //_sb.Append(state); _sb.AppendLine();
                    //_sb.Append(rmx); _sb.Append(","); _sb.Append(rmy); _sb.Append(","); _sb.Append(rmz); _sb.AppendLine();
                    //_sb.Append(bmx); _sb.Append(","); _sb.Append(bmy); _sb.Append(","); _sb.Append(bmz); _sb.AppendLine();
                    //_sb.Append(bam); _sb.Append(","); _sb.Append(bay); _sb.Append(","); _sb.Append(baz); _sb.Append(","); _sb.Append(baNorm); _sb.AppendLine();
                    //_sb.Append(x_dps); _sb.Append(","); _sb.Append(y_dps); _sb.Append(","); _sb.Append(z_dps); _sb.AppendLine();
                    //_sb.Append(tiltXZ); _sb.Append(","); _sb.Append(tiltYZ); _sb.Append(","); _sb.Append(tiltXY); _sb.AppendLine();
                    //_sb.Append(timeSec); _sb.Append(",");
                    //_sb.Append(fusedHeading); _sb.Append(","); _sb.Append(bIsValid); _sb.Append(","); _sb.Append(bIsFusing); _sb.AppendLine();
                    //_sb.Append(ret); _sb.Append(","); 
                }

                /* print line and clear buffer */
                if (_sb.Length != 0)
                {
                    Debug.Print(_sb.ToString());
                    _sb.Clear();
                }

                /* 10ms loop */
                Thread.Sleep(10);

                if (_pigeon.HasResetOccured())
                {
                    int resetCount;
                    int resetFlags;
                    int firmVers;

                    resetCount = _pigeon.GetResetCount();
                    resetFlags = _pigeon.GetResetFlags();
                    firmVers = _pigeon.GetFirmVers();

                    Debug.Print("Pigeon Reset detected...");
                    Debug.Print(" Pigeon Reset Count:" + resetCount.ToString("D"));
                    Debug.Print(" Pigeon Reset Flags:" + resetFlags.ToString("X"));
                    Debug.Print(" Pigeon FirmVers:" + firmVers.ToString("X"));
                    Debug.Print(" Waiting for delay");
                    Thread.Sleep(3000);
                }
            }
        }

        /** Simple stub to start our project */
        static Program _program = new Program();
        public static void Main() { _program.RunForever(); }
    }
}
