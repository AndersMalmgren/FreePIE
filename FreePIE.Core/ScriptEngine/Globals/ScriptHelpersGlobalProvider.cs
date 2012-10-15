using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreePIE.Core.Common;
using FreePIE.Core.ScriptEngine.Globals.ScriptHelpers;

namespace FreePIE.Core.ScriptEngine.Globals
{
    public class ScriptHelpersGlobalProvider : IGlobalProvider
    {
        private readonly Func<Type, IScriptHelper> scriptHelperFactory;

        public ScriptHelpersGlobalProvider(Func<Type, IScriptHelper> scriptHelperFactory)
        {
            this.scriptHelperFactory = scriptHelperFactory;
        }

        public IEnumerable<object> ListGlobals()
        {
            return Utils.GetTypes<IScriptHelper>()
                .Select(t => scriptHelperFactory(t));
        }
    }
}
