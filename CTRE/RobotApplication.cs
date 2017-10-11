using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix
{
    public abstract class RobotApplication
    {
        /**
         * Public instance of the application, allows debuggers to throw this into the watch.
         */
        public static RobotApplication Instance;

        protected static void Start(RobotApplication ra)
        {
            Instance = ra;
            /*first repeat call init until true */
            bool flag = false;
            while (!flag)
            {
                flag = Instance.RobotInit();
                System.Threading.Thread.Sleep(20);
            }
            /* now run loops */
            Instance.RunForever();
            Reporting.ConsolePrint("RunForever returned, this should not be allowed.");
        }

        /**
         * Called by the framework to run the robot.
         */ 
        public virtual void RunForever()
        {
            Reporting.ConsolePrint("RunForever needs to be implemented.");
        }

        /**
         * @return true if user has initialized robot, false if robot is not initialized.
         * Framework will keep calling RobotInit() until it returns true, allowing developers to 
         * spend as much time as needed to properly setup sub systems.  
         * Implementation may also block indefinitely (however periodic sleeping is recommended).
         */
        public virtual bool RobotInit() { return true; }
    }
}