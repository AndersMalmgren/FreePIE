using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.TrackIR;

namespace FreePIE.Core.Plugins
{
    [LuaGlobalType(Type = typeof(TrackIRGlobal))]
    public class TrackIRPlugin : Plugin
    {
        private NPClientSpoof spoofer;
        private HeadPoseData latestKnownData;

        public HeadPoseData Data { get; private set; }

        public TrackIRPlugin()
        {
            Data = new HeadPoseData();
            latestKnownData = new HeadPoseData();
        }

        public override object CreateGlobal()
        {
            return new TrackIRGlobal(this);
        }

        public override Action Start()
        {
            spoofer = new NPClientSpoof();

            return null;
        }

        public override void Stop()
        {
            spoofer.Dispose();
        }

        public override string FriendlyName
        {
            get { return "TrackIR"; }
        }

        public override void DoBeforeNextExecute()
        {
            if(Data != latestKnownData)
            {
                spoofer.SetPosition(Data.X, Data.Y, Data.Z, Data.Roll, Data.Pitch, Data.Yaw);
                latestKnownData = Data;
            }
            else
            {
                var data = new HeadPoseData();

                if (spoofer.ReadPosition(ref data))
                {
                    Data = latestKnownData = data;
                    OnUpdate();
                }
            } 
        }
    }

    [LuaGlobal(Name = "trackIR")]
    public class TrackIRGlobal : UpdateblePluginGlobal
    {
        private readonly TrackIRPlugin plugin;

        public TrackIRGlobal(TrackIRPlugin plugin)
            : base(plugin)
        {
            this.plugin = plugin;
        }

        public float Yaw
        {
            get { return plugin.Data.Yaw; }
            set { plugin.Data.Yaw = value; }
        }

        public float Pitch
        {
            get { return plugin.Data.Pitch; }
            set { plugin.Data.Pitch = value; }
        }

        public float Roll
        {
            get { return plugin.Data.Roll; }
            set { plugin.Data.Roll = value; }
        }

        public float X
        {
            get { return plugin.Data.X; }
            set { plugin.Data.X = value; }
        }

        public float Y
        {
            get { return plugin.Data.Y; }
            set { plugin.Data.Y = value; }
        }

        public float Z
        {
            get { return plugin.Data.Z; }
            set { plugin.Data.Z = value; }
        }
    }
}
