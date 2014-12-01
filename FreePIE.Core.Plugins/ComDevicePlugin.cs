using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.Plugins
{
    public abstract class ComDevicePlugin : Plugin
    {
        private bool stopping;
        private SerialPort serialPort;
        protected string port;
        protected int baudRate;
        protected bool newData;

        protected abstract void Init(SerialPort serialPort);
        protected abstract void Read(SerialPort serialPort);
        protected abstract string BaudRateHelpText { get; }
        protected abstract int DefaultBaudRate { get; }

        public override Action Start()
        {
            return ThreadAction;
        }

        private void ThreadAction()
        {
            OnStarted(this, new EventArgs());

            serialPort = new SerialPort(port, baudRate);
            serialPort.Open();
            Init(serialPort);

            try
            {
                serialPort.DiscardInBuffer();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e);
                throw;
            }


            while (true)
            {
                try
                {
                    Read(serialPort);
                }
                catch (Exception)
                {
                    if (!stopping)
                        throw;

                    break;
                }
            }
        }

        public override void Stop()
        {
            stopping = true;
            serialPort.Close();
            serialPort.Dispose();
        }

        public override void DoBeforeNextExecute()
        {
            if (newData)
            {
                OnUpdate();
                newData = false;
            }
        }

        public override bool GetProperty(int index, IPluginProperty property)
        {
            switch (index)
            {
                case 0:
                    property.Name = "Port";
                    property.Caption = "Com port";
                    property.HelpText = "The com port of the FTDI device";

                    foreach (var p in SerialPort.GetPortNames())
                    {
                        property.Choices.Add(p, p);
                    }

                    property.DefaultValue = "COM3";
                    return true;
                case 1:
                    property.Name = "BaudRate";
                    property.Caption = "Baud rate";
                    property.DefaultValue = DefaultBaudRate;
                    property.HelpText = BaudRateHelpText;

                    foreach (var rate in new int[] { 1200, 2400, 4800, 9600, 14400, 19200, 38400, 57600, 115200 })
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

        public DofData Data { get; protected set; }

        public struct DofData
        {
            public float Yaw;
            public float Pitch;
            public float Roll;
        }
    }

    public abstract class DofGlobal<TPlugin> : UpdateblePluginGlobal<TPlugin> where TPlugin : ComDevicePlugin
    {
        protected DofGlobal(TPlugin plugin) : base(plugin){}

        public float yaw
        {
            get { return plugin.Data.Yaw; }
        }

        public float pitch
        {
            get { return plugin.Data.Pitch; }
        }

        public float roll
        {
            get { return plugin.Data.Roll; }
        }
    }
}
