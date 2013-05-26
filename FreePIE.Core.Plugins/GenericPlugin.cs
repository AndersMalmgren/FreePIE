using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.Plugins
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Generic6DOF
    {
        public int DataId;

        public float Yaw;
        public float Pitch;
        public float Roll;

        public float X;
        public float Y;
        public float Z;
    }

    [GlobalType( Type = typeof(GenericPluginGlobal), IsIndexed = true)]
    public class GenericPlugin : Plugin
    {
        private MemoryMappedFile memoryMappedFile;
        private MemoryMappedViewAccessor accessor;
        private const int deviceCount = 4;
        
        private Generic6DOF[] data;
        private Generic6DOF[] remoteData;
        private List<GenericPluginHolder> holders;

        public override object CreateGlobal()
        {
            data = new Generic6DOF[deviceCount];
            remoteData = new Generic6DOF[deviceCount];

            holders = data.Select((x, i) => new GenericPluginHolder(data, i)).ToList();
            return holders.Select(h => h.Global).ToArray();
        }

        public override Action Start()
        {
            memoryMappedFile = MemoryMappedFile.CreateOrOpen("FPGeneric", Marshal.SizeOf(typeof(Generic6DOF)));
            accessor = memoryMappedFile.CreateViewAccessor();

            return null;
        }

        public override void DoBeforeNextExecute()
        {
            if (holders.Any(h => h.NewDataToWrite))
                Write();
            else
                Read();
        }

        private void Read()
        {
            accessor.ReadArray(0, remoteData, 0, deviceCount);

            for (int i = 0; i < deviceCount; i++)
            {
                var local = data[i];
                var remote = remoteData[i];
                var holder = holders[i];

                if (local.DataId == remote.DataId)
                    holder.SameDataCount++;
                else
                {
                    holder.SameDataCount = 0;
                    holder.SameDataCountCanTriggerUpdate = true;
                    data[i] = remote;
                    holder.OnUpdate();
                }

                if (holder.SameDataCount > 20 && holder.SameDataCountCanTriggerUpdate)
                {
                    holder.SameDataCountCanTriggerUpdate = false;
                    remote = new Generic6DOF {DataId = local.DataId};
                    data[i] = remote;
                    holder.OnUpdate();
                }
            }
        }

        private void Write()
        {
            accessor.WriteArray(0, data, 0, deviceCount);
            holders.ForEach(h => h.NewDataToWrite = false);
        }

        public override string FriendlyName
        {
            get { return "Generic plugin"; }
        }
    }

    public class GenericPluginHolder : IUpdatable
    {
        public GenericPluginHolder(Generic6DOF[] data, int index)
        {
            SameDataCountCanTriggerUpdate = true;
            Global = new GenericPluginGlobal(this, d =>
                {
                    if (!NewDataToWrite)
                        d.DataId++;

                    data[index] = d;

                    NewDataToWrite = true;
                }, () => data[index]);
        }

        public GenericPluginGlobal Global { get; private set; }
        public bool NewDataToWrite { get; set; }
        public int SameDataCount { get; set; }
        public bool SameDataCountCanTriggerUpdate { get; set; }

        public Action OnUpdate { get; set; }
        public bool GlobalHasUpdateListener { get; set; }
    }

    [Global(Name = "generic")]
    public class GenericPluginGlobal : UpdateblePluginGlobal<GenericPluginHolder>
    {
        private readonly Action<Generic6DOF> setter;
        private readonly Func<Generic6DOF> getter;

        public GenericPluginGlobal(GenericPluginHolder plugin, Action<Generic6DOF> setter, Func<Generic6DOF> getter) : base(plugin)
        {
            this.setter = setter;
            this.getter = getter;
        }

        private void Write(Func<Generic6DOF, Generic6DOF> setValue)
        {
            var data = getter();
            setter(setValue(data));
        }

        public float yaw
        {
            get { return getter().Yaw; }
            set { Write(d => { d.Yaw = value; return d; }); }
        }

        public float pitch
        {
            get { return getter().Pitch; }
            set { Write(d => { d.Pitch = value; return d; }); }
        }

        public float roll
        {
            get { return getter().Roll; }
            set { Write(d => { d.Roll = value; return d; }); }
        }

        public float x
        {
            get { return getter().X; }
            set { Write(d => { d.X = value; return d; }); }
        }

        public float y
        {
            get { return getter().Y; }
            set { Write(d => { d.Y = value; return d; }); }
        }

        public float z
        {
            get { return getter().Z; }
            set { Write(d => { d.Z = value; return d; }); }
        }
    }
}
