using System;
using System.Linq;
using Caliburn.Micro;
using FreePIE.Core.Common;
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
            return GetPropertyValueViewModel(false) ?? GetPropertyValueViewModel(true);
        }

        private ValueViewModel GetPropertyValueViewModel(bool checkDefault)
        {
            return Utils.GetTypes<ValueViewModel>()
                .Where(type => (bool)type.GetMethod("CanEdit").Invoke(null, new object[] { pluginProperty, checkDefault }))
                .Select(type => Activator.CreateInstance(type, new object[] { pluginProperty }))
                .FirstOrDefault() as ValueViewModel;
        }

        public string Name { get { return pluginProperty.Caption; } }
        public string HelpText { get { return pluginProperty.HelpText; } }
        public ValueViewModel Value  { get; set; }
    }
}
