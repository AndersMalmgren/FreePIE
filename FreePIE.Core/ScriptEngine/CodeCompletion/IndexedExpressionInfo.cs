using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FreePIE.Core.ScriptEngine.CodeCompletion
{
    public class IndexedExpressionInfo : ExpressionInfo
    {
        private const string Pattern = @"{0}\[.*\]";

        public override bool IsCompleteMatch(Token token)
        {
            if (!IsContextMatch(token.Context))
                return false;

            var regex = new Regex(string.Format(Pattern, Name));
            return regex.IsMatch(token.Value);
        }

        public override bool IsPartialMatch(Token token)
        {
            if (!IsContextMatch(token.Context))
                return false;

            if (token.Value.Length < Name.Length)
                return Name.StartsWith(token.Value);

            if (!token.Value.StartsWith(Name))
                return false;

            if (token.Value.Length > Name.Length)
                return token.Value[Name.Length] == '[';

            return true;
        }

        public override string GetCompletion()
        {
            return Name + "[";
        }
    }
}
