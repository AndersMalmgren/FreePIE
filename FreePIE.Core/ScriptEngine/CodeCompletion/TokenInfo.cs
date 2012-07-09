using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.Core.ScriptEngine.CodeCompletion
{
    public class TokenInfo
    {
        public TokenInfo(Token identifier, ExpressionInfo info)
        {
            Identifier = identifier;
            Info = info;
        }

        public Token Identifier { get; private set; }

        public ExpressionInfo Info { get; private set; }
    }
}
