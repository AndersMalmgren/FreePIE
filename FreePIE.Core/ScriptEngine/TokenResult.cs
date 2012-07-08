using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreePIE.Core.Common;

namespace FreePIE.Core.ScriptEngine
{
    public class TokenResult
    {
        public TokenResult(IEnumerable<string> tokens, Range lastToken)
        {
            this.Tokens = tokens;
            this.LastToken = lastToken;
        }

        public IEnumerable<string> Tokens { get; private set; }
        public Range LastToken { get; private set; }
    }
}
