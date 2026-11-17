using System;
using System.Linq;
using System.Threading.Tasks;
using FreePIE.Core.ScriptEngine.Python.Ast;

namespace FreePIE.Core.ScriptEngine.CodeCompletion
{
    // Parses the script on a background thread so that typing is never blocked by the parser.
    // Callers always get the latest finished model, which can be a few keystrokes old.
    // Only one parse runs at a time, changes made while parsing are parsed together afterwards
    public class ScriptModelCache
    {
        private const int ParseDelay = 100;

        private static ScriptModel emptyModel;

        private readonly object parseLock = new object();
        private readonly object requestLock = new object();
        private ParseRequest pendingRequest;
        private bool isParsing;
        private volatile ParsedScript current;

        public ScriptModelCache()
        {
            AnalyzeInBackground = true;
        }

        // Parse on the calling thread instead, used by tests
        public bool AnalyzeInBackground { get; set; }

        // Raised on a background thread when a new model is available
        public event EventHandler ModelUpdated;

        private static ScriptModel EmptyModel
        {
            get { return emptyModel ?? (emptyModel = ScriptModel.ParseTolerant(string.Empty)); }
        }

        // Starts analyzing the script in the background if the latest model is not for this version of the script
        public void Update(string script, int offset)
        {
            var request = new ParseRequest(script, offset);
            var parsed = current;

            if (parsed == null || !parsed.IsFor(request))
                ParseInBackground(request, parsed == null);
        }

        public ScriptModel GetModel(string script, int offset, out int modelOffset)
        {
            var request = new ParseRequest(script, offset);
            modelOffset = request.ModelOffset;

            var parsed = current;
            if (parsed != null && parsed.IsFor(request))
                return parsed.Model;

            if (!AnalyzeInBackground)
                return Parse(request).Model;

            // Until the first analysis is done there are no script names to suggest
            ParseInBackground(request, parsed == null);
            return parsed != null ? parsed.Model : EmptyModel;
        }

        private void ParseInBackground(ParseRequest request, bool immediately)
        {
            lock (requestLock)
            {
                pendingRequest = request;

                if (isParsing)
                    return;

                isParsing = true;
            }

            Task.Delay(immediately ? 0 : ParseDelay).ContinueWith(task => ParsePendingRequests(), TaskScheduler.Default);
        }

        private void ParsePendingRequests()
        {
            while (true)
            {
                ParseRequest request;

                lock (requestLock)
                {
                    request = pendingRequest;
                    pendingRequest = null;

                    if (request == null)
                    {
                        isParsing = false;
                        return;
                    }
                }

                try
                {
                    Parse(request);

                    var handler = ModelUpdated;
                    if (handler != null)
                        handler(this, EventArgs.Empty);
                }
                catch (Exception)
                { }
            }
        }

        private ParsedScript Parse(ParseRequest request)
        {
            lock (parseLock)
            {
                var parsed = current;
                if (parsed != null && parsed.IsFor(request))
                    return parsed;

                parsed = new ParsedScript(request, ScriptModel.ParseTolerant(request.Text));
                current = parsed;

                return parsed;
            }
        }

        // The line being edited is usually not valid Python, it's replaced with a statement of the same indentation
        // so that the rest of the script parses and the scope of the line is kept
        private class ParseRequest
        {
            public ParseRequest(string script, int offset)
            {
                var lineStart = ScriptTextHelper.GetLineStart(script, offset);
                var lineEnd = ScriptTextHelper.GetLineEnd(script, offset);
                var line = script.Substring(lineStart, lineEnd - lineStart);
                var indent = new string(line.TakeWhile(char.IsWhiteSpace).ToArray());
                var isBlockHeader = ScriptTextHelper.StripComment(line).TrimEnd().EndsWith(":");
                var replacement = indent + (isBlockHeader ? "if 1:" : "pass");

                Script = script;
                LineStart = lineStart;
                ModelOffset = lineStart + indent.Length;
                Text = script.Substring(0, lineStart) + replacement + script.Substring(lineEnd);
            }

            public string Script { get; private set; }
            public int LineStart { get; private set; }
            public int ModelOffset { get; private set; }
            public string Text { get; private set; }
        }

        private class ParsedScript
        {
            private readonly ParseRequest request;

            public ParsedScript(ParseRequest request, ScriptModel model)
            {
                this.request = request;
                Model = model;
            }

            public ScriptModel Model { get; private set; }

            public bool IsFor(ParseRequest other)
            {
                return request.LineStart == other.LineStart && request.Script == other.Script;
            }
        }
    }
}
