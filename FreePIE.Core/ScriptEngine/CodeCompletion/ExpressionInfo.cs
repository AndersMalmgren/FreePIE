using System.Text;

namespace FreePIE.Core.ScriptEngine.CodeCompletion
{
    public class ExpressionInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public ExpressionInfo()
        {
            Name = string.Empty;
            Description = string.Empty;
        }

        public override string ToString()
        {
            return string.Format("{0} --:-- {1}", Name, Description ?? string.Empty);
        }

        public virtual string GetFormattedDescription()
        {
            var b = new StringBuilder(Name.Length + Description.Length + 5);

            b.AppendLine(Name);
            b.AppendLine();
            b.Append(Description);

            return b.ToString();
        }

        public virtual string GetFormattedName()
        {
            return Name;
        }

        public virtual string GetCompletion(string token)
        {
            return Name;
        }
    }
}
