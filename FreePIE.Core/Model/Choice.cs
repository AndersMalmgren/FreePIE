using FreePIE.Core.Contracts;

namespace FreePIE.Core.Model
{
    public class Choice : IChoice
    {
        public string Caption { get; set; }
        public object Value { get; set; }
    }
}
