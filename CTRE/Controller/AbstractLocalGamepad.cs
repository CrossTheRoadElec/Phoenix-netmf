/* Don't think we want this class */

using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.Controller
{
    public abstract class AbstractLocalGamepad : CTRE.Phoenix.Controller.GameController
    {
        public const int kAxis_LeftX = 0;
        public const int kAxis_LeftY = 1;
        public const int kAxis_RightX = 2;
        public const int kAxis_RightY = 5;
        public const int kAxis_LeftShoulder = 3;
        public const int kAxis_RightShoulder = 4;

        public abstract bool IsConnected();
        public abstract void Process();
        public abstract bool GetButtonEvent(uint idx);
        public abstract bool IsButtonLow(uint idx);
   
        public abstract float GetStick(uint axis);
        public abstract uint GetPid();

        abstract public bool ModeButtonEnabled(); // tuck into logi
 
        abstract public bool VibrateButtonEnabled(); // tuck into logi

        abstract public bool LowVoltageDetected(); // tuck into logi

        public AbstractLocalGamepad(Controller.IGameControllerValuesProvider provider, uint idx) : base(provider, idx)
        {           
        }

        virtual public bool IsUsbWorkaroundIsEffect()
        {
            /* default to no support */
            return false;
        }
    }
}
