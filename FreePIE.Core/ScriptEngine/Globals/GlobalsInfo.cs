using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
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
            var typeAttribute = GetAttribute<LuaGlobalType>(type);
            var luaGlobalAttribute = typeAttribute != null ? GetAttribute<LuaGlobal>(typeAttribute.Type) : GetAttribute<LuaGlobal>(type);

            return luaGlobalAttribute != null ? luaGlobalAttribute.Name : null;
        }

        public static IEnumerable<string> GetGlobalMehods(Type pluginType)
        {
            var globalType = GetAttribute<LuaGlobalType>(pluginType).Type;
            var methods = new List<string>();
            foreach(var methodInfo in globalType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                methods.Add(methodInfo.Name);
            }

            return methods;
        }

        private static T GetAttribute<T>(Type type) where T : Attribute
        {
            return type.GetCustomAttributes(typeof (T), false).FirstOrDefault() as T;
        }
    }
}
