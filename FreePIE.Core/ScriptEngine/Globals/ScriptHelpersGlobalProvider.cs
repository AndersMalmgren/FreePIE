using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            var targetType = typeof (IScriptHelper);

            return targetType.Assembly.GetTypes()
                .Where(t => targetType.IsAssignableFrom(t) && t.IsClass)
                .Select(t => scriptHelperFactory(t));
        }
    }
}
