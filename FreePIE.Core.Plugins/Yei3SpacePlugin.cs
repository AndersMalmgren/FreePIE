using System;
using System.Collections.Generic;
using System.Linq;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.Globals;
using FreePIE.Core.Plugins.SensorFusion;
using FreePIE.Core.Plugins.Yei3Space;

namespace FreePIE.Core.Plugins
{
    [GlobalType(Type = typeof(Yei3SpaceGlobal), IsIndexed = true)]
    public class Yei3SpacePlugin : Plugin
    {
        private int countSensors;
        private List<Yei3SpaceGlobalHolder> globals;

        public override object CreateGlobal()
        {
            return new GlobalIndexer<Yei3SpaceGlobal>(CreateDevice);
        }

        public override Action Start()
        {
            globals = new List<Yei3SpaceGlobalHolder>();
            countSensors = Api.GetComPorts();
            if (countSensors == 0)
                throw new Exception("No YEI3 Space devices connected!");
            
            return null;
        }

        public override void Stop()
        {
            globals.ForEach(g => g.CloseDevice());
        }

        private Yei3SpaceGlobal CreateDevice(int index)
        {
            if (index >= countSensors)
                throw new Exception(string.Format("Only {0} connected devices, {1} is out of bounds", countSensors, index));

            var deviceId = Api.CreateDevice(index);
            if ((TssDeviceIdMask)deviceId == TssDeviceIdMask.TSS_NO_DEVICE_ID)
                throw new Exception(string.Format("Could not create device: {0} on port {1}", port.FriendlyName, port.Port));

            var holder = new  Yei3SpaceGlobalHolder(deviceId);
            globals.Add(holder);
            return holder.Global;
        }

        public override void DoBeforeNextExecute()
        {
            globals.ForEach(g => g.Update());
        }

        public override string FriendlyName
        {
            get { return "YEI 3 Space"; }
        }
    }

    public class Yei3SpaceGlobalHolder : IUpdatable
    {
        private readonly int deviceId;

        public Yei3SpaceGlobalHolder(int deviceId)
        {
            this.deviceId = deviceId;

            Global = new Yei3SpaceGlobal(this);
            Quaternion = new Quaternion();
        }

        public void StopStreaming()
        {
            Api.StopStreaming(deviceId);
        }
        public void StartStreaming()
        {
            Api.StartStreaming(deviceId);
        }
        
        public void Update()
        {
            var error = Api.UpdateQuaternion(deviceId, Quaternion);
            if (error != TssError.TSS_NO_ERROR)
                throw new Exception(string.Format("Error while reading device: {0}", error));

            OnUpdate();
        }

        public void CloseDevice()
        {
            var error = Api.CloseDevice(deviceId);
            if(error != TssError.TSS_NO_ERROR) 
                throw new Exception(string.Format("Error while closing device: {0}", error));
        }

        public Quaternion Quaternion { get; private set; }
        public Yei3SpaceGlobal Global { get; private set; }
        public Action OnUpdate { get; set; }
        public bool GlobalHasUpdateListener { get; set; }
    }

    [Global(Name = "yei")]
    public class Yei3SpaceGlobal : UpdateblePluginGlobal<Yei3SpaceGlobalHolder>
    {
        public Yei3SpaceGlobal(Yei3SpaceGlobalHolder plugin)
            : base(plugin)
        {
        }
        public double yaw { get { return plugin.Quaternion.Yaw; } }
        public double pitch { get { return plugin.Quaternion.Pitch; } }
        public double roll { get { return plugin.Quaternion.Roll; } }
        public void stopStreaming()
        {
            plugin.StopStreaming();
        }
        public void startStreaming()
        {
            plugin.StartStreaming();
        }
    }
}
