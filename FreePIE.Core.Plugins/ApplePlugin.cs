using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.Plugins
{

    public abstract class ApplePlugin : Plugin
    {
        private bool Stopped;
        private int UdpPort = 10552;
        private UdpClient UdpSock;


        private int PitchIndex = 4;
        private int RollIndex = 5;
        private int YawIndex = 6;

        private double YawSample;
        private int YawPeriod; // the number of full yaw rotations
        private double PitchSample;
        private double RollSample;
        private bool columnOrderConfigured = false;

        public override Action Start()
        {
            return RunSensorPoll;
        }

        public override void Stop()
        {
            Stopped = true;

            // This will cause the blocking read to kick out an exception and complete
            if (UdpSock != null)
                UdpSock.Close();
        }

        public override bool GetProperty(int index, IPluginProperty property)
        {

            if (index == 0)
            {
                property.Name = "UDPPort";
                property.Caption = "UDP Port";
                property.DefaultValue = 10552;
                property.HelpText = "UDP Port number that Sensor Data App transmits on (default 10552)";
                return true;
            }
                
            return false;
        }

        public override bool SetProperties(Dictionary<string, object> properties)
        {

            UdpPort = (int) properties["UDPPort"];
            return true;
        }

        private void RunSensorPoll()
        {

            try
            {
                var peer = new IPEndPoint(IPAddress.Any, 0);
                UdpSock = new UdpClient(UdpPort);

                char[] delims = new char[] {','};

                Stopped = false;
                OnStarted(this, new EventArgs());
                var started = DateTime.Now;

                while (!Stopped)
                {

                    Byte[] bytes = UdpSock.Receive(ref peer);
                    int len = bytes.Length;
                    if (bytes[len - 2] == '\r') // remove the \r\n
                        len -= 2;

                    string data = Encoding.ASCII.GetString(bytes, 0, len);
                    String[] fields = data.Split(delims);

                    if (fields[0] == "Timestamp")
                    {
                        // The first message is a field definition list
                        // Parse it to determine which data is present and where it is going to be located
                        for (int i = 1; i < fields.Length; i++)
                        {
                            // It looksl like there are bugs below but there are not.
                            // The iPhone app seems to orient phone with the long axis running east/west
                            // while I assume the phone should have the long axis running north/south
                            // This difference in perspective, reverses the definition of pitch and roll
                            if (fields[i] == "Roll")
                                PitchIndex = i;
                            else if (fields[i] == "Pitch")
                                RollIndex = i;
                            else if (fields[i] == "Yaw")
                                YawIndex = i;
                            YawPeriod = 0;
                        }

                        columnOrderConfigured = true;
                    }
                    else if(columnOrderConfigured)
                    {
                        try
                        {
                            RollSample = Parse(fields[RollIndex]);
                            PitchSample = Parse(fields[PitchIndex]);
                            double previous_yaw = YawSample;
                            YawSample = Parse(fields[YawIndex]);

                            //DateTime now = DateTime.Now;
                            //TimeSpan span = now - start;
                            //System.Console.WriteLine(span.TotalMilliseconds + " sampled yaw = " + YawSample);
                            //start = now;

                            double delta = YawSample - previous_yaw;
                            double HALF_CIRCLE = Math.PI;
                            if (Math.Abs(delta) > HALF_CIRCLE)
                            {
                                // We turned across the discontinuity at 180 degrees so increment
                                // the period counter in the probably direction of angular motion
                                if (delta > 0)
                                    YawPeriod--;
                                else
                                    YawPeriod++;
                            }
                        }
                        catch (Exception e)
                        {
                            throw new Exception(string.Format("String '{0}' was not parsed correctly: {1}", data, e.Message));
                        }
                    }

                    if (!columnOrderConfigured && DateTime.Now - started > TimeSpan.FromSeconds(10))
                        throw new Exception("Sensor app did not send column order in expected time frame");
                }
            }
            catch (SocketException err)
            {
                // A graceful shutdown calls close socket and throws an exception while blocked in Receive()
                // Ignore this exception unless it was not generated during shutdown sequence
                if (!Stopped)
                    throw err;
            }
        }

        private double Parse(string value)
        {
            return double.Parse(value, CultureInfo.InvariantCulture);
        }

        public bool ContinousYawMode { get; set; }

        public double Yaw
        {
            get
            {
                // TODO: synchronize thread access to the sample variables
                // Hmmm I wonder if contention could cause some of the drift that I am experiencing
                double yaw;
                if (ContinousYawMode)
                    yaw = (YawPeriod*2*Math.PI) + YawSample;
                else
                    yaw = YawSample;


                return yaw;
            }
        }

        public double Roll
        {
            get
            {
                double roll = RollSample;
                return roll;
            }
        }

        public double Pitch
        {
            get
            {
                double pitch = PitchSample;
                return pitch;
            }
        }
    }

    public class ApplePluginGlobal
    {
        private readonly ApplePlugin Device;

        public ApplePluginGlobal(ApplePlugin plugin)
        {
            Device = plugin;
        }


        public bool continuousYawMode
        {
            get { return Device.ContinousYawMode; }
            set { Device.ContinousYawMode = value; }
        }

        public double yaw
        {
            get { return Device.Yaw; }
        }

        public double pitch
        {
            get { return Device.Pitch; }
        }

        public double roll
        {
            get { return Device.Roll; }
        }
    }

    [GlobalType(Type = typeof (iPhonePluginGlobal))]
    public class iPhonePlugin : ApplePlugin
    {

        public override object CreateGlobal()
        {
            return new iPhonePluginGlobal(this);
        }

        public override string FriendlyName
        {
            get { return "iPhone"; }
        }
    }

    [Global(Name = "iPhone")]
    public class iPhonePluginGlobal : ApplePluginGlobal
    {

        public iPhonePluginGlobal(iPhonePlugin plugin)
            : base(plugin)
        {

        }
    }

    [GlobalType(Type = typeof (iPodPluginGlobal))]
    public class iPodPlugin : ApplePlugin
    {

        public override object CreateGlobal()
        {
            return new iPodPluginGlobal(this);
        }

        public override string FriendlyName
        {
            get { return "iPod"; }
        }
    }

    [Global(Name = "iPod")]
    public class iPodPluginGlobal : ApplePluginGlobal
    {

        public iPodPluginGlobal(iPodPlugin plugin)
            : base(plugin)
        {

        }
    }

    [GlobalType(Type = typeof (iPadPluginGlobal))]
    public class iPadPlugin : ApplePlugin
    {

        public override object CreateGlobal()
        {
            return new iPadPluginGlobal(this);
        }

        public override string FriendlyName
        {
            get { return "iPad"; }
        }
    }

    [Global(Name = "iPad")]
    public class iPadPluginGlobal : ApplePluginGlobal
    {

        public iPadPluginGlobal(iPadPlugin plugin)
            : base(plugin)
        {

        }
    }
}
