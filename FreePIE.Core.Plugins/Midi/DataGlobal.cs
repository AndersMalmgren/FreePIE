using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FreePIE.Core.Plugins.Midi
{
    public class DataGlobal
    {
        public DataGlobal(int message)
        {
           // var integer = BitConverter.ToInt32(new byte[] {0xbC, 0x14, 0x7f, 0x0}, 0);

            var data = BitConverter.GetBytes(message);
            status = (MidiStatus)(data[0] >> 4);
            channel = (byte)(0x0F & data[0]);
            buffer = new byte[2];
            Array.Copy(data, 1, buffer, 0, 2);
        }

        public MidiStatus status { get; private set; }
        public byte channel { get; private set; }
        public byte[] buffer { get; private set; }
    }
}
