using System;
using System.Threading; // bip
using System.Runtime.InteropServices; //bip dllimport
using System.Collections.Generic;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.Plugins
{
    [GlobalType(Type = typeof (BeepGlobal))]
    public class BeepPlugin : Plugin
    {
        public override object CreateGlobal()
        {
            return new BeepGlobal(this);
        }
        public override string FriendlyName
        {
            get { return "Beep"; }
        }
 // Beep Function
        public class BackgroundBeep
        {
            static Thread _beepThread;
            static AutoResetEvent _signalBeep;
            static int frequency;
            static int duration;
            static BackgroundBeep()
            {
                _signalBeep = new AutoResetEvent(false);
                _beepThread = new Thread(() =>
                {
                    for (; ; )
                    {
                        _signalBeep.WaitOne();
                        if (duration != 0)
                            Console.Beep(frequency, duration);
                    }
                }, 1);

                _beepThread.IsBackground = true;
                _beepThread.Start();
            }
            public static void Beep(int f, int d)
            {
                if (d == 0) return;
                duration = d;
                frequency = f;
                _signalBeep.Set();
            }
            public static void Beep(IList<int> bip)
            {
                if (bip == null) return;
                duration = bip[1];
                frequency = bip[0];
                _signalBeep.Set();
            }
        }
// End Beep function
    }

    [Global(Name = "beep")]
    public class BeepGlobal
    {
        private readonly BeepPlugin plugin;

        public BeepGlobal(BeepPlugin plugin)
        {
            this.plugin = plugin;
        }

        public void play(int frequency, int duration = 300)
        {
            BeepPlugin.BackgroundBeep.Beep(frequency, duration);
        }
        public void play(bool value, IList<int> bip1 = null, IList<int> bip2 = null)
        {
            if (value)
                BeepPlugin.BackgroundBeep.Beep(bip1);
            else
                BeepPlugin.BackgroundBeep.Beep(bip2);
        }
    }
}
