using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.Tasking
{
    public interface IAbstractScheduler
    {
        void Add(ILoopable aLoop);
        void RemoveAll();
        ILoopable GetCurrent();

        void Start();
        void Stop();
    }
}
