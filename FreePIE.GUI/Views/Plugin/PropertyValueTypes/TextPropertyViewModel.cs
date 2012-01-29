using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using FreePIE.Core.Model;

namespace FreePIE.GUI.Views.Plugin.PropertyValueTypes
{
    public class TextPropertyViewModel : ValueViewModel
    {
        public TextPropertyViewModel(PluginProperty pluginProperty)
            : base(pluginProperty)
        {
            Text = pluginProperty.Value.ToString();
        }

        private string text;
        public string Text
        {
            get { return text; }
            set 
            { 
                pluginProperty.SetValue(value);
                text = value; 
                NotifyOfPropertyChange(() => Text);
            }
        }
    }
}
