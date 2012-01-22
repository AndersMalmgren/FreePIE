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
        private IChoices choices;
        public IChoices Choices
        {
            get 
            {
                if (choices == null)
                    choices = new Choices();

                return choices; 
            }
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