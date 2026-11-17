using System;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.Plugins.Midi
{
    [Documentation("A MIDI message")]
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

        [Documentation("Time of the message in milliseconds since the device was opened")]
        public long timestamp { get; private set; }
        [Documentation("The kind of message")]
        public MidiStatus status { get; private set; }
        [Documentation("MIDI channel, 0 to 15")]
        public byte channel { get; private set; }
        [Documentation("The two data bytes, for example note and velocity or controller and value")]
        public byte[] buffer { get; private set; }
    }
}
