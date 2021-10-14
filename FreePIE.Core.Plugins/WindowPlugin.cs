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

        public bool Start(string processName)
        {
            var p = Process.Start(processName);
            if (p == null) return false;
            return true;
        }

        public bool Start(string processName, string arguments)
        {
            var p = Process.Start(processName, arguments);
            if (p == null) return false;
            return true;
        }

        public bool Close(string processName)
        {
            var p = Process.GetProcessesByName(processName);
            if (!p.Any()) return false;

            p.First().Close();
            return true;
        }

        public bool CloseMainWindow(string processName)
        {
            var p = Process.GetProcessesByName(processName);
            if (!p.Any()) return false;

            return p.First().CloseMainWindow();
        }
    }

    [Global(Name = "window")]
    public class WindowGlobal : UpdateblePluginGlobal<WindowPlugin>
    {
        public WindowGlobal(WindowPlugin plugin) : base(plugin) { }

        public bool isActive(string processName)
        {
            return plugin.IsActive(processName);
        }

        public bool setActive(string processName)
        {
            return plugin.Activate(processName);
        }

        public string getActive
        {
            get { return plugin.GetActiveWindowProcessName(); }
        }

        public bool open(string processName)
        {
            return plugin.Start(processName);
        }

        public bool open(string processName, string arguments)
        {
            return plugin.Start(processName, arguments);
        }

        public bool close(string processName)
        {
            return plugin.Close(processName);
        }

        public bool closeMainWindow(string processName)
        {
            return plugin.CloseMainWindow(processName);
        }

        public int pollingInterval
        {
            set { plugin.PollingInterval = value; }
            get { return plugin.PollingInterval; }
        }
    }

}
