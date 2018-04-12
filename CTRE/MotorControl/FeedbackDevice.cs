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
        None = -1,

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
        RemoteFeedbackDevice_None = -1,

        RemoteFeedbackDevice_SensorSum = 9,
        RemoteFeedbackDevice_SensorDifference = 10,
        RemoteFeedbackDevice_RemoteSensor0 = 11,
        RemoteFeedbackDevice_RemoteSensor1 = 12,
        //13
        //14
        RemoteFeedbackDevice_SoftwareEmulatedSensor = 15,
    }

}
