using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.Model
{
    public class Choice : IChoice
    {
        public string Caption { get; set; }
        public object Value { get; set; }
    }
}
