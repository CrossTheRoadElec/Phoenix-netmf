using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;


namespace CTRE.Gadgeteer
{
    namespace Module
    {
        public class ModuleBase
        {
            public readonly char kModulePortType;
        }
    }

    public class PortDefinition
    {
        public char[] types;
        public int id;
    }

    public interface IPortGpio3
    {
        Cpu.Pin Pin3 { get; }
        Cpu.Pin Pin4 { get; }
        Cpu.Pin Pin5 { get; }
    }

    public interface IPortGpio7
    {
        Cpu.Pin Pin3 { get; }
        Cpu.Pin Pin4 { get; }
        Cpu.Pin Pin5 { get; }
        Cpu.Pin Pin6 { get; }
        Cpu.Pin Pin7 { get; }
        Cpu.Pin Pin8 { get; }
        Cpu.Pin Pin9 { get; }
    }

    public interface IPortAnalog
    {
        Cpu.AnalogChannel Analog_Pin3 { get; }
        Cpu.AnalogChannel Analog_Pin4 { get; }
        Cpu.AnalogChannel Analog_Pin5 { get; }

        Cpu.Pin Pin3 { get; }
        Cpu.Pin Pin4 { get; }
        Cpu.Pin Pin6 { get; }
    }

    public interface IPortSDCard
    {
        //SD Card Definitions

        Cpu.Pin Pin3 { get; }
    }

    public interface IPortI2C
    {
        //I2C Definitions

        Cpu.Pin Pin3 { get; }
        Cpu.Pin Pin6 { get; }
    }

    public interface IPortUartHandshake
    {
        //Uart + Handshake Definitions

        Cpu.Pin Pin3 { get; }
    }

    public interface IPortAnalogOut
    {
        //Cpu.AnalogOutputChannel AnalogOut_Pin5 { get; }

        Cpu.Pin Pin3 { get; }
        Cpu.Pin Pin4 { get; }
    }

    public interface IPortPWM
    {
        Cpu.PWMChannel PWM_Pin7 { get; }
        Cpu.PWMChannel PWM_Pin8 { get; }
        Cpu.PWMChannel PWM_Pin9 { get; }
    }

    public interface IPortSPI
    {
        Cpu.Pin Chip_Select { get; }
    }

    public interface IPortUart
    {
        string UART { get; }

        Cpu.Pin Pin3 { get; }
        Cpu.Pin Pin6 { get; }
    }
}
