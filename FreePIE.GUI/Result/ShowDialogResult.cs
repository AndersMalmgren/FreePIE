using System;
using Caliburn.Micro;

namespace FreePIE.GUI.Result
{
    public class ShowDialogResult<TModel> : Result
    {
        private readonly IWindowManager windowManager;
        private readonly TModel model;
        private Action<TModel> configure;

        public ShowDialogResult(IWindowManager windowManager, TModel model)
        {
            this.windowManager = windowManager;
            this.model = model;
        }

        public IResult Configure(Action<TModel> configure)
        {
           this.configure = configure;
           return this;
        }

        public override void Execute(CoroutineExecutionContext context)
        {
            if(configure != null)
                configure(model);

            windowManager.ShowDialog(model);
            
            base.Execute(context);
        }
    }
}
