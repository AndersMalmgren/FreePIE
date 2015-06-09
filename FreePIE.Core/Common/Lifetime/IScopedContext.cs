using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreePIE.Core.Common.Lifetime
{
	public interface IScopedContext : IDisposable
	{
		
	}

	public interface IScopedContext<out TEntryPoint> : IScopedContext
	{
		TEntryPoint EntryPoint { get; }
	}
}
