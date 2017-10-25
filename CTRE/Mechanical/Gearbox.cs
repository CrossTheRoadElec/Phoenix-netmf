using System;
using Microsoft.SPOT;
using CTRE.Phoenix.Signals;
using CTRE.Phoenix.MotorControl;

namespace CTRE.Phoenix.Mechanical
{
    /** This class is basically just a collection of simple motor controllers */
    public class Gearbox : Linkage
    {
        public Gearbox(IMotorController mc0) : base(mc0) { }
        public Gearbox(IMotorController master, IFollower[] followers) : base(master, followers) { }
        public Gearbox(IMotorController mc0, IFollower mc1) : base(mc0, mc1) { }
        public Gearbox(IMotorController mc0, IFollower mc1, IFollower mc2) : base(mc0, mc1, mc2) { }
        public Gearbox(IMotorController mc0, IFollower mc1, IFollower mc2, IFollower mc3) : base(mc0, mc1, mc2, mc3) { }
    }
}
