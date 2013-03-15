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

        public static bool CanEdit(PluginProperty pluginProperty, bool defaultCheck)
        {
            return defaultCheck;
        }
    }
}
