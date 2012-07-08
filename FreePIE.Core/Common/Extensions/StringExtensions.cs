using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreePIE.Core.Common;

namespace FreePIE.GUI.Common
{
    public static class StringExtensions
    {
        public static string Replace(this string str, Range range, string replacement)
        {
            var builder = new StringBuilder(str.Length - range.NumberOfElements + replacement.Length);

            builder.Append(str.Substring(0, range.Start));
            builder.Append(replacement);
            builder.Append(str.Substring(range.Start + range.NumberOfElements));

            return builder.ToString();
        }
    }
}
