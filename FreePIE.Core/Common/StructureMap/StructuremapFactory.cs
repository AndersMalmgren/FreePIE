using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
