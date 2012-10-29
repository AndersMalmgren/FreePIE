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
    [GlobalType(Type = typeof(AhrsImuGlobal))]
    public class AhrsImuPlugin : ComDevicePlugin
    {
        private byte[] buffer;

        public override object CreateGlobal()
        {
            return new AhrsImuGlobal(this);
        }

        protected override int DefaultBaudRate
        {
            get { return 57600; }
        }

        protected override void Init(SerialPort serialPort)
        {
            Thread.Sleep(3000); //Wait for IMU to self init
            serialPort.ReadExisting();

            serialPort.Write("#ob"); // Turn on binary output
            serialPort.Write("#o1"); // Turn on continuous streaming output
            serialPort.Write("#oe0"); // Disable error message output
            serialPort.Write("#s00"); //Request sync signal

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (serialPort.BytesToRead < "#SYNCH00\r\n".Length)
            {
                if (stopwatch.ElapsedMilliseconds > 100)
                    throw new Exception(string.Format("No hardware connected to port {0} with AHRS IMU protocol", port));
            }
            stopwatch.Stop();

            var sync = serialPort.ReadLine();
            buffer = new byte[4];
        }

        protected override void Read(SerialPort serialPort)
        {
            while (serialPort.BytesToRead >= 12)
            {
                var data = Data;
                data.Yaw = ReadFloat(serialPort, buffer);
                data.Pitch = ReadFloat(serialPort, buffer);
                data.Roll = ReadFloat(serialPort, buffer);

                Data = data;
                newData = true;
            }

            Thread.Sleep(1);
        }

        protected override string BaudRateHelpText
        {
            get { return "Baud rate, default on AHRS should be 57600"; }
        }
        
        public override string FriendlyName
        {
            get { return "AHRS IMU"; }
        }

        private float ReadFloat(SerialPort port, byte[] buffer)
        {
            port.Read(buffer, 0, buffer.Length);
            var value = BitConverter.ToSingle(buffer, 0);
            return value;
        }
    }

    [Global(Name = "ahrsImu")]
    public class AhrsImuGlobal : DofGlobal<AhrsImuPlugin>
    {
        public AhrsImuGlobal(AhrsImuPlugin plugin) : base(plugin) { }
    }
}
