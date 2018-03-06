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

        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetPackageFullName(IntPtr hProcess, ref UInt32 packageFullNameLength, StringBuilder fullName);


        public override string FriendlyName
        {
            get { return "Window"; }
        }

        private Stopwatch timer;
        private string activeWindow;
        public int PollingInterval { get; set; } = 100;

        private delegate bool EnumWindowProc(IntPtr hwnd, IntPtr lParam);


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
            if (activeWindow != null) return activeWindow;

            var handle = GetForegroundWindow();
            uint pid = 0;
            GetWindowThreadProcessId(handle, out pid);

            var process = Process.GetProcessById((int)pid);

            if (String.Equals(process.ProcessName, "ApplicationFrameHost", StringComparison.OrdinalIgnoreCase))
            {
                EnumWindowProc childProc = new EnumWindowProc(delegate (IntPtr childHwnd, IntPtr lParam)
                {
                    int ERROR_SUCCESS = 0x0;
                    int APPMODEL_ERROR_NO_PACKAGE = 0x3D54;
                    int ERROR_INSUFFICIENT_BUFFER = 0x7A;

                    GetWindowThreadProcessId(childHwnd, out pid);
                    process = Process.GetProcessById((int)pid);
                    uint len = 256;
                    StringBuilder sb = new StringBuilder((int)len);
                    var err = GetPackageFullName(process.Handle, ref len, sb);

                    if (err == APPMODEL_ERROR_NO_PACKAGE)
                    {
                        return true;
                    }
                    else if (err == ERROR_INSUFFICIENT_BUFFER)
                    {
                        Debug.WriteLine("Insuffucient buffer length.");
                        return false;
                    }
                    else if (err == ERROR_SUCCESS)
                    {
                        // activeWindow = sb.ToString();
                        activeWindow = process.ProcessName;
                        return false;
                    }
                    else
                    {
                        return false;
                    }
                });
                EnumChildWindows(handle, childProc, IntPtr.Zero);
                return activeWindow;
            }
            else
            {
                return activeWindow = process.ProcessName;
            }
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

        public int pollingInterval
        {
            set { plugin.PollingInterval = value; }
            get { return plugin.PollingInterval; }
        }
    }

}
