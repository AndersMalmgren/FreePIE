using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FreePIE.Core.ScriptEngine.CodeCompletion
{
    public class IndexedExpressionInfo : ExpressionInfo
    {
        public override string GetCompletion()
        {
            return Name + "[";
        }
    }
}
