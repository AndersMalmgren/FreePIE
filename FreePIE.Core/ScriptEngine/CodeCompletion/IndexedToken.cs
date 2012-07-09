using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FreePIE.Core.ScriptEngine.CodeCompletion
{
    public class IndexedToken : Token
    {
        private const string Pattern = @"{0}\[.*\]";


        public IndexedToken(string name) : base(TokenType.Identifier, name)
        { }

        public override bool IsCompleteMatch(Token token)
        {
            var regex = new Regex(string.Format(Pattern, Value));
            return regex.IsMatch(token.Value);
        }

        public override bool IsPartialMatch(Token token)
        {
            if (token.Value.Length < Value.Length)
                return Value.StartsWith(token.Value);

            if (!token.Value.StartsWith(Value))
                return false;

            if (token.Value.Length > Value.Length)
                return token.Value[Value.Length] == '[';

            return true;
        }
    }
}
