using System;
using System.Collections.Generic;
using System.Linq;
using FreePIE.Core.Model;
using FreePIE.Core.Plugins;
using FreePIE.Core.ScriptEngine.Globals;

namespace FreePIE.Core.ScriptEngine.CodeCompletion
{
    public class RuntimeInfoProvider : IRuntimeInfoProvider
    {
        private readonly IEnumerable<IGlobalProvider> providers;
        private readonly IPluginInvoker invoker;
        private Node<ExpressionInfo> runtimeInfo;

        private Node<ExpressionInfo> RuntimeInfo 
        { 
            get
            {
                return runtimeInfo ?? (runtimeInfo = InfoTransformHelper.ConstructExpressionInfoTree(invoker, providers));
            }
        }

        public RuntimeInfoProvider(IPluginInvoker invoker, IEnumerable<IGlobalProvider> providers)
        {
            this.providers = providers;
            this.invoker = invoker;
        }

        public IEnumerable<Node<ExpressionInfo>> AnalyzeExpression(IEnumerable<string> tokensEnum)
        {
            var tokens = tokensEnum.ToList();
            
            string incompleteToken = tokens.Last();
            var sequence = tokens.Take(tokens.Count() - 1);

            Node<ExpressionInfo> parent = RuntimeInfo.FindSequence(sequence, (exp, str) => exp.IsCompleteMatch(str));

            return parent == null ? Enumerable.Empty<Node<ExpressionInfo>>() : parent.Children.Where(child => child.Value.IsPartialMatch(incompleteToken));
        }

        
    }
}