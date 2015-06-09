using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreePIE.Core.Common.Lifetime;
using StructureMap;

namespace FreePIE.Core.Common.StructureMap
{
	internal sealed class StructureMapScopedContext<TEntryPoint> : IScopedContext<TEntryPoint> where TEntryPoint : class
	{
		private readonly IContainer nested;

		public StructureMapScopedContext(IContainer container)
		{
			nested = container.GetNestedContainer();
			nested.Configure(config => config.For<IScopedContext>().Use(this));
			EntryPoint = nested.GetInstance<TEntryPoint>();
		}

		public TEntryPoint EntryPoint { get; private set; }

		public void Dispose()
		{
			nested.Dispose();
		}
	}
}
