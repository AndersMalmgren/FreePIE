using System.Windows;
using Caliburn.Micro;

namespace FreePIE.GUI.Result
{
    public class CloseResult : Result
    {
        public override void Execute(CoroutineExecutionContext context)
        {
            var window = Window.GetWindow(context.View as DependencyObject);
            window.Close();
            
            base.Execute(context);
        }
    }
}
