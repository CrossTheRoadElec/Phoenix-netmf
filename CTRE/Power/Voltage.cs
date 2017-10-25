using System;
using Microsoft.SPOT;
using CTRE.Phoenix.MotorControl;

namespace CTRE.Phoenix.Power
{
    public static class Voltage {
        public enum Chemistry {
            NotSelected = 0,
            LeadAcid_12V,
            NiMH_12V,
        }
        public static Chemistry SelectedChemistry { get;set;}

        static int _dnCnt = 0;
        static int _upCnt = 0;
        static bool batIsLow = false;

        public static float BatteryVoltage
        {
            get
            {
                float retval = 0;
                /* grab first device battery voltage */
                if (DeviceCatalog.MotorControllerCount != 0)
                {
                    DeviceCatalog.Get(1).GetBusVoltage(out retval);
                }
                return retval;
            }
        }

        public static bool IsBatteryLow()
        {
            switch (SelectedChemistry) {
                case Chemistry.NotSelected:
                    return false;
                case Chemistry.NiMH_12V:
                    float vbat;

                    /* Battery voltage grabbed from LeftFront Talon SRX */
                    vbat = BatteryVoltage; /* modify this back to OG */

                    if (vbat > 10.50)
                    {
                        /* Reset down count */
                        _dnCnt = 0;
                        if (_upCnt < 100)
                            ++_upCnt;
                    }
                    else if (vbat < 10.00)
                    {
                        /* Reset up count */
                        _upCnt = 0;
                        if (_dnCnt < 100)
                            ++_dnCnt;
                    }

                    /* Returns bool low battery */
                    if (_dnCnt > 50)
                        batIsLow = true;    /* Battery under 10.0V for 50 counts */
                    else if (_upCnt > 50)
                        batIsLow = false;   /* Battery above 10.5V for 50 counts */
                    else
                    {
                        /* Don't change filter output */
                    }
                    return batIsLow;
                case Chemistry.LeadAcid_12V:
                    // TODO, use to BatteryVoltage
                    break;
            }

            return true;
        }

    }
}
