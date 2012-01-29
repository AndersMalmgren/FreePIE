using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.Model
{
    [KnownType(typeof(Choices))]
    [KnownType(typeof(Choice))]
    [DataContract]
    public class PluginProperty : IPluginProperty
    {
        public object DefaultValue { get; set; }
        private Choices choices;
        public IChoices Choices
        {
            get
            {
                var dummy = ConcreteChoices;
                return choices; 
            }
        }

        public IEnumerable<Choice> ConcreteChoices
        {
            get
            {
                if (choices == null)
                    choices = new Choices();

                return choices;
            }
        }

        public Choice SelectedChoice
        {
            get { return choices.SingleOrDefault(c => c.Value.Equals(Value)); }
            set { Value = value.Value; }
        }

        public void SetValue(string text)
        {
            var valueType = Value.GetType();
            Value = Convert.ChangeType(text, valueType);
        }

        [DataMember]
        public string Name { get; set; }
        public string Caption { get; set; }
        [DataMember]
        public object Value { get; set; }
        public string HelpText { get; set; }

        public PluginProperty()
        {
            
        }
    }
}