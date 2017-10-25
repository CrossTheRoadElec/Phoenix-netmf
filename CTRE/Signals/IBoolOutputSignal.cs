using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.Signals
{
    public interface IBoolOutputSignal
    {
        void Set(bool value);
    }
}