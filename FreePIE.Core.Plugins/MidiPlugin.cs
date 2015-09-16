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
        private readonly int index;
        private readonly Queue<DataGlobal> messages = new Queue<DataGlobal>();
        private MidiInPort inPort;
        private MidiOutPort outPort;
        private DataGlobal lastData;
        private bool isWriting;
        private bool newDataToWrite;

        public MidiGlobalHolder(int index)
        {
            this.index = index;
            lastData = new DataGlobal(this);
            Global = new MidiGlobal(this);
        }

        private void InitRead()
        {
            inPort = new MidiInPort {Successor = this};

            var count = new MidiInPortCapsCollection().Count;
            if (index >= count)
                throw new Exception(string.Format("MIDI device with port index {0} not found", index));

            inPort.Open(index);
            inPort.Start();
        }

        public void IsWriting()
        {
            isWriting = true;
            newDataToWrite = true;
        }

        public void Update()
        {
            Write();
            Read();
        }

        private void Read()
        {
            if(GlobalHasUpdateListener && inPort == null)
                InitRead();

            if (messages.Count != 0)
            {
                lock (messages)
                {
                    while (messages.Count > 0)
                    {
                        lastData = messages.Dequeue();
                        OnUpdate();
                    }
                }
            }
        }

        private void Write()
        {
            if (!isWriting || !newDataToWrite) return;


            if (outPort == null)
            {
                outPort = new MidiOutPort();
                outPort.Open(index);
            }

            outPort.ShortData(lastData.GetOutput());
            newDataToWrite = false;
        }

        public Action OnUpdate { get; set; }
        public bool GlobalHasUpdateListener { get; set; }
        public MidiGlobal Global { get; private set; }
        
        public DataGlobal Data
        {
            get { return lastData; }
        }

        public void ShortData(int data, long timestamp)
        {
            lock (messages)
            {
                messages.Enqueue(new DataGlobal(data, timestamp, this));
            }
        }

        public void Dispose()
        {
            if (inPort != null)
            {
                inPort.Stop();
                inPort.Close();
                inPort.Dispose();
            }

            if (outPort != null)
            {
                outPort.Close();
                outPort.Dispose();
            }
        }

        public void LongData(MidiBufferStream buffer, long timestamp){ }
    }

    [Global(Name = "midi")]
    public class MidiGlobal : UpdateblePluginGlobal<MidiGlobalHolder>
    {
        public MidiGlobal(MidiGlobalHolder plugin) : base(plugin) { }

        public DataGlobal data
        {
            get { return plugin.Data; }
        }
    }
}

