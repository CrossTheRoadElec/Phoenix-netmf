using System;
using Microsoft.SPOT;
using CTRE.Phoenix.LowLevel;

namespace CTRE.Phoenix
{
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

        public class PinValues
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
        public ErrorCode SetStatusFramePeriod(CANifierStatusFrame statusFrame, uint newPeriodMs, uint timeoutMs = 0)
        {
            return (ErrorCode) _ll.SetStatusFramePeriod(statusFrame, newPeriodMs, timeoutMs);
        }

        public ErrorCode SetPWMOutput(uint pwmChannel, float dutyCycle)
        {
            if (dutyCycle < 0) { dutyCycle = 0; } else if(dutyCycle > 1) { dutyCycle = 1; }

            int dutyCyc10bit = (int)(1023 * dutyCycle);

            return (ErrorCode)_ll.SetPWMOutput((uint)pwmChannel, dutyCyc10bit);
        }

        /**
	     * Enables PWM Outputs
	     * Currently supports PWM 0, PWM 1, and PWM 2
	     * @param pwmChannel  Index of the PWM channel to enable.
	     * @param bEnable			"True" enables output on the pwm channel.
	     */
        public void EnablePWMOutput(int pwmChannel, bool bEnable)
        {
            if (pwmChannel < 0)
            {
                pwmChannel = 0;
            }

            _ll.EnablePWMOutput((uint)pwmChannel, bEnable);
        }

        [Obsolete("Use GetPWMInput instead (different capitalization).")]
        public ErrorCode GetPwmInput(PWMChannel pwmChannel, float[] pulseWidthAndPeriod)
        {
            return GetPWMInput(pwmChannel, pulseWidthAndPeriod);
        }
        
        public ErrorCode GetPWMInput(PWMChannel pwmChannel, float[] pulseWidthAndPeriod)
        {
            return (ErrorCode)_ll.GetPWMInput((uint)pwmChannel, pulseWidthAndPeriod);
        }

        /**
	 * Gets the quadrature encoder's position
	 * @return Position of encoder 
	 */
        public int GetQuadraturePosition()
        {
            int pos;
            ErrorCode err = _ll.GetQuadraturePosition(out pos);
            return pos;
        }

        /**
         * Sets the quadrature encoder's position
         * @param newPosition  Position to set
         * @param timeoutMs  
                        Timeout value in ms. If nonzero, function will wait for
                        config success and report an error if it times out.
                        If zero, no blocking or checking is performed.
         * @return Error Code generated by function. 0 indicates no error.
         */
        public ErrorCode SetQuadraturePosition(int newPosition, int timeoutMs)
        {
            return _ll.SetQuadraturePosition(newPosition, timeoutMs);
        }

        /**
         * Gets the quadrature encoder's velocity
         * @return Velocity of encoder
         */
        public int GetQuadratureVelocity()
        {
            int vel;
            ErrorCode err = _ll.GetQuadratureVelocity(out vel);
            return vel;
        }

        /**
         * Configures the period of each velocity sample.
         * Every 1ms a position value is sampled, and the delta between that sample
         * and the position sampled kPeriod ms ago is inserted into a filter.
         * kPeriod is configured with this function.
         *
         * @param period
         *            Desired period for the velocity measurement. @see
         *            #VelocityMeasPeriod
         * @param timeoutMs
         *            Timeout value in ms. If nonzero, function will wait for
         *            config success and report an error if it times out.
         *            If zero, no blocking or checking is performed.
         * @return Error Code generated by function. 0 indicates no error.
         */
        public ErrorCode ConfigVelocityMeasurementPeriod(CANifierVelocityMeasPeriod period, int timeoutMs)
        {
            ErrorCode retval = _ll.ConfigVelocityMeasurementPeriod(period, timeoutMs);
            return retval;
        }

        /**
         * Sets the number of velocity samples used in the rolling average velocity
         * measurement.
         *
         * @param windowSize
         *            Number of samples in the rolling average of velocity
         *            measurement. Valid values are 1,2,4,8,16,32. If another
         *            value is specified, it will truncate to nearest support value.
         * @param timeoutMs
         *            Timeout value in ms. If nonzero, function will wait for
         *            config success and report an error if it times out.
         *            If zero, no blocking or checking is performed.
         * @return Error Code generated by function. 0 indicates no error.
         */
        public ErrorCode ConfigVelocityMeasurementWindow(int windowSize, int timeoutMs)
        {
            ErrorCode retval = _ll.ConfigVelocityMeasurementWindow(windowSize, timeoutMs);
            return retval;
        }

