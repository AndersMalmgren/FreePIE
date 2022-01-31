using System;
using System.Diagnostics;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.Common.System
{
    public class MainWindowHandleProvider : IHandleProvider
    {
        public IntPtr Handle { get { return Process.GetCurrentProcess().MainWindowHandle; } }
    }
}
