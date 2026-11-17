using System;

namespace FreePIE.Core.Contracts
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Field)]
    public class Documentation : Attribute
    {
        public string Description { get; private set; }

        public Documentation(string description)
        {
            Description = description;
        }
    }
}
