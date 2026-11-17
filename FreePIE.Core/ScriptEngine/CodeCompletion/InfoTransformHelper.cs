using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FreePIE.Core.Contracts;
using FreePIE.Core.Model;
using FreePIE.Core.Plugins;
using FreePIE.Core.ScriptEngine.Globals;
using FreePIE.Core.ScriptEngine.Globals.ScriptHelpers;

namespace FreePIE.Core.ScriptEngine.CodeCompletion
{
    internal static class InfoTransformHelper
    {
        private static readonly IEnumerable<MemberInfo> ObjectMembers = typeof(object).GetMembers();

        public static Node<TokenInfo> ConstructExpressionInfoTree(IPluginInvoker invoker, IEnumerable<IGlobalProvider> providers)
        {
            var root = new Node<TokenInfo>();

            root.AddChildren(invoker.ListAllPluginTypes().Select(type => type.ToTokenInfo()));
            root.AddChildren(invoker.ListAllGlobalEnumTypes().Select(type => type.ToTokenInfo()));
            root.AddChildren(providers.SelectMany(gp => gp.ListGlobals().Select(obj => obj.ToTokenInfo())));

            root.SortChildrenRecursive((a, b) => a.Identifier.Value.CompareTo(b.Identifier.Value));

            return root;
        }

        private static Node<TokenInfo> ToTokenInfo(this Type type)
        {
            return type.IsEnum ? MapEnum(type) : MapClass(type);
        }

        private static Node<TokenInfo> ToTokenInfo(this object obj)
        {
            Type type = obj.GetType();

            return type.IsEnum ? MapEnum(type) : MapClass(obj);
        }

        private static Node<TokenInfo> MapClass(Type type)
        {
            var node = MapClassType(type);
            node.Value.Info.Kind = ExpressionKind.Global;
            node.Value.Info.Description = GetGlobalDocumentation(type);

            var name = GlobalsInfo.GetGlobalName(type);

            node.SetName(name);

            AddClassMembers(node, type);

            return node;
        }

        private static void SetName(this Node<TokenInfo> node, string name)
        {
            node.Value.Identifier.Value = name;
            node.Value.Info.Name = name;
        }

        private static bool HasIndexer(Type type)
        {
            var globalType = type.GetCustomAttributes(true).SingleOrDefault(x => x is GlobalType) as GlobalType;

            return globalType != null && globalType.IsIndexed;
        }

        // Plugins are usually documented on the global class, but the plugin class is also accepted
        private static string GetGlobalDocumentation(Type type)
        {
            var globalType = type.GetCustomAttributes(typeof(GlobalType), false).FirstOrDefault() as GlobalType;
            var documentation = globalType != null ? GetDocumentation(globalType.Type) : null;

            return documentation ?? GetDocumentation(type) ?? string.Empty;
        }

        private static bool IsDeprecated(MemberInfo info)
        {
            return info.GetCustomAttributes(typeof (Deprecated), false).Length > 0;
        }

        private static string GetDocumentation(MemberInfo info)
        {
            var documentation = info.GetCustomAttributes(typeof(Documentation), true).FirstOrDefault() as Documentation;

            return documentation != null ? documentation.Description : null;
        }

        private static void AddClassMembers(Node<TokenInfo> node, Type type)
        {
            var globalMembers = GlobalsInfo.GetGlobalMembers(type).ToList();

            var dotDelim = node.AddChild(ConstructDelimiterNode('.'));

            AddEvents(dotDelim, globalMembers);
            AddProperties(dotDelim, globalMembers);

            AddMethods(dotDelim, globalMembers, true);
        }

        private static void AddProperties(Node<TokenInfo> propDelim, List<MemberInfo> members)
        {
            propDelim.AddChildren(members.Where(m => m.MemberType == MemberTypes.Property && !IsDeprecated(m)).Select(m => MapProperty(m as PropertyInfo)));
        }

        private static void AddEvents(Node<TokenInfo> eventDelim, IEnumerable<MemberInfo> members)
        {
            eventDelim.AddChildren(members.Where(m => m.MemberType == MemberTypes.Event).Select(m => MapEvent(m as EventInfo)));
        }

        private static void AddMethods(Node<TokenInfo> methodDelim, IEnumerable<MemberInfo> members, bool withReturnedMembers)
        {
            methodDelim.AddChildren(members.Where(m => m.MemberType == MemberTypes.Method && !(m as MethodInfo).IsSpecialName && !IsDeprecated(m)).Select(m => MapMethod(m as MethodInfo, withReturnedMembers)));
        }

        private static Node<TokenInfo> MapClassType(Type type)
        {
            var tokenInfo = HasIndexer(type)
                                ? new TokenInfo(IndexedToken.EmptyIndexed(), new IndexedExpressionInfo())
                                : new TokenInfo(Token.Empty(), new ExpressionInfo());

            return new Node<TokenInfo>(tokenInfo);
        }

        private static Node<TokenInfo> MapClass(object obj)
        {
            Type type = obj.GetType();

            string name = GlobalsInfo.GetGlobalName(type) ?? (obj as IGlobalNameProvider).Name;

            Node<TokenInfo> node = MapClassType(type);
            node.Value.Info.Kind = ExpressionKind.Global;
            node.Value.Info.Description = GetGlobalDocumentation(type);

            node.SetName(name);

            AddClassMembers(node, type);

            return node;
        }

