namespace FreePIE.Core.ScriptEngine.CodeCompletion
{
    public class Token
    {
        public Token(TokenContext context, string value)
        {
            Context = context;
            Value = value;
        }

        public string Value { get; private set; }
        public TokenContext Context { get; private set; }

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
            return Equals(other.Value, Value) && Equals(other.Context, Context);
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
                return ((Value != null ? Value.GetHashCode() : 0)*397) ^ Context.GetHashCode();
            }
        }
    }
}