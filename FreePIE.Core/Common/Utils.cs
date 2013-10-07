using System;
using System.Collections.Generic;
using System.Linq;
using FreePIE.Core.Common.Extensions;

namespace FreePIE.Core.Common
{
    public static class Utils
    {
        public static IEnumerable<Type> GetTypes<T>()
        {
            return typeof (T).GetTypes();
        }

        public static IDictionary<T, Type> GetAttributeImplementations<T>() where T : Attribute
        {
            return typeof(T)
                .Assembly.GetTypes()
                .Where(t => !t.IsAbstract)
                .Select(t => new {Type = t, Attribute = t.GetCustomAttributes(typeof (T), false).SingleOrDefault()})
                .Where(t => t.Attribute != null)
                .ToDictionary(t => t.Attribute as T, t => t.Type);
        }
    }
}
