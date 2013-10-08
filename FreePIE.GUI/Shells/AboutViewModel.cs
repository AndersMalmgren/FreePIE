using System.Collections.Generic;
using System.Reflection;
using FreePIE.GUI.Result;
using System.Linq;

namespace FreePIE.GUI.Shells
{
    public class AboutViewModel : ShellPresentationModel
    {
        private static readonly string version;
        private static readonly IEnumerable<string> assemblies;

        static AboutViewModel()
        {
            version = Assembly
                .GetExecutingAssembly()
                .GetName()
                .Version
                .ToString();

            assemblies = Assembly
                .GetExecutingAssembly()
                .GetReferencedAssemblies()
                .OrderBy(a => a.Name)
                .Select(a => string.Format("{0} {1}", a.Name, a.Version))
                .ToList();
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
            get { return assemblies; }
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
