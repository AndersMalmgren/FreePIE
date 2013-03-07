using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.Core.Plugins.Extensions
{
    public static class MathExtensions
    {
        public static double Rad(this double d)
        {
            return Math.PI*d/180;
        }
    }
}
