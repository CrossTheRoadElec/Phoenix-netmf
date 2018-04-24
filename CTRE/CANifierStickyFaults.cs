using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix
{
    public struct CANifierStickyFaults
    {
        //!< True iff any of the above flags are true.
        bool HasAnyFault()  {
		return false;
    }
    int ToBitfield()  {
		int retval = 0;
		return retval;
	}

    CANifierStickyFaults(int bits){}

};
}
