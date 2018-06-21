using System;
using Microsoft.SPOT;
using CTRE.Phoenix.MotorControl.CAN;

namespace CTRE.Phoenix.MotorControl
{
    /**
     *  Motor controller with gadgeteer connector.
     */
    public enum FeedbackDevice
    {
        //NOTE: None was removed as it doesn't exist in firmware
        //TODO: Add None to firmware and add None back in

        QuadEncoder = 0,
        //1
        Analog = 2,
        //3
        Tachometer = 4,
        PulseWidthEncodedPosition = 8,

        SensorSum = 9,
        SensorDifference = 10,
        RemoteSensor0 = 11,
        RemoteSensor1 = 12,
        //13
        //14
        SoftwareEmulatedSensor = 15,

        CTRE_MagEncoder_Absolute = PulseWidthEncodedPosition,
        CTRE_MagEncoder_Relative = QuadEncoder,
    };

    /**
     *  All Motor controllers can select remote signals over CAN Bus (Talon SRX and Cyclone)
     */

    public enum RemoteFeedbackDevice
    {
        //NOTE: RemoteFeedbackDevice_None was removed as it doesn't exist in firmware
        //TODO: Add RemoteFeedbackDevice_None to firmware and add RemoteFeedbackDevice_None back in

        RemoteFeedbackDevice_SensorSum = 9,
        RemoteFeedbackDevice_SensorDifference = 10,
        RemoteFeedbackDevice_RemoteSensor0 = 11,
        RemoteFeedbackDevice_RemoteSensor1 = 12,
        //13
        //14
        RemoteFeedbackDevice_SoftwareEmulatedSensor = 15,
    }
public class FeedbackDeviceRoutines {
    public static string ToString(FeedbackDevice value) {
        switch(value) {
            case FeedbackDevice.QuadEncoder : return "FeedbackDevice.QuadEncoder";
            case FeedbackDevice.Analog : return "FeedbackDevice.Analog";
            case FeedbackDevice.Tachometer : return "FeedbackDevice.Tachometer";
            case FeedbackDevice.PulseWidthEncodedPosition : return "FeedbackDevice.PulseWidthEncodedPosition";
            case FeedbackDevice.SensorSum : return "FeedbackDevice.SensorSum";
            case FeedbackDevice.SensorDifference : return "FeedbackDevice.SensorDifference";
            case FeedbackDevice.RemoteSensor0 : return "FeedbackDevice.RemoteSensor0";
            case FeedbackDevice.RemoteSensor1 : return "FeedbackDevice.RemoteSensor1";
            case FeedbackDevice.SoftwareEmulatedSensor : return "FeedbackDevice.SoftwareEmulatedSensor";
            default : return "InvalidValue";

        }

    }

    public static string ToString(RemoteFeedbackDevice value) {
        switch(value) {
            case 0 : return "None (factory default value)"; 
            case RemoteFeedbackDevice.RemoteFeedbackDevice_SensorSum: return "RemoteFeedbackDevice.RemoteFeedbackDevice_SensorSum";
            case RemoteFeedbackDevice.RemoteFeedbackDevice_SensorDifference: return "RemoteFeedbackDevice.RemoteFeedbackDevice_SensorDifference";
            case RemoteFeedbackDevice.RemoteFeedbackDevice_RemoteSensor0: return "RemoteFeedbackDevice.RemoteFeedbackDevice_RemoteSensor0";
            case RemoteFeedbackDevice.RemoteFeedbackDevice_RemoteSensor1: return "RemoteFeedbackDevice.RemoteFeedbackDevice_RemoteSensor1";
            case RemoteFeedbackDevice.RemoteFeedbackDevice_SoftwareEmulatedSensor: return "RemoteFeedbackDevice.RemoteFeedbackDevice_SoftwareEmulatedSensor";
            default : return "InvalidValue";
        }
            
    }
};

}
