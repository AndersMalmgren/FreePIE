using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreePIE.Core.Model;

namespace FreePIE.GUI.Views.Plugin.PropertyValueTypes
{
    public class ChoicesPropertyViewModel : ValueViewModel
    {
        public ChoicesPropertyViewModel(PluginProperty pluginProperty) : base(pluginProperty)
        {
            Choices = pluginProperty.ConcreteChoices;
            SelectedChoice = pluginProperty.SelectedChoice;
        }

        public IEnumerable<Choice> Choices { get; set; }
        private Choice selectedChoice;

        public Choice SelectedChoice
        {
            get { return selectedChoice; }
            set 
            {
                if (value == null)
                    return;

                selectedChoice = value;
                pluginProperty.SelectedChoice = value;
                NotifyOfPropertyChange(() => SelectedChoice);
            }
        }

        public static bool CanEdit(PluginProperty pluginProperty, bool defaultCheck)
        {
            return !defaultCheck && pluginProperty.ConcreteChoices.Any();
        }
    }
}
