using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreePIE.Core.Plugins
{
    public interface IPluginDataSource
    {
        IEnumerable<Type> ListAllPluginTypes();
        IEnumerable<Type> ListAllGlobalEnumTypes();
    }
}