      /**
        * Sets the value of a custom parameter. This is for arbitrary use.
        *
        * Sometimes it is necessary to save calibration/duty cycle/output
        * information in the device. Particularly if the
        * device is part of a subsystem that can be replaced.
        *
        * @param newValue
        *            Value for custom parameter.
        * @param paramIndex
        *            Index of custom parameter. [0-1]
        * @param timeoutMs
        *            Timeout value in ms. If nonzero, function will wait for
        *            config success and report an error if it times out.
        *            If zero, no blocking or checking is performed.
        * @return Error Code generated by function. 0 indicates no error.
        */
        public ErrorCode ConfigSetCustomParam(int newValue, int paramIndex, int timeoutMs)
        {
            ErrorCode retval = _ll.ConfigSetCustomParam(newValue, paramIndex, timeoutMs);
            return retval;
        }

      /**
        * Gets the value of a custom parameter. This is for arbitrary use.
        *
        * Sometimes it is necessary to save calibration/duty cycle/output
        * information in the device. Particularly if the
        * device is part of a subsystem that can be replaced.
        *
        * @param paramIndex
        *            Index of custom parameter. [0-1]
        * @param timoutMs
        *            Timeout value in ms. If nonzero, function will wait for
        *            config success and report an error if it times out.
        *            If zero, no blocking or checking is performed.
        * @return Value of the custom param.
        */
        public int ConfigGetCustomParam(int paramIndex, int timoutMs)
        {
            int readValue;
            _ll.ConfigGetCustomParam(out readValue, paramIndex, timoutMs);
            return readValue;
        }


        /**
	 * Sets a parameter. Generally this is not used.
   * This can be utilized in
   * - Using new features without updating API installation.
   * - Errata workarounds to circumvent API implementation.
   * - Allows for rapid testing / unit testing of firmware.
	 *
	 * @param param
	 *            Parameter enumeration.
	 * @param value
	 *            Value of parameter.
	 * @param subValue
	 *            Subvalue for parameter. Maximum value of 255.
	 * @param ordinal
	 *            Ordinal of parameter.
	 * @param timeoutMs
	 *            Timeout value in ms. If nonzero, function will wait for
   *            config success and report an error if it times out.
   *            If zero, no blocking or checking is performed.
	 * @return Error Code generated by function. 0 indicates no error.
	 */
        public ErrorCode ConfigSetParameter(ParamEnum param, float value, int subValue, int ordinal, int timeoutMs)
        {
            ErrorCode retval = _ll.ConfigSetParameter(param, value, (byte)subValue, ordinal,
                    timeoutMs);
            return retval;
        }

        /**
         * Sets a parameter. Generally this is not used.
       * This can be utilized in
       * - Using new features without updating API installation.
       * - Errata workarounds to circumvent API implementation.
       * - Allows for rapid testing / unit testing of firmware.
         *
         * @param param
         *            Parameter enumeration.
         * @param value
         *            Value of parameter.
         * @param subValue
         *            Subvalue for parameter. Maximum value of 255.
         * @param ordinal
         *            Ordinal of parameter.
         * @param timeoutMs
         *            Timeout value in ms. If nonzero, function will wait for
       *            config success and report an error if it times out.
       *            If zero, no blocking or checking is performed.
         * @return Error Code generated by function. 0 indicates no error.
         */
        public ErrorCode ConfigSetParameter(int param, float value, int subValue, int ordinal, int timeoutMs)
        {
            ErrorCode retval = _ll.ConfigSetParameter((ParamEnum)param, value, (byte)subValue, ordinal,
                    timeoutMs);
            return retval;
        }
        /**
         * Gets a parameter. Generally this is not used.
       * This can be utilized in
       * - Using new features without updating API installation.
       * - Errata workarounds to circumvent API implementation.
       * - Allows for rapid testing / unit testing of firmware.
         *
         * @param param
         *            Parameter enumeration.
         * @param ordinal
         *            Ordinal of parameter.
         * @param timeoutMs
         *            Timeout value in ms. If nonzero, function will wait for
       *            config success and report an error if it times out.
       *            If zero, no blocking or checking is performed.
         * @return Value of parameter.
         */
        public double ConfigGetParameter(ParamEnum param, int ordinal, int timeoutMs)
        {
            int retval;
            _ll.ConfigGetParameter(param, out retval, ordinal, timeoutMs);
            return retval;
        }


