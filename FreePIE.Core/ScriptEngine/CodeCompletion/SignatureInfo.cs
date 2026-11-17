using System.Collections.Generic;
using System.Linq;

namespace FreePIE.Core.ScriptEngine.CodeCompletion
{
    public class SignatureInfo
    {
        public SignatureInfo(string name, IEnumerable<string> parameters, string prefix = "", string documentation = "")
        {
            Name = name;
            Parameters = parameters.ToList();
            Prefix = prefix ?? string.Empty;
            Documentation = documentation ?? string.Empty;
        }

        public string Name { get; private set; }
        public IList<string> Parameters { get; private set; }

        // Return type for .NET methods, def for Python functions
        public string Prefix { get; private set; }
        public string Documentation { get; private set; }

        public override string ToString()
        {
            return string.Format("{0}{1}({2})", Prefix.Length > 0 ? Prefix + " " : string.Empty, Name, string.Join(", ", Parameters));
        }
    }

    public class SignatureHelpResult
    {
        public SignatureHelpResult(IEnumerable<SignatureInfo> signatures, int parameterIndex, int callStart)
        {
            CallStart = callStart;
            Signatures = signatures.ToList();
            ParameterIndex = parameterIndex;
        }

        public IList<SignatureInfo> Signatures { get; private set; }
        public int ParameterIndex { get; private set; }

        // Index of the opening parenthesis of the call
        public int CallStart { get; private set; }

        // The first overload that has enough parameters for the argument being written
        public SignatureInfo BestMatch
        {
            get { return Signatures.FirstOrDefault(s => s.Parameters.Count > ParameterIndex) ?? Signatures.First(); }
        }
    }
}
