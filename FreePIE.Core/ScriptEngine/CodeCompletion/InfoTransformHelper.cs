﻿using System;
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

        private static bool IsDeprecated(MemberInfo info)
        {
            return info.GetCustomAttributes(typeof (Deprecated), false).Length > 0;
        }

        private static void AddClassMembers(Node<TokenInfo> node, Type type)
        {
            var globalMembers = GlobalsInfo.GetGlobalMembers(type).ToList();

            var dotDelim = node.AddChild(ConstructDelimiterNode('.'));

            AddEvents(dotDelim, globalMembers);
            AddProperties(dotDelim, globalMembers);

            AddMethods(dotDelim, globalMembers);
        }

        private static void AddProperties(Node<TokenInfo> propDelim, List<MemberInfo> members)
        {
            propDelim.AddChildren(members.Where(m => m.MemberType == MemberTypes.Property && !IsDeprecated(m)).Select(m => MapProperty(m as PropertyInfo)));
        }

        private static void AddEvents(Node<TokenInfo> eventDelim, IEnumerable<MemberInfo> members)
        {
            eventDelim.AddChildren(members.Where(m => m.MemberType == MemberTypes.Event).Select(m => MapEvent(m as EventInfo)));
        }

        private static void AddMethods(Node<TokenInfo> methodDelim, IEnumerable<MemberInfo> members)
        {
            methodDelim.AddChildren(members.Where(m => m.MemberType == MemberTypes.Method && !(m as MethodInfo).IsSpecialName && !IsDeprecated(m)).Select(m => MapMethod(m as MethodInfo)));
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
            var expInfo = new ExpressionInfo { Name = ei.Name, Description = "Event" };

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
                        Description = string.Format("{0} {1}", propertyInfo.PropertyType.Name, propertyInfo.Name)
                    }));

            if(IsUserDefinedType(propertyInfo.PropertyType))
            {
                var dotDelim = ConstructDelimiterNode('.');
                node.AddChild(dotDelim);

                var members = GetFilteredMembers(propertyInfo.PropertyType);

                AddEvents(dotDelim, members);
                AddMethods(dotDelim, members);
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

        private static TokenInfo MapMethod(MethodInfo mi)
        {
            var expInfo = new ExpressionInfo {Name = mi.Name};

            var parameters = string.Join(",", GetParametersWithoutIndexer(mi).Select(pi => string.Format("{0} {1}", pi.ParameterType.Name, pi.Name)));
            expInfo.Description = string.Format("{0} {1} ({2})", mi.ReturnType.Name == "Void" ? "void" : mi.ReturnType.Name, mi.Name, parameters);

            return new TokenInfo(new Token(TokenType.Identifier, mi.Name), expInfo);
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
                           Description = string.Format("{0} = {1}", name, Convert.ToInt32(Enum.Parse(type, name)))
                       };
        }

        private static Node<TokenInfo> MapEnumMember(Type type, string name)
        {
            return new Node<TokenInfo>(new TokenInfo(new Token(TokenType.Identifier, name), MapEnumInfo(type, name)));
        }

        private static Node<TokenInfo> MapEnum(Type type)
        {
            var expInfo = new ExpressionInfo {Name = type.Name, Description = "Enum"};

            var node = new Node<TokenInfo>(new TokenInfo(new Token(TokenType.Identifier, type.Name), expInfo));

            var delimiter = node.AddChild(ConstructDelimiterNode('.'));
            
            delimiter.AddChildren(Enum.GetNames(type).Select(name => MapEnumMember(type, name)));

            return node;
        }
    }
}