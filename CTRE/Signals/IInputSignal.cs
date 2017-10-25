using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.Signals
{
    public interface IInputSignal
    {
        float Value { get; }
    }
}
