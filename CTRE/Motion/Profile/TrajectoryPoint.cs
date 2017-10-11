using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.Motion
{
    public class TrajectoryPoint
    {
        public float position;
        public float velocity;

        public float heading;

        public UInt32 timeDurMs;
            
        public UInt32 flags;
    }
}
