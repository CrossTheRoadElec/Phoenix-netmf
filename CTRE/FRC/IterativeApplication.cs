using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix.FRC
{
    public class IterativeApplication : RobotApplication, IRobotStateProvider
    {
        public virtual bool IsEnabled() { return false; }
        public virtual bool IsAuton() { return false; }

        /* implement HasRobotState, but allow child class to override the routines */
        public bool IsConnected() { return true; }
        public virtual void DisabledInit() { }
        public virtual void TeleopInit() { }
        public virtual void AutonInit() { }

        /* holds refernce to the object that determines the enabled/disabled/teleop state of the robot. */
        IRobotStateProvider _robotSt;

        public virtual void DisabledPeriodic() { }
        public virtual void TeleopPeriodic() { }
        public virtual void AutonPeriodic() { }

        private const int kLoopTimeMs = 20;

        public IterativeApplication()
        {
            /* on construct, assume we use local implementations (or child-derived) */
            _robotSt = this;
        }

        /**
         * Allow caller to select other robot state providers, like WiFi, DriverStation, etc...
         */
        public IRobotStateProvider SelectedRobotStateProvider
        {
            set
            {
                _robotSt = value;
            }
            get
            {
                return _robotSt;
            }
        }

        public override sealed void RunForever()
        {
            /*first repeat call init until true */
            bool flag = false;
            while (!flag)
            {
                flag = RobotInit();
                System.Threading.Thread.Sleep(kLoopTimeMs);
            }
            /* now the loops */
            while (true)
            {
                bool en = IsEnabled();
                bool au = IsAuton();

                if (!en)
                {
                    DisabledInit();
                    while (!IsEnabled())
                    {
                        DisabledPeriodic();
                        System.Threading.Thread.Sleep(kLoopTimeMs);
                    }
                }
                else if (au)
                {
                    AutonInit();
                    while (IsEnabled() && IsAuton())
                    {
                        AutonPeriodic();
                        System.Threading.Thread.Sleep(kLoopTimeMs);
                        if (IsConnected())
                            Watchdog.Feed();
                    }
                }
                else
                {
                    TeleopInit();
                    while (IsEnabled() && !IsAuton())
                    {
                        TeleopPeriodic();
                        System.Threading.Thread.Sleep(kLoopTimeMs);
                        if (IsConnected())
                            Watchdog.Feed();
                    }
                }
            }
        }
    }
}
