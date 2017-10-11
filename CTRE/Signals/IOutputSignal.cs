using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.Signals
{
    public interface IOutputSignal
    {
        void Set(float value);
    }
}