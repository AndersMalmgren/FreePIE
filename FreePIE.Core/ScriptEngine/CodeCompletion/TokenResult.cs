using System.Collections.Generic;
using FreePIE.Core.Common;

namespace FreePIE.Core.ScriptEngine.CodeCompletion
{
    public class TokenResult
    {
        public TokenResult(IEnumerable<Token> tokens, Range lastToken)
        {
            this.Tokens = tokens;
            this.LastTokenRange = lastToken;
        }

        public IEnumerable<Token> Tokens { get; private set; }
        public Range LastTokenRange { get; private set; }
    }
}
