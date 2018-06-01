/*
*  Software License Agreement
*
* Copyright (C) Cross The Road Electronics.  All rights
* reserved.
* 
* Cross The Road Electronics (CTRE) licenses to you the right to 
* use, publish, and distribute copies of CRF (Cross The Road) firmware files (*.crf) and Software
* API Libraries ONLY when in use with Cross The Road Electronics hardware products.
* 
* THE SOFTWARE AND DOCUMENTATION ARE PROVIDED "AS IS" WITHOUT
* WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT
* LIMITATION, ANY WARRANTY OF MERCHANTABILITY, FITNESS FOR A
* PARTICULAR PURPOSE, TITLE AND NON-INFRINGEMENT. IN NO EVENT SHALL
* CROSS THE ROAD ELECTRONICS BE LIABLE FOR ANY INCIDENTAL, SPECIAL, 
* INDIRECT OR CONSEQUENTIAL DAMAGES, LOST PROFITS OR LOST DATA, COST OF
* PROCUREMENT OF SUBSTITUTE GOODS, TECHNOLOGY OR SERVICES, ANY CLAIMS
* BY THIRD PARTIES (INCLUDING BUT NOT LIMITED TO ANY DEFENSE
* THEREOF), ANY CLAIMS FOR INDEMNITY OR CONTRIBUTION, OR OTHER
* SIMILAR COSTS, WHETHER ASSERTED ON THE BASIS OF CONTRACT, TORT
* (INCLUDING NEGLIGENCE), BREACH OF WARRANTY, OR OTHERWISE
*/

