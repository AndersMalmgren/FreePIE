using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Caliburn.Micro;
using FreePIE.Core.Model.Events;
using FreePIE.Core.Persistence;
using FreePIE.GUI.Events;
using FreePIE.GUI.Events.Command;
using FreePIE.GUI.Result;
using FreePIE.GUI.Shells;
using Hardcodet.Wpf.TaskbarNotification;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using IEventAggregator = FreePIE.Core.Common.Events.IEventAggregator;

namespace FreePIE.GUI.Views.Main
{
    public class TrayIconViewModel : PropertyChangedBase,

        Core.Common.Events.IHandle<TrayNotificationEvent>,
        Core.Common.Events.IHandle<TrayEvent>,
        Core.Common.Events.IHandle<ActiveScriptDocumentChangedEvent>,
        Core.Common.Events.IHandle<ScriptStateChangedEvent>,
        Core.Common.Events.IHandle<WindowStateChangedEvent>,
        Core.Common.Events.IHandle<StartedEvent>,
        Core.Common.Events.IHandle<ExitingEvent>,
        Core.Common.Events.IHandle<ScriptErrorEvent>,
    IDisposable
    {
        private readonly IEventAggregator eventAggregator;
        
        private readonly ISettingsManager settingsManager;
        private readonly IResultFactory resultFactory;
        private readonly MainShellViewModel shellViewModel;
        private ScriptStateChangedEvent lastScriptEvent;
        private WindowStateChangedEvent lastWindowEvent = new WindowStateChangedEvent(WindowState.Minimized, true);

        private bool startInTray;

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);


        public TrayIconViewModel(IResultFactory resultFactory,
                                IEventAggregator eventAggregator,
                                MainShellViewModel shellViewModel,
                                ISettingsManager settingsManager
                                ) 
        {
            this.resultFactory = resultFactory;
            this.shellViewModel = shellViewModel;
            this.settingsManager = settingsManager;
            this.eventAggregator = eventAggregator;
            this.eventAggregator.Subscribe(this);
        }

        public void OnTrayIconLoaded(object source)
        {
            TaskbarTrayIcon = (TaskbarIcon)((ContentControl)source).Content;
        }

        public bool CanShowWindow
        {
            get { return true; }
        }

        public bool CanHideWindow
        {
            get { return lastWindowEvent.ShowInTaskBar == true; }
            set { }
        }

