using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using FreePIE.Core.Model;
using FreePIE.GUI.Result;
using FreePIE.GUI.Views.Plugin;

namespace FreePIE.GUI.Shells
{
    public class PluginSettingsViewModel : ShellPresentationModel
    {
        public PluginSettingsViewModel(IResultFactory resultFactory) : base(resultFactory)
        {
        }

        public void Init(PluginSetting pluginSetting)
        {
            DisplayName = string.Format("{0} - Plugin settings", pluginSetting.FriendlyName);
            PluginProperties = pluginSetting.PluginProperties.Select(p => new PluginPropertyViewModel(p));
        }

        public IEnumerable<IResult> Ok()
        {
            yield return Result.Close();
        }

        public IEnumerable<PluginPropertyViewModel> PluginProperties { get; private set; }
    }
}
