using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.Plugins
{
    [GlobalType(Type = typeof(GenericComPluginGlobal))]
    public class GenericComPlugin : ComDevicePlugin
    {
        private ConcurrentQueue<byte[]> buffers;
        private string line;

        public override object CreateGlobal()
        {
            buffers = new ConcurrentQueue<byte[]>();
            return new GenericComPluginGlobal(this);
        }

        public override string FriendlyName
        {
            get { return "Generic Serial COM"; }
        }

        protected override void Init(SerialPort serialPort)
        {
        }

        protected override void Read(SerialPort serialPort)
        {
            if (serialPort.BytesToRead != 0)
            {
                var buffer = new byte[serialPort.BytesToRead];
                serialPort.Read(buffer, 0, buffer.Length);
                buffers.Enqueue(buffer);

                newData = true;
            }
        }

        public IEnumerable<byte[]> ReadExisting()
        {
            while (buffers.Count != 0)
            {
                byte[] result;
                if (buffers.TryDequeue(out result))
                    yield return result;

                Thread.Yield();
            }
        }

        public IEnumerable<string> ReadExistingString()
        {
            return ReadExisting().Select(Encoding.Default.GetString);
        }

        public string ReadLine()
        {
            var text = string.Join(string.Empty, ReadExistingString());
            line += text;

            var indexOfNewLine = line.IndexOf(Environment.NewLine, StringComparison.Ordinal);
            if (indexOfNewLine != -1)
            {
                var end = indexOfNewLine + Environment.NewLine.Length;
                var result = line.Substring(0, indexOfNewLine);

                line = line.Substring(end, line.Length - end);

                return result;
            }

            return string.Empty;
        }

        protected override string BaudRateHelpText
        {
            get { return "Baud rate"; }
        }

        protected override int DefaultBaudRate
        {
            get { return 57600; }
        }
    }

    [Global(Name = "genericCom")]
    public class GenericComPluginGlobal : UpdateblePluginGlobal<GenericComPlugin>
    {
        public GenericComPluginGlobal(GenericComPlugin plugin) : base(plugin)
        {
        }

        public IEnumerable<byte[]> readExisting()
        {
            return plugin.ReadExisting();
        }

        public IEnumerable<string> readExistingString()
        {
            return plugin.ReadExistingString();
        }

        public string readLine()
        {
            return plugin.ReadLine();
        }
    }
}
