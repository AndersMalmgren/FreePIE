using Caliburn.Micro;
using Action = System.Action;

namespace FreePIE.GUI.Result
{
    public class CancelResult : Result
    {
        public override void Execute(CoroutineExecutionContext context)
        {
            OnCompleted(this, new ResultCompletionEventArgs { WasCancelled = true });
        }
    }
}
