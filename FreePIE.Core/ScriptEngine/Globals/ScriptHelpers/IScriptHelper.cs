using System;

namespace FreePIE.Core.ScriptEngine.Globals.ScriptHelpers
{
    [AttributeUsage(AttributeTargets.Method)]
    public class NeedIndexer : Attribute { }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public class Deprecated : Attribute
    {
        public string ReplacedWith { get; private set; }

        public Deprecated(string replacedWith)
        {
            ReplacedWith = replacedWith;
        }
    }

    public interface IScriptHelper { }
}