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
        private const int settingsDeviceCount = 5;

        public override object CreateGlobal()
        {
            return new GlobalIndexer<Yei3SpaceGlobal>(CreateDevice);
        }

        public override bool GetProperty(int index, IPluginProperty property)
        {
            switch (index)
            {
                case 0:
                    property.Name = "StreamButtonState";
                    property.Caption = "Stream Button State";
                    property.DefaultValue = false;
                    property.HelpText = "Allows Streaming of the sensor button states";
                    return true;
                case 1:
                    property.Name = "PollUnknownDevices";
                    property.Caption = "Poll Unknown Devices";
                    property.DefaultValue = false;
                    property.HelpText = "Writes bytes to serial ports to find 3-Space devices not listed in registry";
                    return true;

            }

            int deviceIndex = index - 2;

            if (deviceIndex > settingsDeviceCount) return false;

            property.Name = string.Format("Device{0}", deviceIndex);
            property.Caption = string.Format("Device {0}", deviceIndex);
            property.DefaultValue = "0";
            property.HelpText = string.Format("Serial Number of the device in slot {0}", deviceIndex);

            return true;
        }

        public override bool SetProperties(Dictionary<string, object> properties)
        {
            Api.stream_button_state = (bool) properties["StreamButtonState"];
            Api.poll_unknown_devices = (bool) properties["PollUnknownDevices"];

            Enumerable.Range(0, settingsDeviceCount)
                      .ToList()
                      .ForEach(i =>
                          {
                              var key = string.Format("Device{0}", i);
                              try
                              {
                                  Api.device_serials[i] = Convert.ToUInt32((string) properties[key], 16);
                              }
                              catch (FormatException)
                              {
                                  throw new Exception(string.Format("{0} serial is invalid", key));
                              }
                          });

            return true;
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
            Api.CloseDongles();
        }

        private Yei3SpaceGlobal CreateDevice(int index)
        {
            if (index >= countSensors)
                throw new Exception(string.Format("Only {0} connected devices, {1} is out of bounds", countSensors, index));

            var deviceId = Api.CreateDevice(index);
            if ((TssDeviceIdMask)deviceId == TssDeviceIdMask.TSS_NO_DEVICE_ID)
                throw new Exception(string.Format("Could not create device"));

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
        private readonly uint deviceId;

        public Yei3SpaceGlobalHolder(uint deviceId)
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
        public void TareSensor()
        {
            TssError error = Api.TareSensor(deviceId);
            if (error != TssError.TSS_NO_ERROR)
                throw new Exception(string.Format("Error while taring: {0}", error));
        }
        public void Update()
        {
            var error = Api.UpdateSensor(deviceId, Quaternion, out ButtonState);
            if (error != TssError.TSS_NO_ERROR && error != TssError.TSS_ERROR_READ)
            {
                throw new Exception(string.Format("Error while reading device: {0}", error));
            }

            OnUpdate();
        }

        public void CloseDevice()
        {
            var error = Api.CloseDevice(deviceId);
            if(error != TssError.TSS_NO_ERROR) 
                throw new Exception(string.Format("Error while closing device: {0}", error));
        }

        public Quaternion Quaternion { get; private set; }
        public byte ButtonState;
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
        public bool button0 { get { return Convert.ToBoolean(plugin.ButtonState & 1); } }
        public bool button1 { get { return Convert.ToBoolean(plugin.ButtonState & 2); } }
        public void tareSensor()
        {
            plugin.TareSensor();
        }
    }
}
