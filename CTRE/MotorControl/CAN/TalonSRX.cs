using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.MotorControl
{
    public class TalonSRX : BaseMotorController, IMotorControllerEnhanced
    {
        SensorCollection _sensorColl;

        // : CANBusDevice TODO CLEANUP and package CAN stuff  /* all CAN stuff here */
        public TalonSRX(int deviceNumber, bool externalEnable = false) : base(deviceNumber | 0x02040000, externalEnable)
        {
            _sensorColl = new SensorCollection(_ll);
        }

        //------ Current Lim ----------//
        public ErrorCode ConfigPeakCurrentLimit(int amps, int timeoutMs = 0)
        {
            return _ll.ConfigPeakCurrentLimit(amps, timeoutMs);
        }
        public ErrorCode ConfigPeakCurrentDuration(int milliseconds, int timeoutMs = 0)
        {
            return _ll.ConfigPeakCurrentDuration(milliseconds, timeoutMs);
        }
        public ErrorCode ConfigContinuousCurrentLimit(int amps, int timeoutMs = 0)
        {
            return _ll.ConfigPeakCurrentDuration(amps, timeoutMs);
        }
        public void EnableCurrentLimit(bool enable)
        {
            _ll.EnableCurrentLimit(enable);
        }
        //------ Local sensor collection ----------//
        public SensorCollection SensorCollection { get { return _sensorColl; } }
    }
}




