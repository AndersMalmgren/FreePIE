using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.Plugins
{
    [LuaGlobalType(Type = typeof(AhrsImuGlobal))]
    public class AhrsImuPlugin : Plugin, IOPlugin
    {
        private bool running;

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
            OnStarted(this, new EventArgs());

            using (var serialPort = new SerialPort("COM3", 56700))
            {
                serialPort.Open();

                Thread.Sleep(3000); //Wait for IMU to self init
                serialPort.ReadExisting();

                serialPort.Write("#ob"); // Turn on binary output
                serialPort.Write("#o1"); // Turn on continuous streaming output
                serialPort.Write("#oe0"); // Disable error message output
                serialPort.Write("#s00");



                while (serialPort.BytesToRead < "#SYNCH00\r\n".Length) { }

                var sync = serialPort.ReadLine();
                var buffer = new byte[4];

                running = true;

                var data = new AhrsData();

                while (running)
                {
                    while (serialPort.BytesToRead >= 12)
                    {
                        data.Yaw = ReadFloat(serialPort, buffer);
                        data.Pitch = ReadFloat(serialPort, buffer);
                        data.Roll = ReadFloat(serialPort, buffer);

                        Data = data;
                    }

                    Thread.Sleep(1);
                }
                serialPort.Close();
            }
        }

        private float ReadFloat(SerialPort port, byte[] buffer)
        {
            port.Read(buffer, 0, buffer.Length);
            var value = BitConverter.ToSingle(buffer, 0);
            return (float)value;
        }

        public override void Stop()
        {
            running = false;
        }

    }

    public struct AhrsData
    {
        public float Yaw;
        public float Pitch;
        public float Roll;
    }

    [LuaGlobal(Name = "ahrsImu")]
    public class AhrsImuGlobal
    {
        private readonly AhrsImuPlugin plugin;

        public AhrsImuGlobal(AhrsImuPlugin plugin)
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
