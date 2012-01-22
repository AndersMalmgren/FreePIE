using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.Model
{
    public class Choices : List<IChoice>, IChoices
    {
        public void Add(string caption, object value)
        {
            var choice = new Choice();
            choice.Caption = caption;
            choice.Value = value;
            Add(choice);
        }
    }
}
