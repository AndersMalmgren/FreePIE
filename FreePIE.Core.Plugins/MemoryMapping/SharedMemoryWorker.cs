using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FreePIE.Core.Plugins.MemoryMapping
{
    public abstract class SharedMemoryWorker : IWorker, IDisposable
    {
        protected readonly MappedMemory<DisconnectedFreepieData> freePIEData;

        protected SharedMemoryWorker()
        {
            freePIEData = new MappedMemory<DisconnectedFreepieData>(DisconnectedFreepieData.SharedMemoryName);
        }

        public abstract void Execute(IEnumerable<string> arguments);
        public abstract void Dispose();
    }
}
