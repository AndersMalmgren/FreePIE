using System.Windows;
using Caliburn.Micro;

namespace FreePIE.GUI.Result
{
    public class CloseAppResult : CancelResult
    {
        public override void Execute(CoroutineExecutionContext context)
        {
            Application.Current.Shutdown();
            base.Execute(context);
        }
    }
}
