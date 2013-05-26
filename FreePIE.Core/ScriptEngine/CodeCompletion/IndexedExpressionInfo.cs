namespace FreePIE.Core.ScriptEngine.CodeCompletion
{
    public class IndexedExpressionInfo : ExpressionInfo
    {
        public override string GetCompletion(string token)
        {
            var completion = Name + "[";
            return token.Length > completion.Length ? token : completion;
        }

        public override string GetFormattedName()
        {
            return Name + "[";
        }
    }
}
