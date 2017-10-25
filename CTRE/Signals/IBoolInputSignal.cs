using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.Signals
{
    public interface IBoolInputSignal
    {
        bool Value { get; }
    }
}
