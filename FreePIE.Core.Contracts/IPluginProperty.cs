using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.Core.Contracts
{
    public interface IPluginProperty
    {
        string Name { get; }
        object Value { get; }
        object DefaultValue { get; set; }
    }
}
