using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MicroLibrary;

namespace FreePIE.Core.ScriptEngine.ThreadTiming.Strategies
{
    /// <summary>
    /// A timing mode for those in need of extreme speeds, this mode uses a high-precision timer to allow thread intervals to be set in microseconds, allowing scripts to run at more than 1khz, and theoretically, as fast as your CPU will allow.
    /// Submitted by Felipe "HarvesteR" Falanghe
    /// Uses a 3rd party library http://www.codeproject.com/Articles/98346/Microsecond-and-Millisecond-NET-Timer Licensed under CPOL: http://www.codeproject.com/info/cpol10.aspx
    /// 
    /// </summary>
    [Timing(TimingTypes.MicroTimer)]
    public class MicroTimerStrategy : Timing
    {
        private MicroStopwatch mTimer;
        private long lastWaitEnded;


        public MicroTimerStrategy()
        {
            mTimer = new MicroStopwatch();
        }

        public override void Wait()
        {
            EnsureWatchStarted();

            while ((mTimer.ElapsedMicroseconds - lastWaitEnded) < ThreadExecutionInterval)
                Thread.Yield();

            lastWaitEnded = mTimer.ElapsedMicroseconds;
        }

        private void EnsureWatchStarted()
        {
            if (mTimer.IsRunning)
                return;

            lastWaitEnded = 0;
            mTimer.Start();
        }

        public override void Dispose()
        {
            mTimer.Stop();
        }

    }
}


/*
 * A little test script to test the thing:
 * 
 * 

import time


if starting:
	system.setThreadTiming(TimingTypes.MicroTimer)
	system.threadExecutionInterval = 100 # in µseconds
	global tLast
	tLast = 0
	
Hz = 1 / (time.clock() - tLast)
tLast = time.clock()


diagnostics.watch(Hz)
 
 * 
 * 
*/