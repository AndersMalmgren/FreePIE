using System;
using System.Collections.Generic;

namespace FreePIE.Core.Contracts
{
    public delegate void GlobalNoArgumentEvent();

    public interface IPlugin
    {
        object CreateGlobal();
        Action Start();
        void Stop();
        event EventHandler Started;

        string FriendlyName { get; }
        bool GetProperty(int index, IPluginProperty property);
        bool SetProperties(Dictionary<string, object> properties);
        void DoBeforeNextExecute();
    }
}
