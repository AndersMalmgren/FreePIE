using System;
using System.Threading;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.Zeiss;

namespace FreePIE.Core.Plugins
{
    [GlobalType(Type = typeof(ZeissGlobal))]
    public class ZeissPlugin : Plugin
    {
        private bool running;
        private bool newData;

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

            return BackgroundWorker;
        }

        public override void Stop()
        {
            running = false;
            Api.Release();
        }

        public override void DoBeforeNextExecute()
        {
            if (newData)
            {
                OnUpdate();
                newData = false;
            }
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
                var cinemizerRot = new Quat(); 
                var cinemizerEuler = new Euler(); 

                if (Api.GetFrame(ref frame))
                {
                    Api.QuatGetEuler(ref euler, frame.Rot);
                    Api.RotateTrackerToCinemizer(ref cinemizerRot, frame.Rot); 
                    Api.QuatGetEuler(ref cinemizerEuler, cinemizerRot); 
                    
                    Euler = cinemizerEuler; 

                    newData = true;
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
