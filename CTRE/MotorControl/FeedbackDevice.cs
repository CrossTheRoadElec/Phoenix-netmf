using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.MotorControl
{
    /**
     *  Motor controller with gadgeteer connector.
     */
    public enum FeedbackDevice
    {
        None = -1,

        QuadEncoder = 0,
        Analog = 2,
        Tachometer = 4,
        PulseWidthEncodedPosition = 8,

        SensorSum = 9,
        SensorDifference = 10,
        Inertial = 11,
        RemoteSensor = 12,
        CurrentDrawMilliamperes = 14,
        SoftwarEmulatedSensor = 15,
    };

    public enum RemoteCANifierOrTalonOrPigeon
    {
        QuadEncoder = 0,
        Analog = 2,
        Tachometer = 4,
        PulseWidthEncodedPosition = 8,
        Inertial = 11,
    };

    ///**
    // *  All Motor controllers can select remote signals over CAN Bus (Talon SRX and Cyclone)
    // */
    //public enum RemoteFeedbackDevice
    //{
    //    None = -1,

    //    SensorSum = 9,
    //    SensorDifference = 10,
    //    InertialAxis = 11,
    //    RemoteSensor = 12,
    //    CurrentDrawMilliamperes = 14,
    //    SoftwarEmulatedSensor = 15,
    //}

    public class RemoteFeedbackDevice
    {
        public RemoteCANifierOrTalonOrPigeon _type;

        //----- very specific details here that are not worth exposing. ---- //
        internal int _peripheralIndex;
        internal int _reserved;
        internal int _arbId;
        internal TalonSRX _talon;
        internal CANifier _canifier;
        internal object _pigeon;

        public RemoteFeedbackDevice(TalonSRX talon, RemoteCANifierOrTalonOrPigeon type)
        {
            _type = type;
            _peripheralIndex = 0;
            _reserved = 2;
            _talon = talon;
            _arbId = 0;
        }
        public RemoteFeedbackDevice(CANifier canifier, RemoteCANifierOrTalonOrPigeon type)
        {
            _type = type;
            _peripheralIndex = 0;
            _reserved = 3;
            _canifier = canifier;
            _arbId = 0;
        }
        public RemoteFeedbackDevice(object pigeonIMU, RemoteCANifierOrTalonOrPigeon type)
        {
            _type = type;
            _peripheralIndex = 0;
            _reserved = 0;
            _pigeon = pigeonIMU;
            _arbId = 0;
        }

        public ErrorCode SetPosition(float position, int timeoutMs)
        {
            //TODO, use the base objects and switch on the selected type.
            return ErrorCode.FeatureNotSupported;
        }
    }
}
