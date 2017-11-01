using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreePIE.GUI.Views.Main.Menu
{
    public class RecentFileViewModel
    {
        private int index;
        private const string shortcutKey = "_";

        public RecentFileViewModel(string file, int index)
        {
            File = file;
            this.index = index++;
            Caption = string.Format("{0} {1}", GetShortcut(), file.Replace(shortcutKey, "__"));
        }

        private string GetShortcut()
        {
            var str = index.ToString();
            if (str.Length == 1) return shortcutKey+str;

            return str.Substring(0, str.Length - 1) + shortcutKey + str.Substring(str.Length-1);
        }

        public string File { get; private set; }

        public string Caption { get; private set; }


    }
}
