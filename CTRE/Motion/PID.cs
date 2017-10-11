using System;
using Microsoft.SPOT;
using CTRE.Phoenix.Signals;
using CTRE.Phoenix.Tasking;

namespace CTRE.Phoenix.Motion
{
    public class PID : IProcessable, ILoopable
    {
        /**
         * @param inputSig process variable
         */
        public PID(IInputSignal inputSig)
        {

        }
        /**
         * @param inputSig process variable
         * @param derivativeOfInputSig time derivative of Input sig.
         */
        public PID(IInputSignal inputSig, IInputSignal derivativeOfInputSig)
        {

        }

        public void AddOutput(IOutputSignal outputSig)
        {
            
        }
        public void RemoveOutputs()
        {

        }
        public float GetOutputValue()
        {
            return 0;
        }

        public void Process()
        {
            // do it
        }

        public void OnStart() { }

        public void OnLoop() { Process(); }

        public bool IsDone() { return false; }

        public void OnStop() { }
    }
}
