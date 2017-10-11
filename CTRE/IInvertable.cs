using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix
{
    public interface IInvertable
    {
        void SetInverted(bool invert);
        bool GetInverted();
    }
}