using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using FreePIE.Core.Model;
using FreePIE.GUI.Views.Plugin.PropertyValueTypes;

namespace FreePIE.GUI.Views.Plugin
{
    public class PluginPropertyViewModel : PropertyChangedBase
    {
        private readonly PluginProperty pluginProperty;

        public PluginPropertyViewModel(PluginProperty pluginProperty)
        {
            this.pluginProperty = pluginProperty;
            Value = GetPropertyValueViewModel();
        }

        private ValueViewModel GetPropertyValueViewModel()
        {
            if(pluginProperty.ConcreteChoices.Any())
            {
                return new ChoicesPropertyViewModel(pluginProperty);
            }

            return new TextPropertyViewModel(pluginProperty);
        }

        public string Name { get { return pluginProperty.Caption; } }
        public string HelpText { get { return pluginProperty.HelpText; } }
        public ValueViewModel Value  { get; set; }
    }
}
