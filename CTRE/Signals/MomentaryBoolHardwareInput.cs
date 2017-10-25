using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.Signals
{
    internal class MomentaryBoolHardwareInput : IBoolInputSignal
    {
        Microsoft.SPOT.Hardware.InputPort _input;

        public MomentaryBoolHardwareInput(Microsoft.SPOT.Hardware.InputPort input)
        {
            _input = input;
        }
        public bool Value
        {
            get
            {
                return _input.Read();
            }
        }
    }
}
