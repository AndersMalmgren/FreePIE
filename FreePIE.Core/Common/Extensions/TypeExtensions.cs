using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FreePIE.Core.Common.Extensions
{
    public static class TypeExtensions
    {
        public static IEnumerable<Type> GetTypes(this Type type)
        {
            return type.Assembly.GetTypes()
                .Where(t => !t.IsAbstract && (type.IsGenericType ? t.IsAssignableFrom(type.GetGenericTypeDefinition()) : type.IsAssignableFrom(t)))
                .ToList();
        }
    }
}
