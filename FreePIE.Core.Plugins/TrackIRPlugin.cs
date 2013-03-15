﻿using System;
using System.Collections.Generic;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.TrackIR;

namespace FreePIE.Core.Plugins
{
    [GlobalType(Type = typeof(TrackIRGlobal))]
    public class TrackIRPlugin : Plugin
    {
        private NPClientSpoof spoofer;
        private HeadPoseData output;

        public bool ExplicitReadRequested { get; set; }
        public HeadPoseData Input { get; private set; }

        public HeadPoseData Output
        {
            get { return output ?? (output = new HeadPoseData()); }
        }

        private bool doLog;

        public TrackIRPlugin()
        {
            Input = new HeadPoseData();
        }

        public override object CreateGlobal()
        {
            return new TrackIRGlobal(this);
        }

        public override bool GetProperty(int index, IPluginProperty property)
        {
            if(index > 0)
                return false;

            property.Name = "DoLog";
            property.Caption = "Enable logging";
            property.DefaultValue = false;
            property.HelpText = "Enable basic logging concerning TrackIR interop";

            return true;
        }

        public override bool SetProperties(Dictionary<string, object> properties)
        {
            doLog = (bool)properties["DoLog"];

            return true;
        }

        public override Action Start()
        {
            ExplicitReadRequested = false;
            spoofer = new NPClientSpoof(doLog);
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
            if(output != null)
            {
                spoofer.SetPosition(Output.X, Output.Y, Output.Z, Output.Roll, Output.Pitch, Output.Yaw);
                output = null;
            }
            
            if (GlobalHasUpdateListener || ExplicitReadRequested)
                ReadFromNpClient();
        }

        private void ReadFromNpClient()
        {
            var data = new HeadPoseData();
            if (spoofer.ReadPosition(ref data))
            {
                Input.CopyFrom(data);
                OnUpdate();
            }
        }
    }

    [Global(Name = "trackIR")]
    public class TrackIRGlobal : UpdateblePluginGlobal<TrackIRPlugin>
    {
        public TrackIRGlobal(TrackIRPlugin plugin) : base(plugin)
        { }

        public float yaw
        {
            get
            {
                plugin.ExplicitReadRequested = true;
                return plugin.Input.Yaw;
            }
            set { plugin.Output.Yaw = value; }
        }

        public float pitch
        {
            get
            {
                plugin.ExplicitReadRequested = true;
                return plugin.Input.Pitch;
            }
            set { plugin.Output.Pitch = value; }
        }

        public float roll
        {
            get
            {
                plugin.ExplicitReadRequested = true;
                return plugin.Input.Roll;
            }
            set { plugin.Output.Roll = value; }
        }

        public float x
        {
            get
            {
                plugin.ExplicitReadRequested = true;
                return plugin.Input.X;
            }
            set { plugin.Output.X = value; }
        }

        public float y
        {
            get
            {
                plugin.ExplicitReadRequested = true;
                return plugin.Input.Y;
            }
            set { plugin.Output.Y = value; }
        }

        public float z
        {
            get
            {
                plugin.ExplicitReadRequested = true;
                return plugin.Input.Z;
            }
            set { plugin.Output.Z = value; }
        }
    }
}
