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

    [LuaGlobalType(Type = typeof(FreeTrackGlobal))]
    public class FreeTrackPlugin : Plugin
    {
        private MemoryMappedFile memoryMappedFile;
        private MemoryMappedViewAccessor accessor;
        public FreeTrackData Data { get; set; }

        public override object CreateGlobal()
        {
            return new FreeTrackGlobal(this);
        }
        
        public override Action Start()
        {
            memoryMappedFile = MemoryMappedFile.CreateOrOpen("FT_SharedMem", Marshal.SizeOf(typeof(FreeTrackData)));
            accessor = memoryMappedFile.CreateViewAccessor();
            
            OnStarted(this, new EventArgs());

            return null;
        }

        public override void Stop()
        {
            accessor.Dispose();
            memoryMappedFile.Dispose();
        }

        public override string FriendlyName
        {
            get { return "FreeTrack"; }
        }

        public bool IsWriting { get; set; }
        private int oldData;
        public override void DoBeforeNextExecute()
        {
            if(IsWriting)
            {
                if(Data.DataID > oldData)
                {
                    Write();
                    oldData = Data.DataID;
                }
            }
            else
            {
                Read();
            }
        }

        private int sameDataCount = 0;
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
                local = new FreeTrackData();
                local.DataID = Data.DataID;
            }
            
            Data = local;
        }

        public void Write()
        {
            var local = Data;
            Data = local;
            accessor.Write(0, ref local);
        }
    }

    [LuaGlobal(Name = "freeTrack")]
    public class FreeTrackGlobal : UpdateblePluginGlobal
    {
        private readonly FreeTrackPlugin plugin;

        public FreeTrackGlobal(FreeTrackPlugin plugin) : base(plugin)
        {
            this.plugin = plugin;

        }
        
        public float getYaw()
        {
            return plugin.Data.Yaw;
        }

        public float getPitch()
        {
            return plugin.Data.Pitch;
        }

        public float getRoll()
        {
            return plugin.Data.Roll;
        }

        public float getX()
        {
            return plugin.Data.X;
        }

        public float getY()
        {
            return plugin.Data.Y;
        }

        public float getZ()
        {
            return plugin.Data.Z;
        }

        private void Write(Func<FreeTrackData, FreeTrackData> setValue)
        {
            plugin.IsWriting = true;
            var data = plugin.Data;
            data.DataID++;
            plugin.Data = setValue(data);
        }

        public void setYaw(float yaw)
        {
            Write(d => { d.Yaw = yaw; return d; });
        }

        public void setPitch(float pitch)
        {
            Write(d => { d.Pitch = pitch; return d; });
        }

        public void setRoll(float roll)
        {
            Write(d => { d.Roll = roll; return d; });
        }

        public void setX(float x)
        {
            Write(d => { d.X = x; return d; });
        }

        public void setY(float y)
        {
            Write(d => { d.Y = y; return d; });
        }

        public void setZ(float z)
        {
            Write(d => { d.Z = z; return d; });
        }
    }
}
