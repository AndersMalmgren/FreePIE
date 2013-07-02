using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.OculusVR;

namespace FreePIE.Core.Plugins
{
    [GlobalType(Type = typeof(OculusGlobal))]
    public class OculusPlugin : Plugin
    {
        public override object CreateGlobal()
        {
            return new OculusGlobal(this);
        }

        public override string FriendlyName
        {
            get { return "Oculus VR"; }
        }

        public override Action Start()
        {
            if (!Api.Init())
                throw new Exception("Oculus VR SDK failed to init");

            return null;
        }

        public override void Stop()
        {
            Api.Dispose();
        }

        public override void DoBeforeNextExecute()
        {
            Data = Api.Read();
            OnUpdate();
        }

        public void ReCenter()
        {
            Api.ReCenter();
        }

        public OculusVr3Dof Data
        {
            get; private set;
        }
    }

    [Global(Name = "oculusVR")]
    public class OculusGlobal : UpdateblePluginGlobal<OculusPlugin>
    {
        public OculusGlobal(OculusPlugin plugin) : base(plugin){}

        public float yaw { get { return plugin.Data.Yaw; } }
        public float pitch { get { return plugin.Data.Pitch; } }
        public float roll { get { return plugin.Data.Roll; } }

        public void ReCenter()
        {
            plugin.ReCenter();
        }
    }
}
