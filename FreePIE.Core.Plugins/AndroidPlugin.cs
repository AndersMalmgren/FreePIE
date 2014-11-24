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
    public class AndroidGlobal : UpdateblePluginGlobal<AndroidGlobalHolder>
    {
        public AndroidGlobal(AndroidGlobalHolder plugin) : base(plugin) { }

        public double yaw { get { return plugin.Yaw; } }
        public double pitch { get { return plugin.Pitch; } }
        public double roll { get { return plugin.Roll; } }

        public double googleYaw { get { return plugin.GoogleYaw; } }
        public double googlePitch { get { return plugin.GooglePitch; } }
        public double googleRoll { get { return plugin.GoogleRoll; } }
        public RawGlobal raw { get { return plugin.Raw; }}
    }

    public class RawGlobal
    {
        public double ax { get; internal set; }
        public double ay { get; internal set; }
        public double az { get; internal set; }

        public double gx { get; internal set; }
        public double gy { get; internal set; }
        public double gz { get; internal set; }

        public double mx { get; internal set; }
        public double my { get; internal set; }
        public double mz { get; internal set; }
    }
}
