using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreePIE.Core.Persistence.Paths;

namespace FreePIE.Core.Common
{
    internal class Log : ILog
    {
        private readonly IFileSystem fileSystem;
        private readonly string path;

        public Log(IFileSystem fileSystem, IPaths paths)
        {
            this.fileSystem = fileSystem;
            path = paths.GetDataPath("FreePIE.log");
        }

        private string PrependTabsToLinebreaks(string input, int numberOfTabs)
        {
            var tabs = new string('\t', numberOfTabs);

            return tabs + input.Replace(Environment.NewLine, Environment.NewLine + tabs);
        }

        public void Error(Exception e)
        {
            Error(e, 0);
        }

        private void Error(Exception e, int indentation)
        {
            if (e == null)
                return;

            var delimiter = indentation == 0 ? DateTime.Now.ToString() + " - " : string.Empty;

            var log = string.Format("{0}{1}{2} - {3}: {5}{4}{5}", new string('\t', indentation), delimiter, e.GetType().FullName, e.Message, PrependTabsToLinebreaks(e.StackTrace, indentation), Environment.NewLine);
            fileSystem.AppendAllText(path, log);

            Error(e.InnerException, indentation + 1);

            if (indentation == 0)
                fileSystem.AppendAllText(path, Environment.NewLine);
        }
    }
}
