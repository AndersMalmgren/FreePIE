using System;
using System.Collections.Generic;
using System.Linq;
using FreePIE.Core.Common;
using FreePIE.Core.ScriptEngine.Globals.ScriptHelpers;

namespace FreePIE.Core.ScriptEngine.Globals
{
    public class ScriptHelpersGlobalProvider : IGlobalProvider
    {
        private readonly IFactory<IScriptHelper> scriptHelperFactory;

        public ScriptHelpersGlobalProvider(IFactory<IScriptHelper> scriptHelperFactory)
        {
            this.scriptHelperFactory = scriptHelperFactory;
        }

        public IEnumerable<object> ListGlobals()
        {
            return Utils.GetTypes<IScriptHelper>()
                .Select(scriptHelperFactory.Create);
        }
    }
}
