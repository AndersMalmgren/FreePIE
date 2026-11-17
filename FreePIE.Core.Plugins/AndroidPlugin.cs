using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using AHRS;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.Globals;
using FreePIE.Core.Plugins.SensorFusion;

namespace FreePIE.Core.Plugins
{
    [GlobalType(Type = typeof(AndroidGlobal), IsIndexed = true)]
    public class AndroidPlugin : Plugin
    {
        private UdpClient udpClient;
        private bool stopping;
        private int udpPort;
        private Dictionary<int, AndroidGlobalHolder> holders; 

        public override object CreateGlobal()
        {
            holders = new Dictionary<int, AndroidGlobalHolder>();
            return new GlobalIndexer<AndroidGlobal>(Create);
        }

        private AndroidGlobal Create(int index)
        {
            var holder = new AndroidGlobalHolder(index);
            holders[index] = holder;
            return holder.Global;
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
            foreach (var holder in holders.Values)
            {
                if (holder.NewData)
                {
                    holder.OnUpdate();
                    holder.NewData = false;
                }
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
                var deviceIndex = bytes[0];
                if(holders.ContainsKey(deviceIndex))
                    holders[deviceIndex].Update(bytes);
            }
        }
    }

    public class AndroidGlobalHolder : IUpdatable
    {
        [Flags]
        private enum Flags
        {
            Raw = 0x01,
            Orientation = 0x02
        }

        private MahonyAHRS ahrs;
        private readonly Quaternion quaternion;
        private bool freeqSampled;
        private int samples;
        private DateTime started;

        public AndroidGlobalHolder(int index)
        {
            Index = index;
            Global  = new AndroidGlobal(this);

            quaternion = new Quaternion();
            Raw = new RawGlobal();

            freeqSampled = false;
            samples = 0;
            started = DateTime.Now;

        }

        public void Update(byte[] bytes)
        {
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
                        return;
                }

                var index = 2;
                
                if (raw)
                {
                    var ax = Raw.ax = GetFloat(bytes, index, 0);
                    var ay = Raw.ay = GetFloat(bytes, index, 4);
                    var az = Raw.az = GetFloat(bytes, index, 8);

                    var gx = Raw.gx = GetFloat(bytes, index, 12);
                    var gy = Raw.gy = GetFloat(bytes, index, 16);
                    var gz = Raw.gz = GetFloat(bytes, index, 20);

                    var mx = Raw.mx = GetFloat(bytes, index, 24);
                    var my = Raw.my = GetFloat(bytes, index, 28);
                    var mz = Raw.mz = GetFloat(bytes, index, 32);

                    ahrs.Update((float)gx, (float)gy, (float)gz, (float)ax, (float)ay, (float)az, (float)mx, (float)my, (float)mz);
                    quaternion.Update(ahrs.Quaternion[0], ahrs.Quaternion[1], ahrs.Quaternion[2], ahrs.Quaternion[3]);

                    index += 36;
                }

                if (orientation)
                {
                    GoogleYaw = GetFloat(bytes, index, 0);
                    GooglePitch = GetFloat(bytes, index, 4);
                    GoogleRoll = GetFloat(bytes, index, 8);
                    index += 12;
                }

                NewData = true;            
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

        public AndroidGlobal Global { get; private set; }
        public int Index { get; private set; }
        public Action OnUpdate { get; set; }
        public bool GlobalHasUpdateListener { get; set; }

        public bool NewData { get; set; }

        public double Yaw { get { return quaternion.Yaw; } }
        public double Pitch { get { return quaternion.Pitch; } }
        public double Roll { get { return quaternion.Roll; } }
        public double GoogleYaw { get; private set; }
        public double GooglePitch { get; private set; }
        public double GoogleRoll { get; private set; }

        public RawGlobal Raw { get; private set; }
        
    }

    [Global(Name = "android")]
    [Documentation("Sensor data from the FreePIE Android IMU app over UDP, index by device, android[0] is the first phone")]
    public class AndroidGlobal : UpdateblePluginGlobal<AndroidGlobalHolder>
    {
        public AndroidGlobal(AndroidGlobalHolder plugin) : base(plugin) { }

        [Documentation("Yaw in radians, fused from the raw sensors by FreePIE")]
        public double yaw { get { return plugin.Yaw; } }
        [Documentation("Pitch in radians, fused from the raw sensors by FreePIE")]
        public double pitch { get { return plugin.Pitch; } }
        [Documentation("Roll in radians, fused from the raw sensors by FreePIE")]
        public double roll { get { return plugin.Roll; } }

        [Documentation("Yaw in radians from the Android orientation sensor")]
        public double googleYaw { get { return plugin.GoogleYaw; } }
        [Documentation("Pitch in radians from the Android orientation sensor")]
        public double googlePitch { get { return plugin.GooglePitch; } }
        [Documentation("Roll in radians from the Android orientation sensor")]
        public double googleRoll { get { return plugin.GoogleRoll; } }
        [Documentation("Raw sensor data")]
        public RawGlobal raw { get { return plugin.Raw; }}
    }

    [Documentation("Raw Android sensor data")]
    public class RawGlobal
    {
        [Documentation("Accelerometer x in m/s2")]
        public double ax { get; internal set; }
        [Documentation("Accelerometer y in m/s2")]
        public double ay { get; internal set; }
        [Documentation("Accelerometer z in m/s2")]
        public double az { get; internal set; }

        [Documentation("Gyroscope x in rad/s")]
        public double gx { get; internal set; }
        [Documentation("Gyroscope y in rad/s")]
        public double gy { get; internal set; }
        [Documentation("Gyroscope z in rad/s")]
        public double gz { get; internal set; }

        [Documentation("Magnetometer x in microtesla")]
        public double mx { get; internal set; }
        [Documentation("Magnetometer y in microtesla")]
        public double my { get; internal set; }
        [Documentation("Magnetometer z in microtesla")]
        public double mz { get; internal set; }
    }
}
