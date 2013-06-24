using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using AHRS;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.SensorFusion;
using FreePIE.Core.Plugins.Strategies;

namespace FreePIE.Core.Plugins
{
    [GlobalType(Type = typeof(AndroidGlobal))]
    public class AndroidPlugin : Plugin
    {
        [Flags]
        private enum Flags
        {
            Raw = 0x01,
            Orientation = 0x02
        }

        private UdpClient udpClient;
        private bool stopping;
        private int udpPort;
        private bool newData;
        private Quaternion quaternion;
        private ContinousYawStrategy continousYawStrategy;
        private ContinousYawStrategy googleContinousYawStrategy;

        public bool ContinousYaw
        {
            get { return continousYawStrategy.Enable; }
            set
            {
                continousYawStrategy.Enable = value;
                googleContinousYawStrategy.Enable = value;
            }
        }

        public double Yaw { get { return continousYawStrategy.Yaw; } }
        public double Pitch { get { return quaternion.Pitch; } }
        public double Roll { get { return quaternion.Roll; } }
        public double GoogleYaw { get { return googleContinousYawStrategy.Yaw; } }
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
            continousYawStrategy = new ContinousYawStrategy(Units.Radians);
            googleContinousYawStrategy = new ContinousYawStrategy(Units.Radians);

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

                var flag = (Flags)bytes[0];
                bool sendRaw;
                bool sendOrientation;

                SetFlags(flag, out sendRaw, out sendOrientation);

                var dataInPackage = (Flags) bytes[1];
                bool raw;
                bool orientation;

                SetFlags(dataInPackage, out raw, out orientation);

                if (!freeqSampled && raw)
                {
                    if (samples == 0)
                        started = DateTime.Now;

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

                var index = 2;
                
                if (raw)
                {
                    var ax = GetFloat(bytes, index, 0);
                    var ay = GetFloat(bytes, index, 4);
                    var az = GetFloat(bytes, index, 8);

                    var gx = GetFloat(bytes, index, 12);
                    var gy = GetFloat(bytes, index, 16);
                    var gz = GetFloat(bytes, index, 20);

                    var mx = GetFloat(bytes, index, 24);
                    var my = GetFloat(bytes, index, 28);
                    var mz = GetFloat(bytes, index, 32);

                    ahrs.Update(gx, gy, gz, ax, ay, az, mx, my, mz);
                    quaternion.Update(ahrs.Quaternion[0], ahrs.Quaternion[1], ahrs.Quaternion[2], ahrs.Quaternion[3]);
                    continousYawStrategy.Update(quaternion.Yaw);

                    index += 36;
                }

                if (orientation)
                {
                    var yaw = GetFloat(bytes, index, 0);
                    GooglePitch = GetFloat(bytes, index, 4);
                    GoogleRoll = GetFloat(bytes, index, 8);
                    googleContinousYawStrategy.Update(yaw);
                    index += 12;
                }

                newData = true;
            }
        }

        private static float GetFloat(byte[] buffer, int offset, int index)
        {
            return BitConverter.ToSingle(buffer, offset + index);
        }

        private void SetFlags(Flags flag, out bool raw, out bool orientation)
        {
            raw = (flag & Flags.Raw) == Flags.Raw;
            orientation = (flag & Flags.Orientation) == Flags.Orientation;
        }
    }

    [Global(Name = "android")]
    public class AndroidGlobal : UpdateblePluginGlobal<AndroidPlugin>
    {
        public AndroidGlobal(AndroidPlugin plugin) : base(plugin){ }

        public bool continousYaw { get { return plugin.ContinousYaw; } set { plugin.ContinousYaw = value; } }
        public double yaw { get { return plugin.Yaw; } }
        public double pitch { get { return plugin.Pitch; } }
        public double roll { get { return plugin.Roll; } }

        public double googleYaw { get { return plugin.GoogleYaw; } }
        public double googlePitch { get { return plugin.GooglePitch; } }
        public double googleRoll { get { return plugin.GoogleRoll; } }
    }
}
