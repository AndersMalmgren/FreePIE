using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using AHRS;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.SensorFusion;

namespace FreePIE.Core.Plugins
{
    [LuaGlobalType(Type = typeof(AndroidGlobal))]
    public class AndroidPlugin : Plugin
    {
        private UdpClient udpClient;
        private bool stopping;
        private int udpPort;
        private bool newData;
        private Quaternion quaternion;

        public double Yaw { get { return quaternion.Yaw; } }
        public double Pitch { get { return quaternion.Pitch; } }
        public double Roll { get { return quaternion.Roll; } }
        public double GoogleYaw { get; private set; }
        public double GooglePitch { get; private set; }
        public double GoogleRoll { get; private set; }

        public override object CreateGlobal()
        {
            return new AndroidGlobal(this);
        }

        public override string FriendlyName
        {
            get { return "Android"; }
        }

        public override bool GetProperty(int index, IPluginProperty property)
        {
            if(index == 0)
            {
                property.Name = "UDPPort";
                property.Caption = "UDP Port";
                property.DefaultValue = 5555;
                property.HelpText = "UDP Port number that the Wireless IMU app transmits on (default 5555)";

                return true;
            }

            return false;
        }

        public override bool SetProperties(Dictionary<string, object> properties)
        {
            udpPort = (int)properties["UDPPort"];
            return true;
        }

        public override void DoBeforeNextExecute()
        {
            if (newData)
            {
                OnUpdate();
                newData = false;
            }
        }

        public override Action Start()
        {
            return BackGroundWorker;
        }

        public override void Stop()
        {
            stopping = true;
            udpClient.Close();
        }

        private void BackGroundWorker()
        {
            var endpoint = new IPEndPoint(IPAddress.Any, udpPort);
            udpClient = new UdpClient(endpoint);

            MahonyAHRS ahrs = null;
            quaternion = new Quaternion();

            var freeqSampled = false;
            int samples = 0;
            var started = DateTime.Now;

            OnStarted(this, new EventArgs());

            while (true)
            {
                byte[] bytes;

                try
                {
                    bytes = udpClient.Receive(ref endpoint);
                }
                catch (SocketException)
                {
                    if (!stopping)
                        throw;
                    break;
                }

                if (samples == 0)
                    started = DateTime.Now;

                if (!freeqSampled)
                {
                    samples++;
                    var delta = (DateTime.Now - started).TotalSeconds;
                    if (delta > 1)
                    {

                        var freq = samples / (float)delta;
                        freeqSampled = true;

                        ahrs = new MahonyAHRS(1f / freq, 0.1f);
                    }
                    else
                        continue;
                }

                var flag = bytes[0];
                var sendRaw = (flag & 1) == 1;
                var sendOrientation = (flag & 2) == 2;

                var index = 1;
                
                if (sendRaw)
                {
                    var ax = GetFloat(bytes, index, 0);
                    var ay = GetFloat(bytes, index, 4);
                    var az = GetFloat(bytes, index, 8);

                    var gx = GetFloat(bytes, index, 12);
                    var gy = GetFloat(bytes, index, 16);
                    var gz = GetFloat(bytes, index, 20); ;

                    var mx = GetFloat(bytes, index, 24);
                    var my = GetFloat(bytes, index, 28);
                    var mz = GetFloat(bytes, index, 32);

                    ahrs.Update(gx, gy, gz, ax, ay, az, mx, my, mz);
                    quaternion.Udate(ahrs.Quaternion[0], ahrs.Quaternion[1], ahrs.Quaternion[2], ahrs.Quaternion[3]);

                    index += 36;
                }

                if (sendOrientation)
                {
                    GoogleYaw = GetFloat(bytes, index, 0);
                    GooglePitch = GetFloat(bytes, index, 4);
                    GoogleRoll = GetFloat(bytes, index, 8);
                }

                newData = true;
            }
        }

        private static float GetFloat(byte[] buffer, int offset, int index)
        {
            return BitConverter.ToSingle(buffer, offset + index);
        }
    }

    [LuaGlobal(Name = "android")]
    public class AndroidGlobal : UpdateblePluginGlobal<AndroidPlugin>
    {
        public AndroidGlobal(AndroidPlugin plugin) : base(plugin){ }

        public double Yaw { get { return plugin.Yaw; } }
        public double Pitch { get { return plugin.Pitch; } }
        public double Roll { get { return plugin.Roll; } }

        public double GoogleYaw { get { return plugin.GoogleYaw; } }
        public double GooglePitch { get { return plugin.GooglePitch; } }
        public double GoogleRoll { get { return plugin.GoogleRoll; } }
    }
}
