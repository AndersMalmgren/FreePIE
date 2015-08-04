using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using FreePIE.Core.Persistence;
using FreePIE.GUI.Result;
using Action = System.Action;
using MessageBoxResult = System.Windows.MessageBoxResult;

namespace FreePIE.GUI.Views.Main
{
    public class SettingsLoaderViewModel
    {
        private readonly IPersistanceManager persistanceManager;
        private readonly IResultFactory result;

        public SettingsLoaderViewModel(IPersistanceManager persistanceManager, IResultFactory result)
        {
            this.persistanceManager = persistanceManager;
            this.result = result;
        }

        public IEnumerable<IResult> Load(Action onloaded)
        {
            if(!persistanceManager.Load())
            {
                var message = result.ShowMessageBox("FreePIE settings are corrupted", "Continuing your use of the program will reset it to default settings. Continue anyway?", MessageBoxButton.OKCancel);
                yield return message;

                if(message.Result == MessageBoxResult.Cancel)
                {
                    yield return result.CloseApp();
                }
            } 

            onloaded();
        }
    }
}
