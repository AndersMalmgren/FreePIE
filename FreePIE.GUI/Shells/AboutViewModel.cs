using System;
using System.Collections.Generic;
using System.Reflection;
using FreePIE.GUI.Result;
using System.Linq;

namespace FreePIE.GUI.Shells
{
    public class AboutViewModel : ShellPresentationModel
    {
        private static readonly string version;

        static AboutViewModel()
        {
            version = Assembly
                .GetExecutingAssembly()
                .GetName()
                .Version
                .ToString();
        }

        public AboutViewModel(IResultFactory resultFactory) : base(resultFactory)
        {
            DisplayName = "About FreePIE";
        }

        public string Version
        {
            get { return version; }
        }

        public IEnumerable<string> Assemblies
        {
            get
            {
                return AppDomain
                    .CurrentDomain
                    .GetAssemblies()
                    .Where(a => !a.GlobalAssemblyCache && !a.IsDynamic)
                    .GroupBy(a => a.FullName)
                    .Select(a => a.First())
                    .OrderBy(a => a.FullName)
                    .Select(a => string.Format("{0} {1}", a.GetName().Name, a.GetName().Version))
                    .ToList();
            }
        }

        public string ProjectPageUrl
        {
            get { return "http://andersmalmgren.github.io/FreePIE/"; }
        }

        public void GotoProjectPage()
        {
            System.Diagnostics.Process.Start(ProjectPageUrl);
        }
    }
}
