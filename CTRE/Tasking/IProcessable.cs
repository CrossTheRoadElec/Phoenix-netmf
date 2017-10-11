using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix
{
    /**
     * Any class that requires a periodic call to Process() to perform it's expected duties.
     * 
     * An important part of developing a real-time embedded system is learning how to split up 
     * tasks into nonblocking routines that can run concurrently.  HERO is a great platform to learn
     * how to take threaded block routines, and modify them into nonblocking tasks.
     * 
     * As a simple example, here are two thread routines...
     * foo()
     * {
     *      while(fooCondition) { doFoo; }
     * }
     * bar()
     * {
     *      while(barCondition) { doBar; }
     * }
     * main()
     * {
     *      start foo thread;
     *      start bar thread;
     *      while(true)
     *      {
     *          do nothing;
     *      }
     * }
     * ...but on a system that may not support or afford threading, a non-threaded example would be...
     * foo
     * {
     *      Process()
     *      {
     *           if(fooCondition) { doFoo; }
     *      }
     * }
     * bar.Process()
     * {
     *      Process()
     *      {
     *           if(barCondition) { doBar; }
     *      }
     * }
     * main()
     * {
     *      while(true)
     *      {
     *          foo.Process();
     *          bar.Process();
     *      }
     *  }
     * 
     * Marking the class as Processable signals that the class requires a periodic call to Process() in the top-level main loop.
     */
    public interface IProcessable
    {
        void Process();
    }
}
