using System.Windows;
using Caliburn.Micro;

namespace FreePIE.GUI.Result
{
    public class CloseResult : Result
    {
        public override void Execute(ActionExecutionContext context)
        {
            var window = context.View as Window;
            if(window != null)
            {
                window.Close();
            }
            
            base.Execute(context);
        }
    }
}
