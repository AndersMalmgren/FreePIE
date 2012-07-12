using System;

namespace FreePIE.Core.ScriptEngine.CodeCompletion
{
    public class Token
    {
        public static Token Empty() { return new Token(TokenType.Identifier, string.Empty); }

        public Token(TokenType type, string value)
        {
            Type = type;
            Value = value;
        }

        public string Value { get; set; }

        public TokenType Type { get; set; }

        public virtual bool IsCompleteMatch(Token token)
        {
            return this == token;
        }

        public virtual bool IsPartialMatch(Token token)
        {
            return Type == token.Type && Value.StartsWith(token.Value, StringComparison.InvariantCultureIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (Token)) return false;
            return Equals((Token) obj);
        }

        public bool Equals(Token other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Value, Value) && Equals(other.Type, Type);
        }

        public static bool operator ==(Token one, Token two)
        {
            return one.Equals(two);
        }

        public static bool operator !=(Token one, Token two)
        {
            return !(one == two);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Value != null ? Value.GetHashCode() : 0)*397) ^ Type.GetHashCode();
            }
        }
    }
}