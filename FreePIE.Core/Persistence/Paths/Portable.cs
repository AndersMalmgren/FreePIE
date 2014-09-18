using System;
using System.IO;
using Microsoft.Win32;

namespace FreePIE.Core.Persistence.Paths
{
    public class Portable : IPortable
    {
        private const string Key =  @"Software\FreePIE";

        static Portable()
        {
            var path = NormalizePath(Registry.GetValue(Path.Combine(Registry.CurrentUser.ToString(), Key), "path", null) as string);
            var actualPath = NormalizePath(AppDomain.CurrentDomain.BaseDirectory);

            isPortable = path != actualPath;
        }

        private static string NormalizePath(string path)
        {
            if (path == null) return string.Empty;

            return Path.GetFullPath(new Uri(path).LocalPath)
                           .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                           .ToLowerInvariant();
        }


        private static bool isPortable;

        public bool IsPortable { get { return isPortable; } }
    }
}
