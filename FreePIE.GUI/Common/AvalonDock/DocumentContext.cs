using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvalonDock;
using Caliburn.Micro;

namespace FreePIE.GUI.Common.AvalonDock
{
    public static class DocumentContext
    {
        public static void Init()
        {
            MessageBinder.SpecialValues.Add("$documentcontext", context =>
                {
                    var args = context.EventArgs as DocumentClosingEventArgs;

                    var fe = args.Document;
                    return fe.Content;
                });
        }
    }
}
