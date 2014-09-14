using System;
using System.IO;
using FreePIE.Core.Common;

namespace FreePIE.Core.Persistence.Paths
{
    public class UacCompliantPaths : Paths
    {
        private const string appFolder = "%appdata%\\FreePIE";

        public UacCompliantPaths(IFileSystem fileSystem)
        {
            var absoluteDataPath = Environment.ExpandEnvironmentVariables(appFolder);

            fileSystem.CreateDirectory(absoluteDataPath);

            Data = absoluteDataPath;
            Application = AppDomain.CurrentDomain.BaseDirectory;
            EnureWorkingDirectory();
        }
    }
}
