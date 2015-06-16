using System;
using StructureMap;

namespace FreePIE.Core.Common.StructureMap
{
    internal sealed class StructureMapFactory<T> : IFactory<T> where T : class
    {
        private readonly IContainer container;
        public StructureMapFactory(IContainer container)
        {
            this.container = container;
        }

        public T Create(Type type)
        {
            return container.GetInstance(type) as T;
        }
    }
}
