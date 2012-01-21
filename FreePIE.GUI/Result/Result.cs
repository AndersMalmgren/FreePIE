using System;
using Caliburn.Micro;

namespace FreePIE.GUI.Result
{
    public abstract class Result : IResult
    {
        public virtual void Execute(ActionExecutionContext context)
        {
            OnCompleted(this, new ResultCompletionEventArgs());
        }

        protected virtual void OnCompleted(object sender, ResultCompletionEventArgs e)
        {
            if (Completed != null)
                Completed(sender, e);
        }

        public event EventHandler<ResultCompletionEventArgs> Completed;
    }
}