        public void ShowWindow()
        {
            if (lastWindowEvent.WindowState != WindowState.Maximized)
                shellViewModel.WindowState = WindowState.Normal;
            SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);
        }

        public void HideWindow()
        {
            shellViewModel.WindowState = WindowState.Minimized;
            if (!settingsManager.Settings.MinimizeToTray)
            {
                shellViewModel.ShowInTaskBar = false;
                CanHideWindow = false;
            }
        }

        private PanelViewModel activeDocument;
        private PanelViewModel ActiveDocument
        {
            get { return activeDocument; }
            set
            {
                activeDocument = value;
                NotifyOfPropertyChange(() => CanRunScript);
                NotifyOfPropertyChange(() => CanStopScript);
                NotifyOfPropertyChange(() => CanActionScript);
            }
        }

        public bool CanStopScript
        {
            get
            {
                if (lastScriptEvent == null)
                    return false;
                return lastScriptEvent.Running;
            }
        }

        public bool CanRunScript
        {
            get
            {
                if (lastScriptEvent == null || activeDocument == null)
                    return false;
                return !lastScriptEvent.Running 
                    && !string.IsNullOrEmpty(activeDocument.FileContent);
            }
        }

        public bool CanActionScript
        {
            get { return CanStopScript || CanRunScript; }
        }

        public void RunScript()
        {
            shellViewModel.Menu.RunScript();
        }

        public void StopScript()
        {
            shellViewModel.Menu.StopScript();
        }

        public void ActionScript()
        {
            if (CanStopScript)
                StopScript();
            else if (CanRunScript)
                RunScript();
            /*/
            else
                OpenScript();
            //*/
        }

        /*/
        public IEnumerable<IResult> OpenScript()
        {
            if (lastWindowEvent.WindowState != WindowState.Maximized)
                shellViewModel.WindowState = WindowState.Normal;
            SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);
            return shellViewModel.Menu.OpenScript();
        }
        //*/

        public IEnumerable<IResult> Close()
        {
            /*/
            return shellViewModel.Menu.Close();
            /*/
            yield return resultFactory.CloseApp();
            //*/
        }

        public string ToolTipText
        {
            get
            {
                if (lastScriptEvent != null)
                {
                    if (lastScriptEvent.Running)
                    {
                        return string.Format("Running \"{0}\"", lastScriptEvent.Script);
                    }
                }

                return "Double-click for window, right-click for menu";
            }
        }

        public ScriptStateChangedEvent LastScriptEvent
        {
            get { return lastScriptEvent; }
            set
            {
                lastScriptEvent = value;
                NotifyOfPropertyChange(() => CanRunScript);
                NotifyOfPropertyChange(() => CanStopScript);
                NotifyOfPropertyChange(() => CanActionScript);
            }
        }

        public WindowStateChangedEvent LastWindowEvent
        {
            get { return lastWindowEvent; }
            set
            {
                lastWindowEvent = value;
                NotifyOfPropertyChange(() => CanHideWindow);
                NotifyOfPropertyChange(() => CanShowWindow);
            }
        }

        /// <summary>
        /// If true application starts minimized in the taskbar
        /// </summary>
        public bool MinimizeToTray
        {
            get { return settingsManager.Settings.MinimizeToTray; }
            set
            {
                settingsManager.Settings.MinimizeToTray = value;
                this.shellViewModel.ShowInTaskBar = !value;
                NotifyOfPropertyChange(() => MinimizeToTray);
            }
        }

        #region TrayIcon

        public TaskbarIcon TaskbarTrayIcon
        {
            get;
            private set;
        }

        public void ShowBalloonTip(string title, string message)
        {
            ShowBalloonTip(title, message, BalloonIcon.Info);
        }
        public void ShowBalloonTip(string title, string message, BalloonIcon balloon)
        {
            //Icon.ShowBalloonTip(title, message, balloon);
            if (TaskbarTrayIcon != null) TaskbarTrayIcon.ShowBalloonTip(title, message, balloon);
        }

        public void ShowBalloonTip(UIElement rootModel, PopupAnimation animation, int timeout = 4000)
        {
            if (TaskbarTrayIcon != null) TaskbarTrayIcon.ShowCustomBalloon(rootModel, animation, timeout);
        }


        /*public void ShowBalloonTip(object rootModel, PopupAnimation animation, TimeSpan? timeout = null)
        {
            Icon.ShowBalloonTip(rootModel, animation, timeout);
        }*/

        public void CloseBalloon()
        {
            if (TaskbarTrayIcon != null) TaskbarTrayIcon.CloseBalloon();
        }

        #endregion

        public void Dispose()
        {
            if (TaskbarTrayIcon != null) TaskbarTrayIcon.Dispose();
        }

        /// <summary>
        /// Show a balloon notification in the taskbar
        /// </summary>
        /// <param name="message"></param>
        public void Handle(TrayNotificationEvent message)
        {
            if (TaskbarTrayIcon != null)
                TaskbarTrayIcon.ShowBalloonTip(message.Title, message.message, BalloonIcon.Info);
        }

        public void Handle(TrayEvent message)
        {
            //prevent main window from showing automatically
            startInTray = true;
        }

        public void Handle(ActiveScriptDocumentChangedEvent message)
        {
            ActiveDocument = message.Document;
        }

        public void Handle(ScriptStateChangedEvent message)
        {
            LastScriptEvent = message;
        }

        public void Handle(WindowStateChangedEvent message)
        {
            LastWindowEvent = message;
        }

        public void Handle(StartedEvent message)
        {
            if (!startInTray)
                ShowWindow();
        }

        public void Handle(ExitingEvent message)
        {
            Application.Current.Shutdown();
        }

        public void Handle(ScriptErrorEvent message)
        {
            BalloonIcon ico = message.Level == ErrorLevel.Warning ? BalloonIcon.Warning : BalloonIcon.Error;
            if (TaskbarTrayIcon != null && shellViewModel.WindowState == WindowState.Minimized)
                TaskbarTrayIcon.ShowBalloonTip("Script Error", message.Description, ico);
        }
    }
}
