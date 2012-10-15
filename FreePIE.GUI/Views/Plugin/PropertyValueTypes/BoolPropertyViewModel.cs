using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreePIE.Core.Model;

namespace FreePIE.GUI.Views.Plugin.PropertyValueTypes
{
    public class BoolPropertyViewModel : ValueViewModel
    {
        public BoolPropertyViewModel(PluginProperty pluginProperty) : base(pluginProperty)
        {
        }

        public static bool CanEdit(PluginProperty pluginProperty, bool defaultCheck)
        {
            return !defaultCheck && pluginProperty.DefaultValue is bool;
        }
    }
}
