using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.ScriptEngine.Globals.ScriptHelpers
{
    [LuaGlobal(Name = "math")]
    public class MathHelper : IScriptHelper
    {
        public double toRad(double degree)
        {
            return degree / (float)(180.0f / Math.PI);
        }

        public double toDegree(double rad)
        {
            return rad * (float)(180.0f / Math.PI);
        }
    }
}
