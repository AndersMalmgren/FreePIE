using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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
        private Generic6DOF[] data;
        private Generic6DOF[] remoteData;

        private bool[] newDataToWrite;
        private int[] sameDataCount;
        private const int deviceCount = 4;

        public override object CreateGlobal()
        {
            data = new Generic6DOF[deviceCount];
            remoteData = new Generic6DOF[deviceCount];
            newDataToWrite = new bool[deviceCount];
            sameDataCount = new int[deviceCount];

            return new int[deviceCount].Select((x, i) => new GenericPluginGlobal(this, d =>
            {
                if (!newDataToWrite[i])
                    d.DataId++;

                data[i] = d;
                newDataToWrite[i] = true;

            }, () => data[i])).ToList();
        }

        public override Action Start()
        {
            memoryMappedFile = MemoryMappedFile.CreateOrOpen("FPGeneric", Marshal.SizeOf(typeof(Generic6DOF)));
            accessor = memoryMappedFile.CreateViewAccessor();

            return null;
        }

        public override void DoBeforeNextExecute()
        {
            if (newDataToWrite.Any(nd => nd))
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

                if (local.DataId == remote.DataId)
                    sameDataCount[i]++;
                else
                {
                    sameDataCount[i] = 0;
                    OnUpdate();
                }

                if (sameDataCount[i] > 20)
                {
                    remote = new Generic6DOF {DataId = local.DataId};
                    OnUpdate();
                }

                data[i] = remote;
            }
        }

        private void Write()
        {
            accessor.WriteArray(0, data, 0, deviceCount);

            for (int i = 0; i < deviceCount; i++)
            {
                newDataToWrite[i] = false;
            }
        }

        public override string FriendlyName
        {
            get { return "Generic plugin"; }
        }
    }

    [Global(Name = "generic")]
    public class GenericPluginGlobal : UpdateblePluginGlobal<GenericPlugin>
    {
        private readonly Action<Generic6DOF> setter;
        private readonly Func<Generic6DOF> getter;

        public GenericPluginGlobal(GenericPlugin plugin, Action<Generic6DOF> setter, Func<Generic6DOF> getter) : base(plugin)
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
