using System;

namespace FreePIE.Core.Common
{
	public interface IFactory<out T> where T : class
	{
		T Create(Type type);
	}
}