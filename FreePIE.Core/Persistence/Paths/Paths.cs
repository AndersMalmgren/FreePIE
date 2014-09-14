using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FreePIE.Core.Persistence.Paths
{
    public abstract class Paths : IPaths
    {
        protected void EnureWorkingDirectory()
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

        public string Data { get; protected set; }
        public string Application { get; protected set; }
    }
}
