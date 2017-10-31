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
        private readonly Dictionary<uint, DolphiimoteWiimoteData> data;
        private readonly string logFile;
        private readonly Func<IMotionPlusFuser> fuserFactory;

        private readonly Dictionary<byte, WiimoteCapabilities> knownCapabilities;
        private readonly Queue<KeyValuePair<byte, WiimoteCapabilities>> deferredEnables;
        private readonly Queue<KeyValuePair<byte, Boolean>> deferredRumbles;
        private readonly Queue<KeyValuePair<byte, int>> deferredLEDChanges;
        private readonly Queue<byte> deferredStatusRequests;

        public event EventHandler<UpdateEventArgs<uint>> DataReceived;
        public event EventHandler<UpdateEventArgs<uint>> CapabilitiesChanged;
        public event EventHandler<UpdateEventArgs<uint>> StatusChanged;
        public DolphiimoteBridge(LogLevel logLevel, string logFile, Func<IMotionPlusFuser> fuserFactory)
        {
            this.logFile = logFile;
            this.fuserFactory = fuserFactory;

            dll = new DolphiimoteDll(Path.Combine(Environment.CurrentDirectory, "DolphiiMote.dll"));

            deferredEnables = new Queue<KeyValuePair<byte, WiimoteCapabilities>>();
            deferredRumbles = new Queue<KeyValuePair<byte, Boolean>>();
            deferredLEDChanges = new Queue<KeyValuePair<byte, int>>();
            deferredStatusRequests = new Queue<byte>();
            knownCapabilities = new Dictionary<byte, WiimoteCapabilities>();

            data = new Dictionary<uint, DolphiimoteWiimoteData>();

            for (byte i = 0; i < 4; i++)
                data[i] = new DolphiimoteWiimoteData(i, new WiimoteCalibration(), fuserFactory());
        }

        public void Init()
        {

            dll.Init(WiimoteDataReceived,
                                      WiimoteConnectionChanged,
                                      WiimoteCapabilitiesChanged,
                                      WiimoteStatusChanged,
                                      WiimoteLogReceived);
        }

        public void Enable(byte wiimote, WiimoteCapabilities flags)
        {
            deferredEnables.Enqueue(new KeyValuePair<byte, WiimoteCapabilities>(wiimote, flags));
        }
        public void SetRumble(byte wiimote, Boolean shouldRumble)
        {
            deferredRumbles.Enqueue(new KeyValuePair<byte, bool>(wiimote, shouldRumble));
        }
        public void SetLEDState(byte wiimote, int ledState)
        {
            deferredLEDChanges.Enqueue(new KeyValuePair<byte, int>(wiimote, ledState));
        }
        public void RequestStatus(byte wiimote)
        {
            deferredStatusRequests.Enqueue(wiimote);
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
        private void WiimoteStatusChanged(byte wiimote, DolphiimoteStatus status)
        {
            this.data[wiimote].BatteryPercentage = status.battery_level;
            this.data[wiimote].LEDStatus = status.led_status;
            if (StatusChanged != null)
                StatusChanged(this, new UpdateEventArgs<uint>(wiimote));
        }
        private void WiimoteCapabilitiesChanged(byte wiimote, DolphiimoteCapabilities capabilities)
        {
            knownCapabilities[wiimote] = (WiimoteCapabilities)capabilities.available_capabilities;
            dll.SetReportingMode(wiimote, 0x35);
            this.data[wiimote].AvailableCapabilities = knownCapabilities[wiimote];
            this.data[wiimote].EnabledCapabilities = (WiimoteCapabilities)capabilities.enabled_capabilities;
            this.data[wiimote].ExtensionType = (WiimoteExtensions)capabilities.extension_type;
            this.data[wiimote].ExtensionID = capabilities.extension_id;
            if (CapabilitiesChanged != null)
                CapabilitiesChanged(this, new UpdateEventArgs<uint>(wiimote));
        }

        private void WiimoteConnectionChanged(byte wiimote, bool connected)
        {

        }

        public void DoTick()
        {
            dll.Update();

            if (occuredException != null)
                throw new Exception(occuredException.Message, occuredException);

            foreach(var deferredEnable in deferredEnables.ToList())
            {
                if (!HasRequestedCapabilities(deferredEnable.Key, deferredEnable.Value))
                    continue;

                dll.EnableCapabilities(deferredEnable.Key, deferredEnable.Value);
                deferredEnables.Dequeue();
            }
            foreach (var deferredRumble in deferredRumbles.ToList())
            {
                dll.SetRumble(deferredRumble.Key, deferredRumble.Value);
                deferredRumbles.Dequeue();
            }
            foreach (byte wiimote in deferredStatusRequests.ToList())
            {
                dll.RequestStatus(wiimote);
                deferredStatusRequests.Dequeue();
            }
            foreach (var deferredLEDChange in deferredLEDChanges.ToList())
            {
                dll.SetLedState(deferredLEDChange.Key, deferredLEDChange.Value);
                deferredLEDChanges.Dequeue();
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
