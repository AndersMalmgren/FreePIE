using System;
using System.Collections.Generic;
using System.Linq;
using StructureMap.Configuration.DSL;
using StructureMap.Graph;
using StructureMap.Pipeline;
using StructureMap.TypeRules;

namespace FreePIE.Core.Common.StructureMap
{
    internal sealed class ConcreteTypeRegistrationConvention : IRegistrationConvention
    {
        private readonly HashSet<Type> lookup = new HashSet<Type>();

        public void Process(Type type, Registry registry)
        {
            if (lookup.Contains(type))
                return;

            if(!type.IsConcrete())
                return;
            if(type.GetConstructors().All(c => c.GetParameters().Length == 0))
                return;

            lookup.Add(type);
            registry.For(type, new UniquePerRequestLifecycle());
        }
    }
}