using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.Controller
{
    public class Xbox360Gamepad : AbstractLocalGamepad
    {
        private CTRE.Phoenix.Controller.GameControllerValues _gv = new CTRE.Phoenix.Controller.GameControllerValues();
        private uint _oldBtns = 0;
        private int _oldPov = 0;
        private bool _modeButtonOn = false;
        private bool _lowbattery = false;
        private bool _vibrateEnabled = true;

        private int _timesConnected = 0;
        private bool _UsbWorkaroundIsEffect = false;

        public Xbox360Gamepad(Controller.IGameControllerValuesProvider provider, uint idx) : base(provider, idx)
        {
        }

        public override bool IsConnected()
        {
            return _timesConnected > 10; // TODO: confirm if this is necessary, fix HERO firmware if need be.
        }

        /**
         * Called Once per app loop
         */
        public override void Process()
        {
            /* Check to see if controller is connected */
            if (GetConnectionStatus() == CTRE.Phoenix.UsbDeviceConnection.Connected)
                if (_timesConnected < 9999)
                    ++_timesConnected;
                else
                    _timesConnected = 0;

            /* Controller is connected*/
            if (_timesConnected > 0)
            {
                /* save the old one */
                _oldBtns = _gv.btns;
                _oldPov = _gv.pov;

                /* get latest */
                GetAllValues(ref _gv);
                if (_gv.vendorSpecI == null)
                {
                    /* something is wrong */
                    _timesConnected = 0;
                }
            }

            if (_timesConnected > 0)
            {
                if ((_gv.flagBits & 1) != 0)
                    _UsbWorkaroundIsEffect = true;
                else
                    _UsbWorkaroundIsEffect = false;
            }

            /* mode button */
            if (_timesConnected > 0)
            {
                if ((_gv.vendorSpecI[2] & 0x8) == 0) { _modeButtonOn = false; }
                else { _modeButtonOn = true; }
            }

            /* low voltage cuttoff */
            if (_timesConnected > 0)
            {
                if ((_gv.vendorSpecI[2] & 0x2) == 0) { _lowbattery = false; }
                else { _lowbattery = true; }
            }

            /* low voltage cuttoff */
            if (_timesConnected > 0)
            {
                if ((_gv.vendorSpecI[2] & 0x20) == 0) { _vibrateEnabled = false; }
                else { _vibrateEnabled = true; }
            }

            /* clear everything if disconnected*/
            if (_timesConnected <= 0)
            {
                _modeButtonOn = false;	/* default value */
                _vibrateEnabled = true;	/* default value */
                _lowbattery = false;	/* default value */
                _oldBtns = 0;
                _gv.btns = 0;
                _oldPov = 0;
                _gv.pov = 0;
            }
        }

        /**
         * Maps Xbox outputs 
         */
        private uint LogToXboxBtnIdx(uint idx)
        {
            //----dinput------
            //    4
            //  1   3
            //    2
            //5  6  (shoulder)
            //9  10 (back start)
            //11 12 (center press)

            //----xinput------
            //    4
            //  3   2
            //    1
            //5 6   (shoulder)
            //7 8   (back start)
            //9 10  (center press)
            //11    (guide)

            switch (idx)
            {
                case 1: return 3;
                case 2: return 1;
                case 3: return 2;
                case 4: return 4;
                //shoulder
                case 5: return 5;
                case 6: return 6;
                //back start
                case 9: return 7;
                case 10: return 8;
                //center press
                case 11: return 9;
                case 12: return 10;
            }
            return 15; // no button
        }

        /**
         * Grabs a single button press
         */
        override public bool GetButtonEvent(uint idx)
        {
			/* Not sure if we need this, but it remaps the Xbox Controller to a 'D' Input controller */
            //idx = LogToXboxBtnIdx(idx);

            if (idx < 0) 	/* caller can change button defs to -1 to remove certain features */
                return false;
            if (idx == 0)
            {
                Reporting.Log(ErrorCode.InvalidParamValue, Button0Error, 0, "");
                return false;
            }
            if (idx > 0)
                --idx;
            uint old = (_oldBtns >> (int)idx) & 1;
            uint latest = (_gv.btns >> (int)idx) & 1;

            if ((0 == old) && (1 == latest))
                return true;
            return false;
        }

        /**
         * Checks to see if button is being held/pressed down
         */
        override public bool IsButtonLow(uint idx)
        {
            //idx = LogToXboxBtnIdx(idx);
            if (idx == 0)
            {
                Reporting.Log(ErrorCode.InvalidParamValue, Button0Error, 0, "");
                return false;
            }
            if (idx > 0)
                --idx;
            uint latest = (_gv.btns >> (int)idx) & 1;

            if ((1 == latest))
                return true;
            return false;
        }

        /**
         * Check to see if Mode button has been enabled
         */
        override public bool ModeButtonEnabled()
        {
            return _modeButtonOn;
        }
		/**
		* Check to see if Vibration button has been enabled
		*/
        override public bool VibrateButtonEnabled()
        {
            return _vibrateEnabled;
        }
        /**
         * Check to see if controller is low battery
         */
        override public bool LowVoltageDetected()
        {
            return _lowbattery;
        }
        public int GetPov()
        {
            if (_oldPov != _gv.pov)
                return _gv.pov;
            return 0;
        }

        /**
         * Return Axis, Y axis needs to be flipped with -1
         */
        override public float GetStick(uint axis)
        {
			/* Should I return enumerations or just the raw stick ??? */
            if (axis == kAxis_LeftX)
                return base.GetAxis(0);
            if (axis == kAxis_LeftY)
                return -1f * base.GetAxis(1);
            if (axis == kAxis_RightX)
                return base.GetAxis(2);
            if (axis == kAxis_RightY)
                return -1f * base.GetAxis(3);
            if (axis == kAxis_LeftShoulder)
                return base.GetAxis(4);
            if (axis == kAxis_RightShoulder)
                return base.GetAxis(5);

            return 0;
        }
        override public uint GetPid()
        {
            return _gv.pid;
        }
        override public bool IsUsbWorkaroundIsEffect()
        {
            return _UsbWorkaroundIsEffect;
        }
    }
}
