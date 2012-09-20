using System.Collections.Generic;
using System.IO;

namespace FreePIE.Core.Common
{
    public interface IFileSystem
    {
        IEnumerable<string> GetFiles(string path, string pattern);
        void WriteAllText(string path, string content);
        string ReadAllText(string path);
        bool Exists(string path);
        Stream OpenRead(string path);
    }
}