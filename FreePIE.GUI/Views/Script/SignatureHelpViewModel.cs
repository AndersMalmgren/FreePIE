using System;
using System.Linq;
using Caliburn.Micro;
using FreePIE.Core.ScriptEngine.CodeCompletion;

namespace FreePIE.GUI.Views.Script
{
    // Shows the signature of the call the caret is in, with the parameter being written highlighted
    public class SignatureHelpViewModel : PropertyChangedBase
    {
        private const int MaxDocumentationLines = 6;

        private SignatureHelpResult result;
        private int dismissedCallStart = -1;

        public void Update(SignatureHelpResult newResult)
        {
            if (newResult == null || newResult.CallStart != dismissedCallStart)
                dismissedCallStart = -1;

            result = newResult;

            NotifyOfPropertyChange(() => IsVisible);
            NotifyOfPropertyChange(() => Before);
            NotifyOfPropertyChange(() => Current);
            NotifyOfPropertyChange(() => After);
            NotifyOfPropertyChange(() => Overloads);
            NotifyOfPropertyChange(() => Documentation);
            NotifyOfPropertyChange(() => HasDocumentation);
        }

        // Hides the help until the caret leaves the current call
        public void Dismiss()
        {
            if (result == null)
                return;

            dismissedCallStart = result.CallStart;
            NotifyOfPropertyChange(() => IsVisible);
        }

        public bool IsVisible
        {
            get { return result != null && result.CallStart != dismissedCallStart; }
        }

        private SignatureInfo Signature
        {
            get { return result != null ? result.BestMatch : null; }
        }

        private int ParameterIndex
        {
            get { return result != null ? result.ParameterIndex : 0; }
        }

        public string Before
        {
            get
            {
                if (Signature == null)
                    return string.Empty;

                var prefix = Signature.Prefix.Length > 0 ? Signature.Prefix + " " : string.Empty;
                var parameters = Signature.Parameters.Take(ParameterIndex).ToList();

                return string.Format("{0}{1}({2}{3}", prefix, Signature.Name, string.Join(", ", parameters), parameters.Any() && ParameterIndex < Signature.Parameters.Count ? ", " : string.Empty);
            }
        }

        public string Current
        {
            get { return Signature != null && ParameterIndex < Signature.Parameters.Count ? Signature.Parameters[ParameterIndex] : string.Empty; }
        }

        public string After
        {
            get
            {
                if (Signature == null)
                    return string.Empty;

                var parameters = Signature.Parameters.Skip(ParameterIndex + 1).ToList();
                return string.Format("{0}{1})", parameters.Any() ? ", " : string.Empty, string.Join(", ", parameters));
            }
        }

        public string Overloads
        {
            get { return result != null && result.Signatures.Count > 1 ? string.Format("  (+{0} overloads)", result.Signatures.Count - 1) : string.Empty; }
        }

        public string Documentation
        {
            get
            {
                if (Signature == null)
                    return string.Empty;

                var lines = Signature.Documentation.Trim().Split('\n').Select(l => l.TrimEnd()).Take(MaxDocumentationLines);
                return string.Join(Environment.NewLine, lines);
            }
        }

        public bool HasDocumentation
        {
            get { return Documentation.Length > 0; }
        }
    }
}
