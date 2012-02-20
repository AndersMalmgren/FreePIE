using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.Plugins
{
    [LuaGlobalType(Type = typeof(FreeImuGlobal))]
    public class FreeImuPlugin : ComDevicePlugin
    {
        public override object CreateGlobal()
        {
            return new FreeImuGlobal(this);
        }

        protected override int DefaultBaudRate
        {
            get { return 115200; }
        }

        public FreeImuData Data { get; private set; }

        protected override void Init(SerialPort serialPort)
        {
        }

        protected override void Read(SerialPort serialPort)
        {
            var line = serialPort.ReadLine();
            var data = Data;
            newData = true;
            var values = line.Split(',');
            if (values.Length != 3)
                return;

            data.Yaw = ParseFloat(values[0]);
            data.Pitch = ParseFloat(values[1]);
            data.Roll = ParseFloat(values[2]);
            Data = data;
        }

        private float ParseFloat(string value)
        {
            return float.Parse(value, CultureInfo.InvariantCulture);
        }

        protected override string BaudRateHelpText
        {
            get { return "Baud rate, default for the FreeImu library should be 115200"; }
        }
        
        public override string FriendlyName
        {
            get { return "Free IMU"; }
        }
    }

    public struct FreeImuData
    {
        public float Yaw;
        public float Pitch;
        public float Roll;
    }

    [LuaGlobal(Name = "freeImu")]
    public class FreeImuGlobal : UpdateblePluginGlobal
    {
        private readonly FreeImuPlugin plugin;

        public FreeImuGlobal(FreeImuPlugin plugin) : base(plugin)
        {
            this.plugin = plugin;
        }

        public float getYaw()
        {
            return plugin.Data.Yaw;
        }

        public float getPitch()
        {
            return plugin.Data.Pitch;
        }

        public float getRoll()
        {
            return plugin.Data.Roll;
        }
    }
}
