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
    [LuaGlobalType(Type = typeof(AhrsImuGlobal))]
    public class AhrsImuPlugin : Plugin
    {
        private bool running;
        private string port;
        private int baudRate;
        private bool newData;

        public override object CreateGlobal()
        {
            return new AhrsImuGlobal(this);
        }

        public override Action Start()
        {
            return Init;
        }

        public AhrsData Data { get; private set; }

        private void Init()
        {
            running = true;
            OnStarted(this, new EventArgs());

            using (var serialPort = new SerialPort(port, baudRate))
            {
                serialPort.Open();

                Thread.Sleep(3000); //Wait for IMU to self init
                serialPort.ReadExisting();

                serialPort.Write("#ob"); // Turn on binary output
                serialPort.Write("#o1"); // Turn on continuous streaming output
                serialPort.Write("#oe0"); // Disable error message output
                serialPort.Write("#s00");

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                while (serialPort.BytesToRead < "#SYNCH00\r\n".Length)
                {
                    if(stopwatch.ElapsedMilliseconds > 100)
                        throw new Exception(string.Format("No hardware connected to port {0} with AHRS IMU protocol", port));
                }
                stopwatch.Stop();

                var sync = serialPort.ReadLine();
                var buffer = new byte[4];

                var data = new AhrsData();

                while (running)
                {
                    while (serialPort.BytesToRead >= 12)
                    {
                        data.Yaw = ReadFloat(serialPort, buffer);
                        data.Pitch = ReadFloat(serialPort, buffer);
                        data.Roll = ReadFloat(serialPort, buffer);

                        Data = data;
                        newData = true;
                    }

                    Thread.Sleep(1);
                }
                serialPort.Close();
            }
        }

        public override void Stop()
        {
            running = false;
        }

        public override void DoBeforeNextExecute()
        {
            if(newData)
            {
                OnUpdate();
                newData = false;
            }
        }

        public override string FriendlyName
        {
            get { return "AHRS IMU"; }
        }

        public override bool GetProperty(int index, IPluginProperty property)
        {
            switch(index)
            {
                case 0:
                    property.Name = "Port";
                    property.Caption = "Com port";
                    property.HelpText = "The com port of the FTDI device";

                    foreach(var p in SerialPort.GetPortNames())
                    {
                        property.Choices.Add(p, p);
                    }

                    property.DefaultValue = "COM3";
                    return true;
                case 1:
                    property.Name = "BaudRate";
                    property.Caption = "Baud rate";
                    property.DefaultValue = 57600;
                    property.HelpText = "Baud rate, default on AHRS should be 57600";

                    foreach(var rate in new int[] { 1200, 2400, 4800, 9600, 14400, 19200, 38400, 57600, 115200 })
                    {
                        property.Choices.Add(rate.ToString(CultureInfo.InvariantCulture), rate);
                    }

                    return true;
            }

            return false;
        }

        public override bool SetProperties(Dictionary<string, object> properties)
        {
            port = properties["Port"] as string;
            baudRate = (int)properties["BaudRate"];

            return true;
        }

        private float ReadFloat(SerialPort port, byte[] buffer)
        {
            port.Read(buffer, 0, buffer.Length);
            var value = BitConverter.ToSingle(buffer, 0);
            return value;
        }
    }

    public struct AhrsData
    {
        public float Yaw;
        public float Pitch;
        public float Roll;
    }

    [LuaGlobal(Name = "ahrsImu")]
    public class AhrsImuGlobal : UpdateblePluginGlobal
    {
        private readonly AhrsImuPlugin plugin;

        public AhrsImuGlobal(AhrsImuPlugin plugin) : base(plugin)
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
