using System;
using System.Collections.Generic;
using CannedBytes.Midi;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.Globals;
using FreePIE.Core.Plugins.Midi;

namespace FreePIE.Core.Plugins
{
    [GlobalType(Type = typeof (MidiGlobal), IsIndexed = true)]
    public class MidiPlugin : Plugin
    {
        private List<MidiGlobalHolder> holders;

        public override object CreateGlobal()
        {
            holders = new List<MidiGlobalHolder>();
            return new GlobalIndexer<MidiGlobal>(Create);
        }

        private MidiGlobal Create(int index)
        {
            var holder = new MidiGlobalHolder(index);
            holders.Add(holder);
            return holder.Global;
        }

        public override void DoBeforeNextExecute()
        {
            holders.ForEach(h => h.Update());
        }

        public override void Stop()
        {
            holders.ForEach(h => h.Dispose());
        }

        public override string FriendlyName
        {
            get { return "MIDI"; }
        }
    }

    public class MidiGlobalHolder : IUpdatable, IMidiDataReceiver, IDisposable
    {
        private readonly Queue<int> messages = new Queue<int>();
        private readonly MidiInPort port;


        public MidiGlobalHolder(int index)
        {
            Global = new MidiGlobal(this);
            port = new MidiInPort();
            port.Successor = this;

            var count = new MidiInPortCapsCollection().Count;
            if(index >= count) 
                throw new Exception(string.Format("MIDI device with port index {0} not found", index));

            port.Open(index);
            port.Start();
        }

        public void Update()
        {
            if (messages.Count != 0)
            {
                lock (messages)
                {
                    while (messages.Count > 0)
                    {
                        lastData = new DataGlobal(messages.Dequeue());
                        OnUpdate();
                    }
                }
            }
        }

        public Action OnUpdate { get; set; }
        public bool GlobalHasUpdateListener { get; set; }
        public MidiGlobal Global { get; private set; }

        private DataGlobal lastData;

        public DataGlobal Data
        {
            get { return lastData; }
        }

        public void ShortData(int data, long timestamp)
        {
            lock (messages)
            {
                messages.Enqueue(data);
            }
        }

        public void Dispose()
        {
            port.Stop();
            port.Close();
        }

        public void LongData(MidiBufferStream buffer, long timestamp){ }
    }

    [Global(Name = "midi")]
    public class MidiGlobal : UpdateblePluginGlobal<MidiGlobalHolder>
    {
        public MidiGlobal(MidiGlobalHolder plugin)
            : base(plugin)
        {

        }

        public DataGlobal data
        {
            get { return plugin.Data; }
        }
    }
}

