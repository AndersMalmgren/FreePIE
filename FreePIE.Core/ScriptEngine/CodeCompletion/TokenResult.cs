using System.Collections.Generic;
using FreePIE.Core.Common;

namespace FreePIE.Core.ScriptEngine.CodeCompletion
{
    public class TokenResult
    {
        public TokenResult(IEnumerable<Token> tokens, Range lastToken)
        {
            this.Tokens = tokens;
            this.LastToken = lastToken;
        }

        public IEnumerable<Token> Tokens { get; private set; }
        public Range LastToken { get; private set; }
    }
}
