using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using FreePIE.Core.Contracts;
using FreePIE.Core.Model;
using FreePIE.Core.Plugins;
using FreePIE.Core.ScriptEngine.Globals;
using FreePIE.Core.ScriptEngine.Globals.ScriptHelpers;

namespace FreePIE.Core.ScriptEngine.CodeCompletion
{
    internal static class InfoTransformHelper
    {
        public static Node<TokenInfo> ConstructExpressionInfoTree(IPluginInvoker invoker, IEnumerable<IGlobalProvider> providers)
        {
            var root = new Node<ExpressionInfo>();

            root.AddChildren(invoker.ListAllPluginTypes().Select(type => type.ToExpressionInfo()));
            root.AddChildren(invoker.ListAllGlobalEnumTypes().Select(type => type.ToExpressionInfo()));
            root.AddChildren(providers.SelectMany(gp => gp.ListGlobals().Select(obj => obj.ToExpressionInfo())));

            root.SortChildrenRecursive((a, b) => a.Name.CompareTo(b.Name));

            return new Node<TokenInfo>(new TokenInfo(new Token(TokenType.Identifier, "PLACEHOLDER"), new ExpressionInfo()));
        }

        private static Node<ExpressionInfo> ToExpressionInfo(this Type type)
        {
            return type.IsEnum ? MapEnum(type) : MapClass(type);
        }

        private static Node<ExpressionInfo> ToExpressionInfo(this object obj)
        {
            Type type = obj.GetType();

            return type.IsEnum ? MapEnum(type) : MapClass(obj);
        }

        private static Node<ExpressionInfo> MapClass(Type type)
        {
            var node = MapClassType(type);

            node.Value.Name = GlobalsInfo.GetGlobalName(type);

            AddClassMembers(node, type);

            return node;
        }

        private static bool HasIndexer(Type type)
        {
            var globalType = type.GetCustomAttributes(true).SingleOrDefault(x => x is LuaGlobalType) as LuaGlobalType;

            return globalType != null && globalType.IsIndexed;
        }

        private static void AddClassMembers(Node<ExpressionInfo> node, Type type)
        {
            var globalMembers = GlobalsInfo.GetGlobalMembers(type).ToList();

            node.AddChildren(globalMembers.Where(m => m.MemberType == MemberTypes.Event).Select(m => MapEvent(m as EventInfo)));
            node.AddChildren(globalMembers.Where(m => m.MemberType == MemberTypes.Method).Select(m => MapMethod(m as MethodInfo)));
        }

        private static Node<ExpressionInfo> MapClassType(Type type)
        {
            return HasIndexer(type) ? new Node<ExpressionInfo>(new IndexedExpressionInfo()) : new Node<ExpressionInfo>(new ExpressionInfo());
        }

        private static Node<ExpressionInfo> MapClass(object obj)
        {
            Type type = obj.GetType();

            string name = GlobalsInfo.GetGlobalName(type) ?? (obj as IGlobalNameProvider).Name;

            Node<ExpressionInfo> node = MapClassType(type);

            node.Value.Name = name;

            AddClassMembers(node, type);

            return node;
        }

        private static ExpressionInfo MapEvent(EventInfo ei)
        {
            var expInfo = new ExpressionInfo();
            expInfo.Name = ei.Name;
            expInfo.Description = "Event";

            return expInfo;
        }

        private static ExpressionInfo MapMethod(MethodInfo mi)
        {
            var expInfo = new ExpressionInfo();
            expInfo.Name = mi.Name;

            var parameters = string.Join(",", GetParametersWithoutIndexer(mi).Select(pi => string.Format("{0} {1}", pi.ParameterType.Name, pi.Name)));
            expInfo.Description = string.Format("{0} {1} ({2})", mi.ReturnType.Name == "Void" ? "void" : mi.ReturnType.Name, mi.Name, parameters);

            return expInfo;
        }

        private static IEnumerable<ParameterInfo> GetParametersWithoutIndexer(MethodInfo mi)
        {
            var parameters = mi.GetParameters();
            return mi.GetCustomAttributes(typeof(NeedIndexer), false).Length > 0 ? parameters.Take(parameters.Length - 1) : parameters;
        }

        private static Node<ExpressionInfo> MapEnum(Type type)
        {
            var expInfo = new Node<ExpressionInfo>(new ExpressionInfo { Name = type.Name, Description = "Enum" });

            expInfo.AddChildren(Enum.GetNames(type).Select(name => new ExpressionInfo { Name = name, Description = string.Format("{0} = {1}", name, (int)Enum.Parse(type, name)) }));

            return expInfo;
        }
    }
}
