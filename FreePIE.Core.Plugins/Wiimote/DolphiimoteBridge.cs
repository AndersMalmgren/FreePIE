using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace FreePIE.Core.Plugins.Wiimote
{
    public class DolphiimoteBridge : IWiimoteBridge
    {
        private readonly DolphiimoteDll dll;
        private readonly WiimoteCalibration calibration;
        private readonly Dictionary<uint, DolphiimoteWiimoteData> data;
        private readonly string logFile;
        private readonly Func<IMotionPlusFuser> fuserFactory;

        private readonly Dictionary<byte, WiimoteCapabilities> knownCapabilities;
        private readonly Queue<KeyValuePair<byte, WiimoteCapabilities>> deferredEnables;

        public event EventHandler<UpdateEventArgs<uint>> DataReceived;

        public DolphiimoteBridge(LogLevel logLevel, string logFile, Func<IMotionPlusFuser> fuserFactory)
        {
            this.logFile = logFile;
            this.fuserFactory = fuserFactory;

            calibration = new WiimoteCalibration();
            dll = new DolphiimoteDll(Path.Combine(Environment.CurrentDirectory, "DolphiiMote.dll"));

            deferredEnables = new Queue<KeyValuePair<byte, WiimoteCapabilities>>();
            knownCapabilities = new Dictionary<byte, WiimoteCapabilities>();

            data = new Dictionary<uint, DolphiimoteWiimoteData>();

            for (byte i = 0; i < 4; i++)
                data[i] = new DolphiimoteWiimoteData(i, calibration, fuserFactory());
        }

        public void Init()
        {
            int wiimoteFlag = dll.Init(WiimoteDataReceived,
                                      WiimoteConnectionChanged,
                                      WiimoteCapabilitiesChanged,
                                      WiimoteLogReceived);

            for (byte i = 0; i < 4; i++, wiimoteFlag >>= 1)
                if ((wiimoteFlag & 0x01) == 0x01)
                    dll.DetermineCapabilities(i);
        }

        public void Enable(byte wiimote, WiimoteCapabilities flags)
        {
            deferredEnables.Enqueue(new KeyValuePair<byte, WiimoteCapabilities>(wiimote, flags));
        }

        private void WiimoteLogReceived(string log)
        {
            if(logFile == null)
                Debug.WriteLine(log);
            else File.AppendAllText(logFile, log);
        }

        private Exception occuredException;

        private void WiimoteDataReceived(byte wiimote, DolphiimoteData data)
        {
            try
            {
                this.data[wiimote].Update(data);
                OnDataReceived(wiimote);
            }
            catch (Exception e)
            {
                occuredException = e;
            }
        }

        private void OnDataReceived(uint wiimote)
        {
            if (DataReceived != null)
                DataReceived(this, new UpdateEventArgs<uint>(wiimote));
        }

        private void WiimoteCapabilitiesChanged(byte wiimote, DolphiimoteCapabilities capabilities)
        {
            knownCapabilities[wiimote] = (WiimoteCapabilities)capabilities.available_capabilities;
            dll.SetReportingMode(wiimote, 0x35);
        }

        private void WiimoteConnectionChanged(byte wiimote, bool connected)
        {

        }

        public void DoTick()
        {
            dll.Update();

            if (occuredException != null)
                throw occuredException;

            foreach(var deferredEnable in deferredEnables.ToList())
            {
                if (!HasRequestedCapabilities(deferredEnable.Key, deferredEnable.Value))
                    continue;

                dll.EnableCapabilities(deferredEnable.Key, deferredEnable.Value);
                deferredEnables.Dequeue();
            }
        }

        private bool HasRequestedCapabilities(byte wiimote, WiimoteCapabilities capabilities)
        {
            if (!knownCapabilities.ContainsKey(wiimote))
                return false;

            return (knownCapabilities[wiimote] & capabilities) == capabilities;
        }

        public void Dispose()
        {
            dll.Dispose();
        }

        public IWiimoteData GetData(uint i)
        {
            return data[i];
        }

        public void Shutdown()
        {
            dll.Shutdown();
        }
    }
}
