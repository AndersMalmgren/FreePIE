﻿using Caliburn.Micro;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout;

namespace FreePIE.GUI.Common.AvalonDock
{
    public static class DocumentContext
    {
        public static void Init()
        {
            MessageBinder.SpecialValues.Add("$documentcontext", context =>
                {
                    LayoutDocument doc = null;
                    if (context.EventArgs is DocumentClosingEventArgs)
                    {
                        var args = context.EventArgs as DocumentClosingEventArgs;

                        doc = args.Document;
                    } 
                    else if (context.EventArgs is DocumentClosedEventArgs)
                    {
                        var args = context.EventArgs as DocumentClosedEventArgs;
                        doc = args.Document;
                    }

                    return doc.Content;
                });
        }
    }
}
