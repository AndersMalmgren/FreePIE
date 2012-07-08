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

        public override bool IsCompleteMatch(string str)
        {
            var regex = new Regex(string.Format(Pattern, Name));
            return regex.IsMatch(str);
        }

        public override bool IsPartialMatch(string token)
        {
            if (token.Length < Name.Length)
                return Name.StartsWith(token);

            if (!token.StartsWith(Name))
                return false;

            if (token.Length > Name.Length)
                return token[Name.Length] == '[';

            return true;
        }

        public override string GetCompletion()
        {
            return Name + "[";
        }
    }
}
