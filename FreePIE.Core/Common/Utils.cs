using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FreePIE.Core.Common
{
    public static class Utils
    {
        public static string GetAbsolutePath(string path)
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + path;
        }
    }
}
