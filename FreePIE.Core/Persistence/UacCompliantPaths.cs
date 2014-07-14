using System;
using System.IO;
using FreePIE.Core.Common;

namespace FreePIE.Core.Persistence
{
    public class UacCompliantPaths : IPaths
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

        private void EnureWorkingDirectory()
        {
            Environment.CurrentDirectory = Application;
        }

        public string GetDataPath(string filename)
        {
            return Path.Combine(Data, filename);
        }

        public string GetApplicationPath(string filename)
        {
            return Path.Combine(Application, filename);
        }

        public string Data { get; private set; }
        public string Application { get; private set; }
    }
}
