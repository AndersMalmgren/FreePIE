using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.Core.ScriptEngine.Python.Ast
{
    public class ScriptEdit
    {
        public ScriptEdit(int index, string text)
        {
            Index = index;
            Text = text;
        }

        public int Index { get; private set; }
        public string Text { get; private set; }
    }

    public static class ScriptEditExtensions
    {
        // Edits are applied from the end of the script so that the indexes of the remaining edits stay valid
        public static string ApplyEdits(this string script, IEnumerable<ScriptEdit> edits)
        {
            var builder = new StringBuilder(script);

            foreach (var edit in edits.Select((e, i) => new { Edit = e, Order = i }).OrderByDescending(e => e.Edit.Index).ThenByDescending(e => e.Order))
                builder.Insert(edit.Edit.Index, edit.Edit.Text);

            return builder.ToString();
        }
    }
}
