using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using FreePIE.GUI.Result;
using Action = System.Action;

namespace FreePIE.GUI.Shells
{
    public abstract class ShellPresentationModel : Screen
    {
        public ShellPresentationModel(IResultFactory resultFactory)
        {
            Result = resultFactory;
        }
        
        public IResultFactory Result { get; private set; }

        public override void CanClose(Action<bool> callback)
        {
            var cancel = false;
            var cancelCallback = new Action(() => cancel = true);

            EventHandler<ResultCompletionEventArgs> completed = (s, e) =>
            {
                if (!cancel)
                    base.CanClose(callback);
            };

            var results = CanClose(cancelCallback);
            if (results != null)
                Coroutine.BeginExecute(results.GetEnumerator(), null, completed);
            else 
                completed(null, null);

        }

        protected virtual IEnumerable<IResult> CanClose(Action cancelCallback)
        {
            return null;
        }
    }
}
