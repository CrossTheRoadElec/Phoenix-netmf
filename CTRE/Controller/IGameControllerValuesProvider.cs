using System;

namespace CTRE.Phoenix.Controller
{
    /** 
     * Interface for anything that provides gamepad/joystick values (could be from a host pc or from USB attached gamepad). 
     * @return  Negative If values could not be retrieved due to connection issue.  toFill is cleared.
                Zero if values are stale (no new data). toFill is left untouched.
     *          Positive if values are updated. toFill is filled in.
     */
    public interface IGameControllerValuesProvider
    {
        int Get(ref GameControllerValues toFill, uint idx);
        int Sync(ref GameControllerValues toFill, uint rumbleL, uint rumbleR, uint ledCode, uint controlFlags, uint idx);
        void SetRef(GameController reference, uint idx);
    }
}
