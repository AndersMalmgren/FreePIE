using System.Collections.Generic;
using System.IO;

namespace FreePIE.Core.Common
{
    public class FileSystem : IFileSystem
    {
        public IEnumerable<string> GetFiles(string path, string pattern)
        {
            return Directory.GetFiles(path, pattern);
        }

        public void WriteAllText(string path, string content)
        {
            File.WriteAllText(path, content);
        }

        public string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }

        public bool Exists(string path)
        {
            return File.Exists(path);
        }

        public Stream OpenRead(string path)
        {
            return File.OpenRead(path);
        }

        public void AppendAllText(string path, string content)
        {
            File.AppendAllText(path, content);
        }

        public string GetFilename(string path)
        {
            return Path.GetFileName(path);
        }

        public void Delete(string path)
        {
            File.Delete(path);
        }
    }
}
