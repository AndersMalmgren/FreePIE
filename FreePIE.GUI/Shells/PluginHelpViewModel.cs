using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FreePIE.Core.Common;
using FreePIE.Core.Model;
using FreePIE.GUI.Result;

namespace FreePIE.GUI.Shells
{
    public class PluginHelpViewModel : ShellPresentationModel
    {
        private readonly IFileSystem fileSystem;

        public PluginHelpViewModel(IResultFactory resultFactory, IFileSystem fileSystem) : base(resultFactory)
        {
            this.fileSystem = fileSystem;
        }

        public void Init(PluginSetting pluginSetting)
        {
            HelpFile = fileSystem.OpenRead(pluginSetting.HelpFile);
            DisplayName = string.Format("{0} - Help", pluginSetting.FriendlyName);
        }

        public Stream HelpFile { get; private set; }
    }
}
