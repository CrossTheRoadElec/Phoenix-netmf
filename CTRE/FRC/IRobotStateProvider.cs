using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.FRC
{
    public interface IRobotStateProvider
    {
        bool IsConnected();
        bool IsEnabled();
        bool IsAuton();
    }
}
