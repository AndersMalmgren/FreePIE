using System.Collections.Generic;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.Model
{
    public class Choices : List<Choice>, IChoices
    {
        public void Add(string caption, object value)
        {
            var choice = new Choice {Caption = caption, Value = value};
            Add(choice);
        }
    }
}
