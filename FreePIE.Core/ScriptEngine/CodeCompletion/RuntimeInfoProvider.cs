using System;
using System.Collections.Generic;
using System.Linq;
using FreePIE.Core.Common.Events;
using FreePIE.Core.Model;
using FreePIE.Core.Model.Events;
using FreePIE.Core.Plugins;
using FreePIE.Core.ScriptEngine.Globals;

namespace FreePIE.Core.ScriptEngine.CodeCompletion
{
    public class RuntimeInfoProvider : IRuntimeInfoProvider, IHandle<CurveChangedNameEvent>
    {
        private readonly IEnumerable<IGlobalProvider> providers;
        private readonly IPluginInvoker invoker;
        private Node<TokenInfo> runtimeInfo;

        private Node<TokenInfo> RuntimeInfo 
        { 
            get
            {
                return runtimeInfo ?? (runtimeInfo = InfoTransformHelper.ConstructExpressionInfoTree(invoker, providers));
            }
        }

        public RuntimeInfoProvider(IPluginInvoker invoker, IEnumerable<IGlobalProvider> providers, IEventAggregator eventAggregator)
        {
            this.providers = providers;
            this.invoker = invoker;
            eventAggregator.Subscribe(this);
        }

        public IEnumerable<ExpressionInfo> AnalyzeExpression(IEnumerable<Token> tokensEnum)
        {
            var tokens = tokensEnum.ToList();
            
            Token incompleteToken = tokens.Last();
            var sequence = tokens.Take(tokens.Count() - 1);

            Node<TokenInfo> parent = RuntimeInfo.FindSequence(sequence, (tokenInfo, token) => tokenInfo.Identifier.IsCompleteMatch(token));

            return parent == null ? Enumerable.Empty<ExpressionInfo>() : parent.Children.Where(child =>
                                                                                    child.Value.Identifier.IsPartialMatch(incompleteToken))
                                                                                    .Select(child => child.Value.Info);
        }


        public void Handle(CurveChangedNameEvent message)
        {
            runtimeInfo = null;
        }
    }
}