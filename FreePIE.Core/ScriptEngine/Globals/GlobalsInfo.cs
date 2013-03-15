using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.ScriptEngine.Globals
{
    public static class GlobalsInfo
    {
        public static string GetGlobalName(object global)
        {
            var name = GetGlobalName(global.GetType());

            if (name == null)
            {
                return (global as IGlobalNameProvider).Name;
            }

            return name;
        }

        public static string GetGlobalName(Type type)
        {
            var typeAttribute = GetAttribute<GlobalType>(type);
            var globalAttribute = typeAttribute != null ? GetAttribute<Global>(typeAttribute.Type) : GetAttribute<Global>(type);

            return globalAttribute != null ? globalAttribute.Name : null;
        }

        public static IEnumerable<MemberInfo> GetGlobalMembers(Type pluginType)
        {
            var globalType = GetAttribute<GlobalType>(pluginType);
            pluginType = globalType != null ? globalType.Type : pluginType;

            return GetMembers(pluginType, new List<MemberInfo>());
        }

        private static IEnumerable<MemberInfo> GetMembers(Type type, List<MemberInfo> members)
        {
            if (type == typeof(object))
                return members;

            members.AddRange(type.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly));
            return GetMembers(type.BaseType, members);
        }

        public static IEnumerable<string> GetGlobalMehods(Type pluginType)
        {
            var globalType = GetAttribute<GlobalType>(pluginType).Type;

            return globalType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Select(methodInfo => methodInfo.Name).ToList();
        }

        private static T GetAttribute<T>(Type type) where T : Attribute
        {
            return type.GetCustomAttributes(typeof (T), false).FirstOrDefault() as T;
        }
    }
}
