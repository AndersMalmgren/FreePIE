using System;
using System.Collections.Generic;
using Caliburn.Micro;
using FreePIE.GUI.Result;

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
            Coroutine.BeginExecute(CanClose().GetEnumerator(), null, (s, e) => callback(!e.WasCancelled));
        }

        protected virtual IEnumerable<IResult> CanClose()
        {
            yield break;
        }
    }
}