        /**
	 * Sets the period of the given status frame.
	 *
	 * @param statusFrame
	 *            Frame whose period is to be changed.
	 * @param periodMs
	 *            Period in ms for the given frame.
	 * @param timeoutMs
	 *            Timeout value in ms. If nonzero, function will wait for
   *            config success and report an error if it times out.
   *            If zero, no blocking or checking is performed.
	 * @return Error Code generated by function. 0 indicates no error.
	 */
        public ErrorCode SetStatusFramePeriod(CANifierStatusFrame statusFrame, int periodMs, int timeoutMs)
        {
            ErrorCode retval = _ll.SetStatusFramePeriod(statusFrame, (uint)periodMs, (uint)timeoutMs);
            return retval;
        }
        /**
         * Sets the period of the given status frame.
         *
         * @param statusFrame
         *            Frame whose period is to be changed.
         * @param periodMs
         *            Period in ms for the given frame.
         * @param timeoutMs
         *            Timeout value in ms. If nonzero, function will wait for
       *            config success and report an error if it times out.
       *            If zero, no blocking or checking is performed.
         * @return Error Code generated by function. 0 indicates no error.
         */
        public ErrorCode SetStatusFramePeriod(int statusFrame, int periodMs, int timeoutMs)
        {
            ErrorCode retval = _ll.SetStatusFramePeriod((CANifierStatusFrame)statusFrame, (uint)periodMs, (uint)timeoutMs);
            return retval;
        }

        /**
         * Gets the period of the given status frame.
         *
         * @param frame
         *            Frame to get the period of.
         * @param timeoutMs
         *            Timeout value in ms. If nonzero, function will wait for
       *            config success and report an error if it times out.
       *            If zero, no blocking or checking is performed.
         * @return Period of the given status frame.
         */
        public int GetStatusFramePeriod(CANifierStatusFrame frame, int timeoutMs)
        {
            int period;
            _ll.GetStatusFramePeriod(frame, out period, timeoutMs);
            return period;
        }

        /**
         * Sets the period of the given control frame.
         *
         * @param frame
         *            Frame whose period is to be changed.
         * @param periodMs
         *            Period in ms for the given frame.
         * @return Error Code generated by function. 0 indicates no error.
         */
        public ErrorCode SetControlFramePeriod(CANifierControlFrame frame, int periodMs)
        {
            ErrorCode retval = _ll.SetControlFramePeriod(frame, periodMs);
            return retval;
        }
        /**
         * Sets the period of the given control frame.
         *
         * @param frame
         *            Frame whose period is to be changed.
         * @param periodMs
         *            Period in ms for the given frame.
         * @return Error Code generated by function. 0 indicates no error.
         */
        public ErrorCode SetControlFramePeriod(int frame, int periodMs)
        {
            ErrorCode retval = _ll.SetControlFramePeriod((CANifierControlFrame)frame, periodMs);
            return retval;
        }


        /**
	 * Gets the firmware version of the device.
	 *
	 * @return Firmware version of device.
	 */
        public int GetFirmwareVersion()
        {
            return _ll.GetFirmwareVersion();
        }

        /**
         * Returns true if the device has reset since last call.
         *
         * @return Has a Device Reset Occurred?
         */
        public bool HasResetOccurred()
        {
            return _ll.HasResetOccured();
        }

        // ------ Faults ----------//
        /**
         * Gets the CANifier fault status
         *
         * @param toFill
         *            Container for fault statuses.
         * @return Error Code generated by function. 0 indicates no error.
         */
        public ErrorCode GetFaults(CANifierFaults toFill)
        {
            ErrorCode bits = _ll.GetFaults(toFill);
            return GetLastError();
        }
        /**
         * Gets the CANifier sticky fault status
         *
         * @param toFill
         *            Container for sticky fault statuses.
         * @return Error Code generated by function. 0 indicates no error.
         */
        public ErrorCode GetStickyFaults(CANifierStickyFaults toFill)
        {
            ErrorCode bits = _ll.GetStickyFaults(toFill);
            return GetLastError();
        }
        /**
         * Clears the Sticky Faults
         *
         * @return Error Code generated by function. 0 indicates no error.
         */
        public ErrorCode ClearStickyFaults(int timeoutMs)
        {
            ErrorCode retval = _ll.ClearStickyFaults(timeoutMs);
            return retval;
        }

        public ErrorCode GetLastError()
        {
            return _ll.GetLastError();
        }

        /**
	 * Gets the bus voltage seen by the device.
	 *
	 * @return The bus voltage value (in volts).
	 */
        public float GetBusVoltage()
        {
            return _ll.GetBatteryVoltage();
        }

        /**
         * @return The Device Number
         */
        public int GetDeviceID()
        {
            return (int)_ll.GetDeviceNumber();
        }
    }
}
