using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.LowLevel
{
    public struct ResetStats
    {
        public Int32 resetCount;
        public Int32 resetFlags;
        public Int32 firmVers;
        public bool hasReset;
    };
}
