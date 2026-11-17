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
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public override string FriendlyName
        {
            get { return "Window"; }
        }

        private Stopwatch timer;
        private string activeWindow;
        public int PollingInterval { get; set; } = 100;


        public override Action Start()
        {
            timer = new Stopwatch();
            timer.Start();

            return null;
        }

        public override void Stop()
        {
            timer.Stop();
        }

        public override void DoBeforeNextExecute()
        {
            if (timer.ElapsedMilliseconds > PollingInterval)
            {
                timer.Restart();
                var lastActiveWindow = activeWindow;
                activeWindow = null;

                if (GlobalHasUpdateListener && lastActiveWindow != GetActiveWindowProcessName())
                    OnUpdate();
            }
        }
        
        public string GetActiveWindowProcessName()
        {
            if(activeWindow != null) return activeWindow;
            
            var handle = GetForegroundWindow();
            uint pid = 0;
            GetWindowThreadProcessId(handle, out pid);

            var process = Process.GetProcessById((int)pid);
            return activeWindow = process.ProcessName;
        }

        public override object CreateGlobal()
        {
            return new WindowGlobal(this);
        }

        public bool IsActive(string processName)
        {
            return GetActiveWindowProcessName().Equals(processName, StringComparison.InvariantCultureIgnoreCase);
        }

        public bool Activate(string processName)
        {
            var p = Process.GetProcessesByName(processName);
            if (!p.Any()) return false;

            SetForegroundWindow(p.First().MainWindowHandle);
            return true;
        }
    }

    [Global(Name = "window")]
    [Documentation("Checks and changes the active window. The update event is raised when the active window changes")]
    public class WindowGlobal : UpdateblePluginGlobal<WindowPlugin>
    {
        public WindowGlobal(WindowPlugin plugin) : base(plugin) { }

        [Documentation("True if the active window belongs to the process, the name is without .exe and not case sensitive")]
        public bool isActive(string processName)
        {
            return plugin.IsActive(processName);
        }

        [Deprecated("activate")]
        public bool setActive(string processName)
        {
            return plugin.Activate(processName);
        }

        [Documentation("Brings the main window of the process to the front, returns false if the process is not running")]
        public bool activate(string processName)
        {
            return plugin.Activate(processName);
        }

        [Deprecated("active")] public string getActive => active;

        [Documentation("Process name of the active window")]
        public string active => plugin.GetActiveWindowProcessName();


        [Documentation("How often the active window is checked for the update event, in milliseconds (default 100)")]
        public int pollingInterval
        {
            set { plugin.PollingInterval = value; }
            get { return plugin.PollingInterval; }
        }
    }

}
