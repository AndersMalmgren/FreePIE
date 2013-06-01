using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.Zeiss;

namespace FreePIE.Core.Plugins
{
    [GlobalType(Type = typeof(ZeissGlobal))]
    public class ZeissPlugin : Plugin
    {
        private bool running;

        public override object CreateGlobal()
        {
            return new ZeissGlobal(this);
        }

        public override Action Start()
        {
            int error;
            var result = Api.Init(out error);
            if (!result)
            {
                throw new Exception(string.Format("Error while initializing tracker: {0}", Api.GetError(error)));
            }

            result = Api.SetBootloaderMode(false, out error);
            if (!result)
            {
                throw new Exception(string.Format("Error while setting bootloader mode: {0}", Api.GetError(error)));
            }


            return BackgroundWorker;
        }

        public override void Stop()
        {
            running = false;
            Api.Release();
        }

        private void BackgroundWorker()
        {
            running = true;
            OnStarted(this, new EventArgs());

            while (running)
            {
                if (!(Api.WaitNextFrame())) Thread.Sleep(10);


                var frame = new Frame();
                var euler = new Euler();

                if (Api.GetFrame(ref frame))
                {
                    Api.QuatGetEuler(ref euler, frame.Rot);
                    Euler = euler;
                }
            }
        }

        public Euler Euler { get; private set; }

        public override string FriendlyName
        {
            get { return "Zeiss"; }
        }
    }

    [Global(Name = "zeiss")]
    public class ZeissGlobal : UpdateblePluginGlobal<ZeissPlugin>
    {
        public ZeissGlobal(ZeissPlugin plugin) : base(plugin){}

        public float Yaw { get { return plugin.Euler.Yaw; } }
        public float Pitch { get { return plugin.Euler.Pitch; } }
        public float Roll { get { return plugin.Euler.Roll; } }
    }
}
