using System;
using System.Text;
using FreePIE.Core.Contracts;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace FreePIE.Core.Plugins
{
    [GlobalType(Type = typeof(WindowGlobal))]
    public class Window : Plugin
    {
        private string ActiveWindow;

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public override string FriendlyName
        {
            get
            {
                return "Window";
            }
        }

        public override object CreateGlobal()
        {
            return new WindowGlobal(this);
        }

        public override Action Start()
        {
            ActiveWindow = "not initialized";
            return null; // TODO?
        }

        public override void DoBeforeNextExecute()
        {
            uint pid = 0;

            IntPtr handle = GetForegroundWindow();

            GetWindowThreadProcessId(handle, out pid);

            try
            {
                var processId = Convert.ToInt32(pid);

                var process = Process.GetProcessById(Convert.ToInt32(processId));
                if ( process != null)
                {
                    ActiveWindow = process.ProcessName;
                } else {
                    ActiveWindow = "unable to find foreground window";
                }
            } catch (Exception ex)
            {
                ActiveWindow = "error while finding foreground window: " + ex.ToString();
            }


        }

        public string GetActiveWindow
        {
            get { return ActiveWindow; }
        }
    }

    [Global(Name = "Window")]
    public class WindowGlobal
    {
        private readonly Window plugin;

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public WindowGlobal(Window plugin)
        {
            this.plugin = plugin;
        }

        public bool is_active(string title)
        {
            return String.Equals(plugin.GetActiveWindow, title);
        }

        public bool activate(string title)
        {
            System.Diagnostics.Process[] p = System.Diagnostics.Process.GetProcessesByName(title);
            if (p.Length > 0)
            {
                SetForegroundWindow(p[0].MainWindowHandle);
                return true;
            }
            return false; // TODO
        }

        public string active
        {
            get { return plugin.GetActiveWindow; }
        }
    }

}
