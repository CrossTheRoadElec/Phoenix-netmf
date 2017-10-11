using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.Tasking
{
    public interface ILoopable
    {
        void OnStart();
        void OnLoop();
        bool IsDone();
        void OnStop();
        String ToString();
    }
}
