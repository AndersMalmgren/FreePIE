using System;
using System.IO;

namespace FreePIE.Core.Persistence
{
    public class UacCompliantPaths : IPaths
    {
        private const string appFolder = "%appdata%\\FreePIE";

        public UacCompliantPaths()
        {
            var absoluteDataPath = Environment.ExpandEnvironmentVariables(appFolder);

            if (!Directory.Exists(absoluteDataPath))
                Directory.CreateDirectory(absoluteDataPath);

            Data = absoluteDataPath;
            Application = Environment.CurrentDirectory;
        }

        public string GetDataPath(string filename)
        {
            return Path.Combine(Data, filename);
        }

        public string GetApplicationPath(string filename)
        {
            return Path.Combine(Application, filename);
        }

        public void addLoadPaths(string[] args)
        {
            LoadPaths = args;
        }

        public string Data { get; private set; }
        public string Application { get; private set; }
        public string[] LoadPaths { get; private set; }
    }
}
