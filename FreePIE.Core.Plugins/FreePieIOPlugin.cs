using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.Plugins
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FreePieIO6Dof
    {
        public int DataId;

        public float Yaw;
        public float Pitch;
        public float Roll;

        public float X;
        public float Y;
        public float Z;
    }

    [GlobalType( Type = typeof(FreePieIOPluginGlobal), IsIndexed = true)]
    public class FreePieIOPlugin : Plugin
    {
        private MemoryMappedFile memoryMappedFile;
        private MemoryMappedViewAccessor accessor;
        private const int deviceCount = 4;
        
        private FreePieIO6Dof[] data;
        private FreePieIO6Dof[] remoteData;
        private List<FrePieIOPluginHolder> holders;

        public override object CreateGlobal()
        {
            data = new FreePieIO6Dof[deviceCount];
            remoteData = new FreePieIO6Dof[deviceCount];

            holders = data.Select((x, i) => new FrePieIOPluginHolder(data, i)).ToList();
            return holders.Select(h => h.Global).ToArray();
        }

        public override Action Start()
        {
            memoryMappedFile = MemoryMappedFile.CreateOrOpen("FPGeneric", Marshal.SizeOf(typeof(FreePieIO6Dof)) * deviceCount);
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
                    remote = new FreePieIO6Dof {DataId = local.DataId};
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
            get { return "FreePIE IO Plugin"; }
        }
    }

    public class FrePieIOPluginHolder : IUpdatable
    {
        public FrePieIOPluginHolder(FreePieIO6Dof[] data, int index)
        {
            SameDataCountCanTriggerUpdate = true;
            Global = new FreePieIOPluginGlobal(this, d =>
                {
                    if (!NewDataToWrite)
                        d.DataId++;

                    data[index] = d;

                    NewDataToWrite = true;
                }, () => data[index]);
        }

        public FreePieIOPluginGlobal Global { get; private set; }
        public bool NewDataToWrite { get; set; }
        public int SameDataCount { get; set; }
        public bool SameDataCountCanTriggerUpdate { get; set; }

        public Action OnUpdate { get; set; }
        public bool GlobalHasUpdateListener { get; set; }
    }

    [Global(Name = "freePieIO")]
    public class FreePieIOPluginGlobal : UpdateblePluginGlobal<FrePieIOPluginHolder>
    {
        private readonly Action<FreePieIO6Dof> setter;
        private readonly Func<FreePieIO6Dof> getter;

        public FreePieIOPluginGlobal(FrePieIOPluginHolder plugin, Action<FreePieIO6Dof> setter, Func<FreePieIO6Dof> getter) : base(plugin)
        {
            this.setter = setter;
            this.getter = getter;
        }

        private void Write(Func<FreePieIO6Dof, FreePieIO6Dof> setValue)
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
