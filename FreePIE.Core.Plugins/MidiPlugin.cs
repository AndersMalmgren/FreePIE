using System;
using System.Collections.Generic;
using System.Linq;
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
            var ports = new MidiInPortCapsCollection()
                .Select((p, i) => new MidiPortInfo { Index = i, InPort = true, Name = p.Name})
                .Concat(new MidiOutPortCapsCollection().Select((p, i) => new MidiPortInfo { Index = i, InPort = false, Name = p.Name }))
                .ToList();

            return new GlobalIndexer<MidiGlobal, int, string>(index => Create(ports.Where(p => p.Index == index)), name => Create(ports.Where(p => p.Name == name)));
        }

        private MidiGlobal Create(IEnumerable<MidiPortInfo> ports)
        {
            var holder = new MidiGlobalHolder(ports);
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

    public class MidiPortInfo
    {
        public bool InPort { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }
    }

    public class MidiGlobalHolder : IUpdatable, IMidiDataReceiver, IDisposable
    {
        private readonly IEnumerable<MidiPortInfo> ports;
        private readonly Queue<DataGlobal> messages = new Queue<DataGlobal>();
        private MidiInPort inPort;
        private MidiOutPort outPort;
        private DataGlobal lastData;
        private bool isWriting;
        private bool newDataToWrite;

        public MidiGlobalHolder(IEnumerable<MidiPortInfo> ports)
        {
            if(!ports.Any())
                throw new Exception("Requested MIDI device not found with given key");

            this.ports = ports;
            lastData = new DataGlobal(this);
            Global = new MidiGlobal(this);
        }

        private void InitRead()
        {
            inPort = new MidiInPort {Successor = this};

            var port = ports.SingleOrDefault(p => p.InPort);
            if (port == null)
                throw new Exception("Requested MIDI device with inport not found");

            inPort.Open(port.Index);
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
                InitWrite();

            outPort.ShortData(lastData.GetOutput());
            newDataToWrite = false;
        }

        private void InitWrite()
        {
            var port = ports.SingleOrDefault(p => !p.InPort);
            if (port == null)
                throw new Exception("Requested MIDI device with outport not found");

            outPort = new MidiOutPort();
            outPort.Open(port.Index);
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

