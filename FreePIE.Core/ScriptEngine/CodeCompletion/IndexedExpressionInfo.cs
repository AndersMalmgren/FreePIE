using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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
