using System.Windows;
using Caliburn.Micro;

namespace FreePIE.GUI.Result
{
    public class CloseResult : Result
    {
        public override void Execute(ActionExecutionContext context)
        {
            var window = Window.GetWindow(context.View);
            window.Close();
            
            base.Execute(context);
        }
    }
}
