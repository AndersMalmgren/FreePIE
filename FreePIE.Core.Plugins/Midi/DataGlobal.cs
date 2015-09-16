using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FreePIE.Core.Plugins.Midi
{
    public class DataGlobal
    {
        private readonly MidiGlobalHolder plugin;
        private readonly byte[] buffer;
        private MidiStatus _status;
        private byte _channel;
        private byte _dataOne;
        private byte _dataTwo;

        public DataGlobal(MidiGlobalHolder plugin)
        {
            this.plugin = plugin;
            buffer = new byte[2];
        }

        public DataGlobal(int message, long timestamp, MidiGlobalHolder plugin) : this(plugin)
        {
            var data = BitConverter.GetBytes(message);

            this.timestamp = timestamp;
            _status = (MidiStatus)(data[0] >> 4);
            _channel = (byte)(0x0F & data[0]);
            Array.Copy(data, 1, buffer, 0, 2);
            _dataOne = buffer[0];
            _dataTwo = buffer[1];
        }

        private void IsReading()
        {
            plugin.GlobalHasUpdateListener = true;
        }

        internal int GetOutput()
        {
            var outArr = new byte[] { (byte)(((byte)_status << 4) + _channel), _dataOne, _dataTwo, 0 };
            return BitConverter.ToInt32(outArr, 0);
        }

        public long timestamp { get; private set; }

        public MidiStatus status
        {
            get
            {
                IsReading();
                return _status;
            }
            set
            {
                plugin.IsWriting();
                _status = value;
            }
        }

        public byte channel
        {
            get
            {
                IsReading();
                return _channel;
            }
            set
            {
                plugin.IsWriting();
                _channel = value;
            }
        }

        public byte dataOne
        {
            get
            {
                IsReading();
                return _dataOne;
            }
            set
            {
                plugin.IsWriting();
                _dataOne = value;
            }
        }

        public byte dataTwo
        {
            get
            {
                IsReading();
                return _dataTwo;
            }
            set
            {
                plugin.IsWriting();
                _dataTwo = value;
            }
        }
    }
}
