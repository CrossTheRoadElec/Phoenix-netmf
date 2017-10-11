/*
 *  Software License Agreement
 *
 * Copyright (C) Cross The Road Electronics.  All rights
 * reserved.
 * 
 * Cross The Road Electronics (CTRE) licenses to you the right to 
 * use, publish, and distribute copies of CRF (Cross The Road) binary firmware files (*.crf) 
 * and software example source ONLY when in use with Cross The Road Electronics hardware products.
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
using System.Collections;
using CTRE.Gadgeteer;
using CTRE.HERO;
using Microsoft.SPOT;

namespace CTRE.Phoenix.FRC
{
    public class DriverStation : IRobotStateProvider, Controller.IGameControllerValuesProvider, Tasking.ILoopable
    {
        //public HERO.Module.WiFiESP12F _wifiModule;
        byte[] _data;//Data for decoder
        bool _enabled;//Enabled or not
        bool _connected;//Connected to wifi module or not
        bool _updateFlag;//New data
        byte[] _sendingMessage;//Byte array to module
        static byte _len= 0;
        static int _iterator = 0;
        static UInt16 _eofA = 0;
        static UInt16 _checksum = 0;
        static CTRE.Phoenix.Containers.ByteRingBuffer buffer;
        /** Cache for reading out bytes in serial driver. */
        Stopwatch _timeout = new Stopwatch();
        Stopwatch _enableTimeout = new Stopwatch();
        Stopwatch _initialization = new Stopwatch();
        
        System.IO.Ports.SerialPort _uart;
        Microsoft.SPOT.Hardware.OutputPort _restart;
        Microsoft.SPOT.Hardware.OutputPort _flashPin;

        State _currentState;
        private CTRE.Phoenix.Controller.GameController[] _controllers;
        public CTRE.Phoenix.Controller.GameControllerValues[] _joysticks;
        public enum State
        {
            teleopDisabled,
            testDisabled,
            autonDisabled,
            blank,
            teleopEnabled,
            testEnabled,
            autonEnabled
        }

        static Processing processing = Processing.header;
        private enum Processing
        {
            header,
            length,
            payload,
            eof1,
            eof2,
            process
        }

        private static int status;

        public DriverStation(PortDefinition wifiPort)
        {
            if (wifiPort is IPortUart)
            {
                IPortUart p = (IPortUart)wifiPort;
                _uart = new System.IO.Ports.SerialPort(p.UART, 115200, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
                _restart = new Microsoft.SPOT.Hardware.OutputPort(p.Pin6, true);
                if (wifiPort is Port1Definition) _flashPin = new Microsoft.SPOT.Hardware.OutputPort(((Port1Definition)wifiPort).Pin3, true);
                if (wifiPort is Port4Definition) _flashPin = new Microsoft.SPOT.Hardware.OutputPort(((Port4Definition)wifiPort).Pin3, true);
                if (wifiPort is Port6Definition) _flashPin = new Microsoft.SPOT.Hardware.OutputPort(((Port6Definition)wifiPort).Pin3, true);
            }
            else
            {
                status = StatusCodes.PORT_MODULE_TYPE_MISMATCH;
                Reporting.SetError(status);
            }


            _uart.Open();
            _uart.Flush();


            _enabled = false;
            _connected = false;
            CTRE.Phoenix.Controller.GameControllerValues _g = new Controller.GameControllerValues();
            CTRE.Phoenix.Controller.GameControllerValues _h = new Controller.GameControllerValues();
            CTRE.Phoenix.Controller.GameControllerValues _j = new Controller.GameControllerValues();
            CTRE.Phoenix.Controller.GameControllerValues _k = new Controller.GameControllerValues();
            CTRE.Phoenix.Controller.GameControllerValues _l = new Controller.GameControllerValues();
            CTRE.Phoenix.Controller.GameControllerValues _m = new Controller.GameControllerValues();
            _joysticks = new CTRE.Phoenix.Controller.GameControllerValues[6] { _g, _h, _j, _k, _l, _m };
            _controllers = new CTRE.Phoenix.Controller.GameController[6];
            _data = new byte[255];
            _sendingMessage = new byte[1] { 0x00 };
            buffer = new Containers.ByteRingBuffer(255);
        }

        public void update()
        {
            if (status < 0)
            {
                Reporting.SetError(status);
                return;
            }
            _sendingMessage = combine(new byte[1] { 0x00 }, _sendingMessage);
            _uart.Write(_sendingMessage, 0, _sendingMessage.Length);
            _sendingMessage = new byte[0];
            _updateFlag = false;
            _sendingMessage = new byte[1];

            while (_uart.BytesToRead > 0)
            {
                _updateFlag = true;
                buffer.Push((byte)_uart.ReadByte());
            }
            
            //Process data until buffer is empty to ensure newest data
            while (!buffer.Empty)
            {
                switch (processing)
                {
                    //Header frame, make sure I've got an AA
                    case Processing.header:
                        _checksum = 0;
                        if (buffer.Pop() == 0xAA)
                        {
                            _checksum += 0xAA;
                            processing = Processing.length;
                        }
                        break;

                    //Length frame, find out how long the payload is
                    case Processing.length:
                        _len = buffer.Pop();
                        _checksum += _len;
                        _iterator = 0;
                        processing = Processing.payload;
                        break;

                    //Payload frame, assign payload to data array
                    case Processing.payload:
                        while (!buffer.Empty && _iterator < _len)
                        {
                            _data[_iterator] = buffer.Pop();
                            _checksum += _data[_iterator];
                            _iterator++;
                        }
                        if (_iterator == _len)
                        {
                            processing = Processing.eof1;
                        }
                        break;

                    //End of frame 1, record first checksum byte
                    case Processing.eof1:
                        _eofA = buffer.Pop();
                        processing = Processing.eof2;
                        break;

                    //End of frame 2, record second checksum byte, combine two, and ensure it's 0
                    case Processing.eof2:
                        _checksum += (UInt16)((UInt16)(_eofA << 8) + (UInt16)buffer.Pop());
                        if (_checksum == 0x00)
                        {
                            processing = Processing.process;
                            goto case Processing.process;
                        }
                        else
                        {
                            //Bad _checksum
                            processing = Processing.header;
                        }
                        break;
                    
                    //Process frame, assign payload data to joysticks
                    case Processing.process:

                        int joystickIndex = 0;//Initialize gamepad index
                        int index = 5;//Initialize data index just before gamepad data
                        _currentState = (State)_data[3];
                        _enabled = (_currentState == State.autonEnabled || _currentState == State.teleopEnabled || _currentState == State.testEnabled);
                        
                        //GamePad Data Parsing

                        float[] tempAxis = new float[6] { 0, 0, 0, 0, 0, 0 };
                        uint tempButtons = 0;
                        int tempHat = 0xffff;


                        while (_len - index > 8 && joystickIndex < 6)//Check if there's gamepad data & make sure there isn't more than 6 joysticks
                        {
                            if (_data[++index] == 0) break;

                            index += 2;//Push index into slot that checks number of axis
                            int numJoysticks = _data[index];
                            for (int i = 0; i < numJoysticks && i < 6; i++)//Run for loop for number of axis
                            {
                                tempAxis[i] = ((_data[++index] / 128f));
                                if (tempAxis[i] >= 1) tempAxis[i] = tempAxis[i] - 2;
                            }

                            index++;//Push index into slot that checks number of buttons
                            if (_data[index] > 0)
                                tempButtons = (uint)_data[++index] << 8 | _data[++index];

                            if (_data[++index] > 0)//Check to ensure there is/are hats
                            {
                                tempHat = (_data[++index] << 8) | _data[++index];//Assign hat value
                            }

                            _joysticks[joystickIndex].axes[0] = tempAxis[0];
                            _joysticks[joystickIndex].axes[1] = tempAxis[1];
                            _joysticks[joystickIndex].axes[2] = tempAxis[2];
                            _joysticks[joystickIndex].axes[3] = tempAxis[3];
                            _joysticks[joystickIndex].axes[4] = tempAxis[4];
                            _joysticks[joystickIndex].axes[5] = tempAxis[5];
                            _joysticks[joystickIndex].btns = tempButtons;
                            _joysticks[joystickIndex].pov = tempHat;
                            ++joystickIndex;
                        }
                        processing = Processing.header;
                        break;
                    
                    //If anything else, set to header
                    default:
                        processing = Processing.header;
                        break;
                }
                _timeout.Start();
            }
            
            if (_timeout.DurationMs > 5000)
            {
                _restart.Write(false);//Restart module if no data is coming
                _timeout.Start();
                _connected = false;
                for (int i = 0; i < 6; i++)
                {
                    _joysticks[i].axes[0] = 0;
                    _joysticks[i].axes[1] = 0;
                    _joysticks[i].axes[2] = 0;
                    _joysticks[i].axes[3] = 0;
                    _joysticks[i].axes[4] = 0;
                    _joysticks[i].axes[5] = 0;
                    _joysticks[i].btns = 0;
                    _joysticks[i].pov = 0xffff;
                }
            }
            else
            {
                _restart.Write(true);
                _connected = true;
            }

            if(_timeout.DurationMs > 500)
            {
                _enabled = false;
            }
            

            if (_enabled)
            {
                CTRE.Phoenix.Watchdog.Feed();
            }
            else
            {
                _enabled = false;
            }
        }
        

        public void SendBattery(float voltage)
        {
            if (status < 0)
            {
                Reporting.SetError(status);
                return;
            }
            byte p1 = (byte)(int)voltage;
            voltage -= (float)(p1 + 0.005);
            byte p2 = (byte)(int)((voltage * 10) * 255);
            _sendingMessage = combine(_sendingMessage, (new byte[] { 0x33, (byte)'b', 0x02, (byte)p1, (byte)p2 }));
        }

        public void SendIP(byte[] moduleIP, byte[] targetIP)
        {
            if (status < 0)
            {
                Reporting.SetError(status);
                return;
            }
            _sendingMessage = combine(_sendingMessage, new byte[] { 0x33, (byte)'i', (byte)(moduleIP.Length + targetIP.Length) });
            _sendingMessage = combine(_sendingMessage, moduleIP);
            _sendingMessage = combine(_sendingMessage, targetIP);
        }

        public void SendUDP(UInt16 port, byte[] data)
        {
            if (status < 0)
            {
                Reporting.SetError(status);
                return;
            }
            if (data.Length < 200)
            {
                _sendingMessage = combine(_sendingMessage, new byte[] { 0x33, (byte)'u', (byte)(data.Length + 2) });

                byte[] b = BitConverter.GetBytes(port);
                _sendingMessage = combine(_sendingMessage, b);
                _sendingMessage = combine(_sendingMessage, data);
            }
            else
            {
                throw new Exception("Data length too long");
            }
        }
        private byte[] combine(byte[] a, byte[] b)
        {
            byte[] c = new byte[a.Length + b.Length];
            Array.Copy(a, c, a.Length);
            Array.Copy(b, 0, c, a.Length, b.Length);
            return c;
        }

        /** IRobotState Provider */
        public bool IsConnected()
        {
            if (status < 0)
            {
                Reporting.SetError(status);
                return false;
            }
            return _connected;
        }
        public bool IsEnabled()
        {
            if (status < 0)
            {
                Reporting.SetError(status);
                return false;
            }
            return _enabled;
        }
        public bool IsAuton()
        {
            if (status < 0)
            {
                Reporting.SetError(status);
                return false;
            }
            return _currentState == State.autonDisabled || _currentState == State.autonEnabled;
        }
        public State GetState()
        {
            if (status < 0)
            {
                Reporting.SetError(status);
                return State.blank;
            }
            return _currentState;
        }

        public String GetConnectionStatus()
        {
            if (status < 0)
            {
                Reporting.SetError(status);
                return "Wrong Port";
            }
            if (_connected) return "Connected";
            else return "Not Connected";
        }

        /**IGameControllerProvider*/
        public int Get(ref CTRE.Phoenix.Controller.GameControllerValues toFill, uint idx)
        {
            if (idx >= 0 && idx <= 5)
                return SyncGet(ref toFill, idx);
            else return 0;
        }
        public int Sync(ref CTRE.Phoenix.Controller.GameControllerValues toFill, uint rumbleL, uint rumbleR, uint ledCode, uint controlFlags, uint idx)
        {
            if (idx >= 0 && idx <= 5)
                return SyncGet(ref toFill, idx);
            else return 0;
        }

        public CTRE.Phoenix.Controller.GameController getController(uint idx)
        {
            if (idx >= 0 && idx <= 5)
                return _controllers[idx];
            else
                return null;
        }

        public void SetRef(CTRE.Phoenix.Controller.GameController reference, uint idx)
        {
            if(idx >= 0 && idx <= 5)
                _controllers[idx] = reference;
        }

        private int SyncGet(ref CTRE.Phoenix.Controller.GameControllerValues toFill, uint idx)
        {
            /* always get latest data for now */
            if (_updateFlag)
            {
                toFill = _joysticks[idx];
            }
            return _updateFlag ? 1 : 0;
        }

        /**ILoopable*/
        public void OnStart()
        {
            _timeout.Start();
            _enableTimeout.Start();
            _initialization.Start();
        }

        public void OnLoop()
        {
            update();
        }

        public bool IsDone()
        {
            return false;
        }

        public void OnStop()
        {
            //Driverstation can't stop
        }
    }
}
