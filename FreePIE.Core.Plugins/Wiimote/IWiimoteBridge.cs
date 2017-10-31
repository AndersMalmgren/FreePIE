using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.Core.Plugins.Wiimote
{
    public enum LogLevel { Debug = 0, Info = 1, Warning = 2, Error = 3 }

    public class UpdateEventArgs<T> : EventArgs
    {
        public UpdateEventArgs(T updatedValue)
        {
            UpdatedValue = updatedValue;
        }

        public T UpdatedValue { get; private set; }
    }

    public interface IWiimoteBridge : IDisposable
    {
        void DoTick();
        event EventHandler<UpdateEventArgs<uint>> DataReceived;
        event EventHandler<UpdateEventArgs<uint>> CapabilitiesChanged;
        event EventHandler<UpdateEventArgs<uint>> StatusChanged;
        IWiimoteData GetData(uint i);
        void Shutdown();
        void Init();
        void Enable(byte wiimote, WiimoteCapabilities flags);
        void SetRumble(byte wiimote, Boolean shouldRumble);
        void RequestStatus(byte wiimote);
    }
}
