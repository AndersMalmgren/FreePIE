using System;

namespace FreePIE.Core.ScriptEngine.Globals.ScriptHelpers
{
    [AttributeUsage(AttributeTargets.Method)]
    public class NeedIndexer : Attribute { }

    public interface IScriptHelper { }
}