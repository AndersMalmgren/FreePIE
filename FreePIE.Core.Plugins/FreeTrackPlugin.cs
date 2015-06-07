using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using FreePIE.Core.Contracts;
using Microsoft.Win32;

namespace FreePIE.Core.Plugins
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FreeTrackData
    {
        public int DataID;
        public int CamWidth;
        public int CamHeight;
        // virtual pose
        public float Yaw;   // positive yaw to the left
        public float Pitch; // positive pitch up
        public float Roll;  // positive roll to the left
        public float X;
        public float Y;
        public float Z;
        // raw pose with no smoothing, sensitivity, response curve etc. 
        public float RawYaw;
        public float RawPitch;
        public float RawRoll;
        public float RawX;
        public float RawY;
        public float RawZ;
        // raw points, sorted by Y, origin top left corner
        public float X1;
        public float Y1;
        public float X2;
        public float Y2;
        public float X3;
        public float Y3;
        public float X4;
        public float Y4;
    };

    [GlobalType(Type = typeof(FreeTrackGlobal))]
    public class FreeTrackPlugin : Plugin
    {
        private const string keyName = "Software\\Freetrack\\FreetrackClient";
        private const string valueName = "Path";
        private const string dllName = "FreeTrackClient.dll";

        private string key;
        private string oldRegistryPath;

        private int sameDataCount;

        private MemoryMappedFile memoryMappedFile;
        private MemoryMappedViewAccessor accessor;
        public FreeTrackData Data { get; set; }

        public override object CreateGlobal()
        {
            return new FreeTrackGlobal(this);
        }
        
        public override Action Start()
        {
            key = Path.Combine(Registry.CurrentUser.ToString(), keyName);
            RegisterDll();

            memoryMappedFile = MemoryMappedFile.CreateOrOpen("FT_SharedMem", Marshal.SizeOf(typeof(FreeTrackData)));
            accessor = memoryMappedFile.CreateViewAccessor();
            
            OnStarted(this, new EventArgs());

            return null;
        }

        private void RegisterDll()
        {
            oldRegistryPath = Registry.GetValue(key, valueName, null) as string;
            SetRegistry(Environment.CurrentDirectory);
        }

        private void UnregisterDll()
        {
            if (oldRegistryPath != null)
                SetRegistry(oldRegistryPath);
        }

        private void SetRegistry(string path)
        {
            Registry.SetValue(key, valueName, path);
        }

        public override void Stop()
        {
            accessor.Dispose();
            memoryMappedFile.Dispose();

            UnregisterDll();
        }

        public override string FriendlyName
        {
            get { return "FreeTrack"; }
        }

        public bool IsWriting { get; set; }
        public bool NewDataToWrite { get; set; }

        public override void DoBeforeNextExecute()
        {
            if(IsWriting)
            {
                if (NewDataToWrite)
                {
                    Write();
                    NewDataToWrite = false;
                }
            }
            else
            {
                Read();
            }
        }

        public void Read()
        {
            FreeTrackData local;
            accessor.Read(0, out local);
            if (local.DataID == Data.DataID)
                sameDataCount++;
            else
            {
                sameDataCount = 0;
                OnUpdate();
            }

            if (sameDataCount > 20)
            {
                local = new FreeTrackData {DataID = Data.DataID};
                OnUpdate();
            }
            
            Data = local;
        }

        public void Write()
        {
            var local = Data;
            local.DataID++;
			local.CamWidth = 1920;
			local.CamHeight = 1080; 

            Data = local;
            accessor.Write(0, ref local);
        }
    }

    [Global(Name = "freeTrack")]
    public class FreeTrackGlobal : UpdateblePluginGlobal<FreeTrackPlugin>
    {
        public FreeTrackGlobal(FreeTrackPlugin plugin) : base(plugin) { }

        private void Write(Func<FreeTrackData, FreeTrackData> setValue)
        {
            plugin.IsWriting = true;
            plugin.NewDataToWrite = true;

            var data = plugin.Data;
            plugin.Data = setValue(data);
        }

        public float yaw
        {
            get { return plugin.Data.Yaw; }
            set { Write(d => { d.Yaw = value; return d; }); }
        }

        public float pitch
        {
            get { return plugin.Data.Pitch; }
            set { Write(d => { d.Pitch = value; return d; }); }
        }

        public float roll
        {
            get { return plugin.Data.Roll; }
            set { Write(d => { d.Roll = value; return d; }); }
        }

        public float x
        {
            get { return plugin.Data.X; }
            set { Write(d => { d.X = value; return d; }); }
        }

        public float y
        {
            get { return plugin.Data.Y; }
            set { Write(d => { d.Y = value; return d; }); }
        }

        public float z
        {
            get { return plugin.Data.Z; }
            set { Write(d => { d.Z = value; return d; }); }
        }
    }
}
