using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.Core.ScriptEngine.CodeCompletion
{
    public class ExpressionInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public TokenContext Context { get; set; }

        public ExpressionInfo()
        {
            this.Context = TokenContext.All;
        }

        public override string ToString()
        {
            return string.Format("{0} --:-- {1}", Name, Description ?? string.Empty);
        }

        protected bool IsContextMatch(TokenContext other)
        {
            return Context == TokenContext.All || other == Context;
        }

        public virtual string GetFormattedDescription()
        {
            StringBuilder b = new StringBuilder(Name + Description + 5);

            b.AppendLine(Name);
            b.AppendLine();
            b.Append(Description);

            return b.ToString();
        }

        public virtual bool IsCompleteMatch(Token token)
        {
            return IsContextMatch(token.Context) && Name == token.Value;
        }

        public virtual bool IsPartialMatch(Token token)
        {
            return IsContextMatch(token.Context) && Name.StartsWith(token.Value);
        }

        public virtual string GetCompletion()
        {
            return Name;
        }
    }
}
