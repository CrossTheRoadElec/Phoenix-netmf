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
    public class LogitechGamepad : AbstractLocalGamepad
    {
        private CTRE.Phoenix.Controller.GameControllerValues _gv = new CTRE.Phoenix.Controller.GameControllerValues();
        private uint _oldBtns = 0;
        private int _oldPov = 0;
        private bool _modeButtonOn = false;

        public LogitechGamepad(Controller.IGameControllerValuesProvider provider, uint idx) : base(provider, idx)
        {
        }

        public override bool IsConnected()
        {
            return (GetConnectionStatus() == CTRE.Phoenix.UsbDeviceConnection.Connected);
        }

        /// <summary>
        /// Called once per app loop.
        /// </summary>
        public override void Process()
        {

            if (IsConnected())
            {
                /* save the old one */
                _oldBtns = _gv.btns;
                _oldPov = _gv.pov;

                /* get latest */
                GetAllValues(ref _gv);

                if (_gv.vendorSpecI[2] == 0x74)
                {
                    /* mode is off */
                    _modeButtonOn = false;
                }
                else if (_gv.vendorSpecI[2] == 0x7C)
                {
                    /* mode is on */
                    _modeButtonOn = true;
                }
                else
                {
                    /* unknown */
                    _modeButtonOn = false;
                }
            }
            else
            {
                /* clear everything */
                _modeButtonOn = false;
                _oldBtns = 0;
                _gv.btns = 0;
                _oldPov = 0;
                _gv.pov = 0;
            }
        }
        public override bool GetButtonEvent(uint idx)
        {
            if (idx < 0) /* caller can change button defs to -1 to remove certain features */
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
        public override bool IsButtonLow(uint idx)
        {
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
        override public bool ModeButtonEnabled()
        {
            return _modeButtonOn;
        }
        override public bool VibrateButtonEnabled()
        {
            return false;
        }
        public override bool LowVoltageDetected()
        {
            return false;
        }
        public int GetPov()
        {
            if (_oldPov != _gv.pov)
                return _gv.pov;
            return 0;
        }
        override public float GetStick(uint axis)
        {
            return base.GetAxis(axis);
        }
        override public uint GetPid()
        {
            return _gv.pid;
        }
    }
}
