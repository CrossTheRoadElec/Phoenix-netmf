using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.MotorControl
{
    public enum RemoteSensorSource
    {
        RemoteSensorSource_Off,
        RemoteSensorSource_TalonSRX_SelectedSensor,
        RemoteSensorSource_Pigeon_Yaw,
        RemoteSensorSource_Pigeon_Pitch,
        RemoteSensorSource_Pigeon_Roll,
        RemoteSensorSource_CANifier_Quadrature,
        RemoteSensorSource_CANifier_PWMInput0,
        RemoteSensorSource_CANifier_PWMInput1,
        RemoteSensorSource_CANifier_PWMInput2,
        RemoteSensorSource_CANifier_PWMInput3,
        RemoteSensorSource_GadgeteerPigeon_Yaw,
        RemoteSensorSource_GadgeteerPigeon_Pitch,
        RemoteSensorSource_GadgeteerPigeon_Roll,
    };
public class RemoteSensorSourceRoutines {
    public static string ToString(RemoteSensorSource value) {
        switch(value) {
            case RemoteSensorSource.Off                     : return "RemoteSensorSource.Off";
            case RemoteSensorSource.TalonSRX_SelectedSensor : return "RemoteSensorSource.TalonSRX_SelectedSensor";
            case RemoteSensorSource.Pigeon_Yaw              : return "RemoteSensorSource.Pigeon_Yaw";
            case RemoteSensorSource.Pigeon_Pitch            : return "RemoteSensorSource.Pigeon_Pitch";
            case RemoteSensorSource.Pigeon_Roll             : return "RemoteSensorSource.Pigeon_Roll";
            case RemoteSensorSource.CANifier_Quadrature     : return "RemoteSensorSource.CANifier_Quadrature";
            case RemoteSensorSource.CANifier_PWMInput0      : return "RemoteSensorSource.CANifier_PWMInput0";
            case RemoteSensorSource.CANifier_PWMInput1      : return "RemoteSensorSource.CANifier_PWMInput1";
            case RemoteSensorSource.CANifier_PWMInput2      : return "RemoteSensorSource.CANifier_PWMInput2";
            case RemoteSensorSource.CANifier_PWMInput3      : return "RemoteSensorSource.CANifier_PWMInput3";
            case RemoteSensorSource.GadgeteerPigeon_Yaw     : return "RemoteSensorSource.GadgeteerPigeon_Yaw";
            case RemoteSensorSource.GadgeteerPigeon_Pitch   : return "RemoteSensorSource.GadgeteerPigeon_Pitch";
            case RemoteSensorSource.GadgeteerPigeon_Roll    : return "RemoteSensorSource.GadgeteerPigeon_Roll";
            default : return "InvalidValue";
        }
    }
};

}
