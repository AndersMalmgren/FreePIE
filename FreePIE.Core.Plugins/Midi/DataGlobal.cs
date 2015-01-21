using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FreePIE.Core.Plugins.Midi
{
    public class DataGlobal
    {
        public DataGlobal()
        {
            buffer = new byte[2];
        }

        public DataGlobal(int message, long timestamp) : this()
        {
            var data = BitConverter.GetBytes(message);

            this.timestamp = timestamp;
            status = (MidiStatus)(data[0] >> 4);
            channel = (byte)(0x0F & data[0]);
            Array.Copy(data, 1, buffer, 0, 2);
        }

        public long timestamp { get; private set; }
        public MidiStatus status { get; private set; }
        public byte channel { get; private set; }
        public byte[] buffer { get; private set; }
    }
}
