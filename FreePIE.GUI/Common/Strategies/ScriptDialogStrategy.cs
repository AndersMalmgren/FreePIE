using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using FreePIE.Core.Common;
using FreePIE.GUI.Result;
using FreePIE.GUI.Views.Main;

namespace FreePIE.GUI.Common.Strategies
{
    public class ScriptDialogStrategy
    {
        private readonly IResultFactory resultFactory;
        private readonly IFileSystem fileSystem;
        private const string fileFilter = "Python scripts (*.py)|*.py|All files (*.*)|*.*";

        public ScriptDialogStrategy(IResultFactory resultFactory, IFileSystem fileSystem)
        {
            this.resultFactory = resultFactory;
            this.fileSystem = fileSystem;
        }

        public IEnumerable<IResult> SaveAs(PanelViewModel document, bool quickSave, Action<string> onSaved)
        {
            if (quickSave && !string.IsNullOrEmpty(document.FilePath))
            {
                onSaved(document.FilePath);
            }
            else
            {
                var result = resultFactory.ShowFileDialog("Save script", fileFilter, FileDialogMode.Save, document.FilePath);
                yield return result;

                if (!string.IsNullOrEmpty(result.File))
                    onSaved(result.File);
            }

        }

        public IEnumerable<IResult> Open(Action<string> onOpened)
        {
            var result = resultFactory.ShowFileDialog("Open script", fileFilter, FileDialogMode.Open);
            yield return result;

            if (!string.IsNullOrEmpty(result.File))
                onOpened(result.File);
        }
    }
}
