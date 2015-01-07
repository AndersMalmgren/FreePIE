using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.Globals;

namespace FreePIE.Core.Plugins
{
    [GlobalType(Type = typeof(MidiGlobal), IsIndexed = true)]
    public class MidiPlugin : Plugin
    {
        public override object CreateGlobal()
        {
            return new GlobalIndexer<MidiGlobal>(Create);
        }

        private MidiGlobal Create(int index)
        {
            var holder = new MidiGlobalHolder(index);
            return holder.Global;
        }

        public override string FriendlyName
        {
            get { return "Midi"; }
        }
    }

    public class MidiGlobalHolder : IUpdatable
    {
        private readonly Dictionary<int, byte> data;

        public MidiGlobalHolder(int index)
        {
            Global = new MidiGlobal(this);
            data = new Dictionary<int, byte>();
        }

        public Action OnUpdate { get; set; }
        public bool GlobalHasUpdateListener { get; set; }
        public MidiGlobal Global { get; private set; }

        public byte GetControl(int index)
        {
            if (!data.ContainsKey(index)) return 0;
            return data[index];
        }
    }

    [Global(Name = "midi")]
    public class MidiGlobal : UpdateblePluginGlobal<MidiGlobalHolder>
    {
        public MidiGlobal(MidiGlobalHolder plugin) : base(plugin)
        {
            
        }

        public byte getControl(int index)
        {
            return plugin.GetControl(index);
        }
    }
}
