using System;
using Microsoft.SPOT;
using System.Collections;
using CTRE.Phoenix.MotorControl;

namespace CTRE.Phoenix
{
    internal static class DeviceCatalog
    {
        static ArrayList _mcs = new ArrayList();

        internal static void Register(IMotorController motorController)
        {
            _mcs.Add(motorController);
        }

        internal static void Unregister(IMotorController motorController)
        {
            _mcs.Remove((object)motorController);
        }

        internal static int MotorControllerCount
        {
            get { return _mcs.Count; }
        }

        internal static IMotorController Get(int idx)
        {
            return (IMotorController)_mcs[idx];
        }
    }
}