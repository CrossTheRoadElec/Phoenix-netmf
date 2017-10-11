using System;
using Microsoft.SPOT;
using System.Collections;

namespace CTRE.Phoenix.MotorControllers
{
    internal static class GroupMotorControllers
    {
        static ArrayList _mcs = new ArrayList();

        internal static void Register(IMotorController motorController)
        {
            _mcs.Add(motorController);
        }

        internal static int MotorControllerCount {
            get {
                return _mcs.Count;
            }
        }
        internal static IMotorController Get(int idx)
        {
            return (IMotorController)_mcs[idx];
        }
    }
}