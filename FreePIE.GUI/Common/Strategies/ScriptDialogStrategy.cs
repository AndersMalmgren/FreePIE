using System;
using System.Collections.Generic;
using Caliburn.Micro;
using FreePIE.GUI.Result;
using FreePIE.GUI.Views.Main;

namespace FreePIE.GUI.Common.Strategies
{
    public class ScriptDialogStrategy
    {
        private readonly IResultFactory resultFactory;
        private const string fileFilter = "Python scripts (*.py)|*.py|All files (*.*)|*.*";

        public ScriptDialogStrategy(IResultFactory resultFactory)
        {
            this.resultFactory = resultFactory;
        }

        public IEnumerable<IResult> SaveAs(PanelViewModel document, bool quickSave, Action<string> fileSelected)
        {
            if (quickSave && !string.IsNullOrEmpty(document.FilePath))
            {
                fileSelected(document.FilePath);
            }
            else
            {
                var result = resultFactory.ShowFileDialog("Save script", fileFilter, FileDialogMode.Save, document.FilePath);
                yield return result;

                if (!string.IsNullOrEmpty(result.File))
                    fileSelected(result.File);
            }

        }

        public IEnumerable<IResult> Open(Action<string> fileSelected)
        {
            var result = resultFactory.ShowFileDialog("Open script", fileFilter, FileDialogMode.Open);
            yield return result;

            if (!string.IsNullOrEmpty(result.File))
                fileSelected(result.File);
        }
    }
}
