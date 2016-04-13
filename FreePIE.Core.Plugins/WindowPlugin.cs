using System;
using System.Text;
using FreePIE.Core.Contracts;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Linq;

namespace FreePIE.Core.Plugins
{
    [GlobalType(Type = typeof(WindowGlobal))]
    public class WindowPlugin : Plugin
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public override string FriendlyName
        {
            get
            {
                return "Window";
            }
        }

        public override void DoBeforeNextExecute()
        {
            activeWindow = null;
        }

        private string activeWindow;

        public string GetActiveWindow()
        {
            if(activeWindow != null) return activeWindow;

            uint pid = 0;

            IntPtr handle = GetForegroundWindow();

            GetWindowThreadProcessId(handle, out pid);
            var processId = Convert.ToInt32(pid);

            var process = Process.GetProcessById(Convert.ToInt32(processId));
            return activeWindow = process.MainWindowTitle;
        }

        public override object CreateGlobal()
        {
            return new WindowGlobal(this);
        }

        public override Action Start()
        {
            return null;
        }

        public bool IsActive(string title)
        {
            return GetActiveWindow().Equals(title, StringComparison.InvariantCultureIgnoreCase);
        }

        public bool Activate(string title)
        {
            Process[] p = Process.GetProcessesByName(title);
            if (!p.Any()) return false;

            SetForegroundWindow(p[0].MainWindowHandle);
            return true;
        }
    }

    [Global(Name = "window")]
    public class WindowGlobal
    {
        private readonly WindowPlugin plugin;

        public WindowGlobal(WindowPlugin plugin)
        {
            this.plugin = plugin;
        }

        public bool isActive(string title)
        {
            return plugin.IsActive(title);
        }

        public bool activate(string title)
        {
            return plugin.Activate(title);
        }

        public string active
        {
            get { return plugin.GetActiveWindow(); }
        }
    }

}
