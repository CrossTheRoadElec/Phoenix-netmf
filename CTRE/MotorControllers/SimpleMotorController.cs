/* We still need to make this into an interface */

using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.MotorControllers
{
    public class SimpleMotorController : IMotorController
    {
        private BasicControlMode m_controlMode = BasicControlMode.kPercentVbus;

        /* Various values sent through CAN to motor */
        private enum SimpleControlMode
        {
            kThrottle = 0,
            kVoltageMode = 4,
            kFollowerMode = 5,
            kDisabled = 15,
        };
        private SimpleControlMode m_sendMode;

        /* Device ID */
        UInt32 _deviceNumber;

        /* Talon Implementation??? */
        CTRE.Phoenix.LowLevel_TalonSrx m_impl;

        /* Global verison of set value */
        private float m_setPoint = 0;

        /* Is motor inverted */
        bool _IsInverted;

        /* Status for all of Basic motor operations */
        int status = 0;
        /* Previous status, set within status handler */
        private int _lastStatus = 0;
        /* Bhecks to see if CAN is alright, never set though */
        const int CAN_OK = 0;
        /* PID profile, but don't really care about at the moment */
        private uint m_profile = 0;

        /** Constructor for basic motor */
        public SimpleMotorController(int deviceNumber, bool externalEnable = false)
        {
            _deviceNumber = (UInt32)deviceNumber;
            m_impl = new CTRE.Phoenix.LowLevel_TalonSrx((ushort)deviceNumber, externalEnable);
            ApplyControlMode(m_controlMode);
            m_impl.SetProfileSlotSelect(m_profile);
        }

        public void SetControlMode(BasicControlMode mode)
        {
            if (m_controlMode == mode)
            {
                /* we already are in this mode, don't perform disable workaround */
            }
            else
            {
                ApplyControlMode(mode);
            }
        }

        public UInt32 GetDeviceNumber()
        {
            return _deviceNumber;
        }

        /** Inverts the input to the motor */
        public void SetInverted(bool invert)
        {
            _IsInverted = invert;
        }
        public bool GetInverted()
        {
            return _IsInverted;
        }

        /** Set for Basic motor for the two given modes */
        public void Set(float value)
        {
            /* cache set point for GetSetpoint() */
            m_setPoint = value;
            switch (m_controlMode)
            {
                case BasicControlMode.kPercentVbus:
                    {
                        m_impl.Set(_IsInverted ? -value : value, (int)m_sendMode);
                        status = 0;
                    }
                    break;
                case BasicControlMode.kVoltage:
                    {
                        // Voltage is an 8.8 fixed point number.
                        int volts = (int)((_IsInverted ? -value : value) * 256);
                        status = m_impl.SetDemand(volts, (int)m_sendMode);
                    }
                    break;
                case BasicControlMode.kFollower:
                    {
                        status = m_impl.SetDemand((int)value, (int)m_sendMode);
                    }
                    break;
            }
        }

        /**
         * Set the maximum voltage change rate.  This ramp rate is in affect regardless
         * of which control mode
         * the TALON is in.
         *
         * When in PercentVbus or Voltage output mode, the rate at which the voltage
         * changes can
         * be limited to reduce current spikes.  Set this to 0.0 to disable rate
         * limiting.
         *
         * @param rampRate The maximum rate of voltage change in Percent Voltage mode in
         * V/s.
         */
        public void SetVoltageRampRate(float rampRate)
        {
            /* Caller is expressing ramp as Voltage per sec, assuming 12V is full.
                    Talon's throttle ramp is in dThrot/d10ms.  1023 is full fwd, -1023 is
               full rev. */
            int rampRatedThrotPer10ms = 0;
            if (rampRate <= 0)
            {
                /* caller wants to disable feature */
            }
            else
            {
                /* desired ramp rate is positive and nonzero */
                rampRatedThrotPer10ms = (int)((rampRate * 1023.0f / 12.0f) / 100f);
                if (rampRatedThrotPer10ms == 0)
                    rampRatedThrotPer10ms = 1; /* slowest ramp possible */
                else if (rampRatedThrotPer10ms > 255)
                    rampRatedThrotPer10ms = 255; /* fastest nonzero ramp */
            }
            int status = m_impl.SetRampThrottle(rampRatedThrotPer10ms);
            HandleStatus(status);
        }
        public void SetVoltageCompensationRampRate(float rampRate)
        {
            /* when in voltage compensation mode, the voltage compensation rate
              directly caps the change in target voltage */
            int status = CAN_OK;
            status = m_impl.SetVoltageCompensationRate(rampRate / 1000);
            HandleStatus(status);
        }

        public int ConfigPeakOutputVoltage(float forwardVoltage, float reverseVoltage, uint timeoutMs = 0)
        {
            int status1 = CAN_OK, status2 = CAN_OK;
            /* bounds checking */
            if (forwardVoltage > 12)
                forwardVoltage = 12;
            else if (forwardVoltage < 0)
                forwardVoltage = 0;
            if (reverseVoltage > 0)
                reverseVoltage = 0;
            else if (reverseVoltage < -12)
                reverseVoltage = -12;
            /* config calls */
            status1 = ConfigSetParameter(LowLevel_TalonSrx.ParamEnum.ePeakPosOutput, 1023f * forwardVoltage / 12.0f, timeoutMs);
            status2 = ConfigSetParameter(LowLevel_TalonSrx.ParamEnum.ePeakNegOutput, 1023f * reverseVoltage / 12.0f, timeoutMs);
            /* return the worst one */
            if (status1 == CAN_OK)
                status1 = status2;
            return status1;
        }
        public int ConfigNominalOutputVoltage(float forwardVoltage, float reverseVoltage, uint timeoutMs = 0)
        {
            int status1 = CAN_OK, status2 = CAN_OK;
            /* bounds checking */
            if (forwardVoltage > 12)
                forwardVoltage = 12;
            else if (forwardVoltage < 0)
                forwardVoltage = 0;
            if (reverseVoltage > 0)
                reverseVoltage = 0;
            else if (reverseVoltage < -12)
                reverseVoltage = -12;
            /* config calls */
            status1 = ConfigSetParameter(LowLevel_TalonSrx.ParamEnum.eNominalPosOutput, 1023f * forwardVoltage / 12.0f, timeoutMs);
            status2 = ConfigSetParameter(LowLevel_TalonSrx.ParamEnum.eNominalNegOutput, 1023f * reverseVoltage / 12.0f, timeoutMs);
            /* return the worst one */
            if (status1 == CAN_OK)
                status1 = status2;
            return status1;
        }

        public float GetBusVoltage()
        {
            float voltage;
            int status = m_impl.GetBatteryV(out voltage);
            HandleStatus(status);
            return voltage;
        }
        /**
         * General set frame.  Since the parameter is a general integral type, this can
         * be used for testing future features.
         */
        public int ConfigSetParameter(CTRE.Phoenix.LowLevel_TalonSrx.ParamEnum paramEnum, float value, uint timeoutMs = 0)
        {
            int status;
            /* config peak throttle when in closed-loop mode in the positive direction. */
            status = m_impl.SetParam(paramEnum, value, timeoutMs);
            return HandleStatus(status);
        }
        public void ConfigSetParameter(UInt32 paramEnum, int value, uint timeoutMs = 0)
        {
            int status;
            /* config peak throttle when in closed-loop mode in the positive direction. */
            status = m_impl.SetParam((LowLevel_TalonSrx.ParamEnum)paramEnum, value, timeoutMs);
            HandleStatus(status);
        }

        /** Applies the control mode */
        private void ApplyControlMode(BasicControlMode mode)
        {
            m_controlMode = mode;

            switch (mode)
            {
                case BasicControlMode.kPercentVbus:
                    m_sendMode = SimpleControlMode.kThrottle;
                    break;
                case BasicControlMode.kVoltage:
                    m_sendMode = SimpleControlMode.kVoltageMode;
                    break;
                case BasicControlMode.kFollower:
                    m_sendMode = SimpleControlMode.kFollowerMode;
                    break;
            }
            // Keep the talon disabled until Set() is called.
            int status = m_impl.SetModeSelect((int)SimpleControlMode.kDisabled);
            HandleStatus(status);
        }

        /* Handle the status, returns error if we have one */
        private int HandleStatus(int status)
        {
            /* error handler */
            if (status != 0)
            {
                Reporting.SetError(status, Reporting.getHALErrorMessage(status));
            }
            /* mirror last status */
            _lastStatus = status;
            return _lastStatus;
        }

        //--- IFollowerMotorController --- //
        void IFollower.Follow(Object masterToFollow)
        {
            /* Not really doing anything as this is not a thing */
            SimpleMotorController master = (SimpleMotorController)masterToFollow;
            /* set it up */
            SetControlMode(BasicControlMode.kFollower);
            Set(master.GetDeviceNumber());
        }
        void IFollower.ValueUpdated()
        {
            //do nothing
        }
    }
}