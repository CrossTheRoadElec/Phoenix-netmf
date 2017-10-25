/**
 * Object driver for CANifier, a multi purpose CAN device capable of
 * - driving RGB common anode LED strip.
 * - reading up to four general purpose PWM inputs.
 * - generating up to four general purpose PWM outputs.
 * - I2C/SPI transfers queued over CAN bus.
 * - 11 3.3V GPIOs
 * - Quadrature input
 * - field-upgradeable for future Talon/Pigeon/CANifier control features.
 */
using System;
using Microsoft.SPOT;
using CTRE.Phoenix.LowLevel;

namespace CTRE.Phoenix
{
    public class CANifier
    {
        CANifier_LowLevel _ll;

        public enum LEDChannel
        {
            LEDChannelA,
            LEDChannelB,
            LEDChannelC
        };

        public enum PWMChannel
        {
            PWMChannel0,
            PWMChannel1,
            PWMChannel2,
            PWMChannel3,
        };
        public const uint PWMChannelCount = 4;

        public enum GeneralPin
        {
            QUAD_IDX = (0),
            QUAD_B = (1),
            QUAD_A = (2),
            LIMR = (3),
            LIMF = (4),
            SDA = (5),
            SCL = (6),
            SPI_CS = (7),
            SPI_MISO_PWM2P = (8),
            SPI_MOSI_PWM1P = (9),
            SPI_CLK_PWM0P = (10),
        }

        public struct PinValues
        {
            public bool QUAD_IDX;
            public bool QUAD_B;
            public bool QUAD_A;
            public bool LIMR;
            public bool LIMF;
            public bool SDA;
            public bool SCL;
            public bool SPI_CS_PWM3;
            public bool SPI_MISO_PWM2;
            public bool SPI_MOSI_PWM1;
            public bool SPI_CLK_PWM0;
        }

        public enum StatusFrameRate
        {
            Status1_General = 0,
            Status2_General = 1,
            Status3_PwmInput0 = 2,
            Status4_PwmInput1 = 3,
            Status5_PwmInput2 = 4,
            Status6_PwmInput3 = 5,
        }

        private int[] _tempPins = new int[11];

        public CANifier(UInt16 deviceId, bool externalEnable = false)
        {
            _ll = new CANifier_LowLevel(deviceId, externalEnable);
        }

        public ErrorCode SetLEDOutput(float percentOutput, LEDChannel ledChannel)
        {
            /* convert float to integral fixed pt */
            if (percentOutput > 1) { percentOutput = 1; }
            if (percentOutput < 0) { percentOutput = 0; }
            int dutyCycle = (int)(percentOutput * 1023); // [0,1023]

            return _ll.SetLEDOutput(dutyCycle, (uint)ledChannel);
        }

        public ErrorCode SetGeneralOutput(GeneralPin outputPin, bool outputValue, bool outputEnable)
        {
            return _ll.SetGeneralOutput((CANifier_LowLevel.GeneralPin)outputPin, outputValue, outputEnable);
        }
        public ErrorCode SetGeneralOutputs(UInt32 outputBits, UInt32 isOutputBits)
        {
            return _ll.SetGeneralOutputs(outputBits, isOutputBits);
        }

        public ErrorCode GetGeneralInputs(PinValues allPins)
        {
            ErrorCode err = _ll.GetGeneralInputs(_tempPins);
            allPins.LIMF = _tempPins[(int)GeneralPin.LIMF] != 0;
            allPins.LIMR = _tempPins[(int)GeneralPin.LIMR] != 0;
            allPins.QUAD_A = _tempPins[(int)GeneralPin.QUAD_A] != 0;
            allPins.QUAD_B = _tempPins[(int)GeneralPin.QUAD_B] != 0;
            allPins.QUAD_IDX = _tempPins[(int)GeneralPin.QUAD_IDX] != 0;
            allPins.SCL = _tempPins[(int)GeneralPin.SCL] != 0;
            allPins.SDA = _tempPins[(int)GeneralPin.SDA] != 0;
            allPins.SPI_CLK_PWM0 = _tempPins[(int)GeneralPin.SPI_CLK_PWM0P] != 0;
            allPins.SPI_MOSI_PWM1 = _tempPins[(int)GeneralPin.SPI_MOSI_PWM1P] != 0;
            allPins.SPI_MISO_PWM2 = _tempPins[(int)GeneralPin.SPI_MISO_PWM2P] != 0;
            allPins.SPI_CS_PWM3 = _tempPins[(int)GeneralPin.SPI_CS] != 0;

            return err;
        }
        /**
         * Call GetLastError() to determine success.
         * @return true if specified input is high, false o/w.  
         */
        public bool GetGeneralInput(GeneralPin inputPin)
        {
            return _ll.GetGeneralInput((CANifier_LowLevel.GeneralPin)inputPin);
        }
        public ErrorCode SetStatusFramePeriod(StatusFrameRate statusFrame, uint newPeriodMs, uint timeoutMs = 0)
        {
            return (ErrorCode) _ll.SetStatusFramePeriod((uint)statusFrame, newPeriodMs, timeoutMs);
        }

        public ErrorCode SetPWMOutput(uint pwmChannel, float dutyCycle)
        {
            if (dutyCycle < 0) { dutyCycle = 0; } else if(dutyCycle > 1) { dutyCycle = 1; }

            int dutyCyc10bit = (int)(1023 * dutyCycle);

            return (ErrorCode)_ll.SetPWMOutput((uint)pwmChannel, dutyCyc10bit);
        }


        public ErrorCode GetPwmInput(PWMChannel pwmChannel, float[] dutyCycleAndPeriod)
        {
            return (ErrorCode)_ll.GetPwmInput((uint)pwmChannel, dutyCycleAndPeriod);
        }
    }
}
