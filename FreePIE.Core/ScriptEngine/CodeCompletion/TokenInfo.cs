namespace FreePIE.Core.ScriptEngine.CodeCompletion
{
    public class TokenInfo
    {
        public static readonly TokenInfo Empty = new TokenInfo(new Token(TokenType.Identifier, string.Empty), new ExpressionInfo());

        public TokenInfo(Token identifier, ExpressionInfo info)
        {
            Identifier = identifier;
            Info = info;
        }

        public Token Identifier { get; set; }

        public override string ToString()
        {
            return Identifier.Value;
        }

        public ExpressionInfo Info { get; set; }
    }
}
