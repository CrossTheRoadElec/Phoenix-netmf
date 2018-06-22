using System;
using Microsoft.SPOT;

namespace CTRE
{
    public class Constants
    {
        public const int GetParamTimeoutMs = 4; //!< Minimum timeout for ConfigGet* API
        public const float DefaultDeadband = 0.04f; /* default neutral deadband when in open loop */
    }
}
