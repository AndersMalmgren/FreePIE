using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using FreePIE.GUI.Result;

namespace FreePIE.GUI.Shells
{
    public class AboutViewModel : ShellPresentationModel
    {
        private static readonly string version;

        static AboutViewModel()
        {
            version = String.Format("{0} Alpha",Assembly
                    .GetExecutingAssembly()
                    .GetName()
                    .Version
                    .ToString());
        }

        public AboutViewModel(IResultFactory resultFactory) : base(resultFactory)
        {
            DisplayName = "About FreePIE";
        }

        public string Version
        {
            get { return version; }
        }

        public string ProjectPageUrl
        {
            get { return "http://andersmalmgren.github.com/FreePIE/"; }
        }

        public void GotoProjectPage()
        {
            System.Diagnostics.Process.Start(ProjectPageUrl);
        }
    }
}
