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
        private readonly DolphiimoteDLL dll;
        private readonly WiimoteCalibration calibration;
        private readonly Dictionary<uint, DolphiimoteWiimoteData> data;
        private readonly string logFile;

        public event EventHandler<UpdateEventArgs<uint>> DataReceived;

        public DolphiimoteBridge(LogLevel logLevel, string logFile)
        {
            this.logFile = logFile;

            double motionPlusSlowGain = 1.0 / (8192.0 / 595.0) / 1.44; //TODO: FIX CALIBRATION
            calibration = new WiimoteCalibration(9.8 / 0x19, -0x80, motionPlusSlowGain , motionPlusSlowGain * 2000 / 440 ,-0x2000);
            dll = new DolphiimoteDLL(Path.Combine(Environment.CurrentDirectory, "DolphiiMote.dll"));

            data = new Dictionary<uint, DolphiimoteWiimoteData>();

            for (byte i = 0; i < 4; i++)
                data[i] = new DolphiimoteWiimoteData(i, calibration);
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
            dll.EnableCapabilities(wiimote, flags);
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
