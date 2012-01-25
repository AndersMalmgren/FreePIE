using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.Core.Contracts
{
    public interface IOPlugin
    {
        object CreateGlobal();
        Action Start();
        void Stop();
        event EventHandler Started;

        bool GetProperty(int index, IPluginProperty property);
        bool SetProperties(Dictionary<string, object> properties);
        void DoBeforeNextExecute();
    }
}
