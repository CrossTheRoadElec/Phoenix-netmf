using System;
using Microsoft.SPOT;
using CTRE.Phoenix.MotorControllers;
using CTRE.Phoenix.Signals;

namespace CTRE.Phoenix.Mechanical
{
    /** This class is basically just a collection of simple motor controllers */
    public class Gearbox : Linkage
    {
        public Gearbox(MotorControllers.IMotorController mc1) : base(mc1) { }
        public Gearbox(MotorControllers.IMotorController mc1, MotorControllers.IFollower mc2) : base(mc1, mc2) { }
        public Gearbox(MotorControllers.IMotorController mc1, MotorControllers.IFollower mc2, MotorControllers.IFollower mc3) : base(mc1, mc2, mc3) { }
        public Gearbox(MotorControllers.IMotorController mc1, MotorControllers.IFollower mc2, MotorControllers.IFollower mc3, MotorControllers.IFollower mc4) : base(mc1, mc2, mc3, mc4) { }
    }
}
