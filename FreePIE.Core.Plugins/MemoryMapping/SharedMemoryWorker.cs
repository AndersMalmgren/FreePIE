using System.Collections.Generic;

namespace FreePIE.Core.Plugins.MemoryMapping
{
    public abstract class SharedMemoryWorker : IWorker
    {
        protected readonly MappedMemory<DisconnectedFreepieData> freePIEData;

        protected SharedMemoryWorker()
        {
            freePIEData = new MappedMemory<DisconnectedFreepieData>(DisconnectedFreepieData.SharedMemoryName);
        }

        public void Execute(IEnumerable<string> arguments)
        {
            DoExecute(arguments);
        }

        protected abstract void DoExecute(IEnumerable<string> arguments);

        public virtual void Dispose()
        {
            freePIEData.Dispose();
        }
    }
}
