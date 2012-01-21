using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FreePIE.Core.Common
{
    public class FileSystem : IFileSystem
    {
        public IEnumerable<string> GetFiles(string path, string pattern)
        {
            return Directory.GetFiles(path, pattern);
        }
    }
}