        private static Node<TokenInfo> ConstructDelimiterNode(char delimiter)
        {
            return new Node<TokenInfo>(new TokenInfo(new Token(TokenType.Delimiter, delimiter.ToString()), new ExpressionInfo() { Name = delimiter.ToString() } ));
        }

        private static TokenInfo MapEvent(EventInfo ei)
        {
            var expInfo = new ExpressionInfo { Name = ei.Name, Description = ExpressionInfo.WithDocumentation("Event", GetDocumentation(ei)), Kind = ExpressionKind.Event };

            return new TokenInfo(new Token(TokenType.Identifier, ei.Name), expInfo);
        }

        private static bool IsUserDefinedType(Type type)
        {
            return !type.IsPrimitive && type != typeof(string) && !type.IsArray;
        }

        private static Node<TokenInfo> MapProperty(PropertyInfo propertyInfo)
        {
            var node = new Node<TokenInfo>(new TokenInfo(new Token(TokenType.Identifier, propertyInfo.Name),
                new ExpressionInfo
                    {
                        Name = propertyInfo.Name,
                        Kind = ExpressionKind.Property,
                        Description = ExpressionInfo.WithDocumentation(string.Format("{0} {1}", propertyInfo.PropertyType.Name, propertyInfo.Name), GetDocumentation(propertyInfo))
                    }));

            if(IsUserDefinedType(propertyInfo.PropertyType))
            {
                var dotDelim = ConstructDelimiterNode('.');
                node.AddChild(dotDelim);

                var members = GetFilteredMembers(propertyInfo.PropertyType);

                AddEvents(dotDelim, members);
                AddMethods(dotDelim, members, false);
                AddProperties(dotDelim, members);
            }

            return node;
        }

        private static List<MemberInfo> GetFilteredMembers(Type type)
        {
            
            return type.GetMembers().Where(IsViableForCodeCompletion).ToList();            
        }

        private static bool IsViableForCodeCompletion(MemberInfo member)
        {
            return member.DeclaringType != typeof(object);
        }

        // The members of the returned type are only added one level down, a method can return its own type
        private static Node<TokenInfo> MapMethod(MethodInfo mi, bool withReturnedMembers)
        {
            var expInfo = new ExpressionInfo {Name = mi.Name, Kind = ExpressionKind.Method};

            var parameters = GetParametersWithoutIndexer(mi).Select(pi => string.Format("{0} {1}", pi.ParameterType.Name, pi.Name)).ToList();
            var returnType = mi.ReturnType.Name == "Void" ? "void" : mi.ReturnType.Name;
            var documentation = GetDocumentation(mi);
            expInfo.Description = ExpressionInfo.WithDocumentation(string.Format("{0} {1} ({2})", returnType, mi.Name, string.Join(",", parameters)), documentation);
            expInfo.Signature = new SignatureInfo(mi.Name, parameters, returnType, documentation);

            var node = new Node<TokenInfo>(new TokenInfo(new Token(TokenType.Identifier, mi.Name), expInfo));

            if (withReturnedMembers && mi.ReturnType != typeof(void) && !mi.ReturnType.IsEnum && IsUserDefinedType(mi.ReturnType))
            {
                var dotDelim = node.AddChild(ConstructDelimiterNode('.'));
                var members = GetFilteredMembers(mi.ReturnType);

                AddEvents(dotDelim, members);
                AddMethods(dotDelim, members, false);
                AddProperties(dotDelim, members);
            }

            return node;
        }

        private static IEnumerable<ParameterInfo> GetParametersWithoutIndexer(MethodInfo mi)
        {
            var parameters = mi.GetParameters();
            return mi.GetCustomAttributes(typeof(NeedIndexer), false).Length > 0 ? parameters.Take(parameters.Length - 1) : parameters;
        }

        private static ExpressionInfo MapEnumInfo(Type type, string name)
        {
            return new ExpressionInfo
                       {
                           Name = name,
                           Kind = ExpressionKind.EnumMember,
                           Description = ExpressionInfo.WithDocumentation(string.Format("{0} = {1}", name, Convert.ToInt32(Enum.Parse(type, name))), GetDocumentation(type.GetField(name)))
                       };
        }

        private static Node<TokenInfo> MapEnumMember(Type type, string name)
        {
            return new Node<TokenInfo>(new TokenInfo(new Token(TokenType.Identifier, name), MapEnumInfo(type, name)));
        }

        private static Node<TokenInfo> MapEnum(Type type)
        {
            var expInfo = new ExpressionInfo {Name = type.Name, Description = ExpressionInfo.WithDocumentation("Enum", GetDocumentation(type)), Kind = ExpressionKind.Enum};

            var node = new Node<TokenInfo>(new TokenInfo(new Token(TokenType.Identifier, type.Name), expInfo));

            var delimiter = node.AddChild(ConstructDelimiterNode('.'));
            
            delimiter.AddChildren(Enum.GetNames(type).Select(name => MapEnumMember(type, name)));

            return node;
        }
    }
}