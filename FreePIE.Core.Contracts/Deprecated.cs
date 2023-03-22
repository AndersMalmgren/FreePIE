using System;

namespace FreePIE.Core.Contracts
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public class Deprecated : Attribute
    {
        public string ReplacedWith { get; private set; }

        public Deprecated(string replacedWith)
        {
            ReplacedWith = replacedWith;
        }
    }
}
