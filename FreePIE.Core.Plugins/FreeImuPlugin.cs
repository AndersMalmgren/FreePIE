﻿using System.Globalization;
using System.IO.Ports;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.Plugins
{
    [GlobalType(Type = typeof(FreeImuGlobal))]
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
            newData = true;
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

    [Global(Name = "freeImu")]
    public class FreeImuGlobal : DofGlobal<FreeImuPlugin>
    {
        public FreeImuGlobal(FreeImuPlugin plugin) : base(plugin) { }
    }
}
