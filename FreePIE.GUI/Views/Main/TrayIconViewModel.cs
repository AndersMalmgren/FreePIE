using System;
using System.Collections.Generic;
using Caliburn.Micro;
using FreePIE.Core.Model.Events;
using FreePIE.Core.Persistence;
using FreePIE.GUI.Common.CommandLine;
//using FreePIE.GUI.Common.TrayIcon;
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
        Core.Common.Events.IHandle<ScriptStateChangedEvent>,
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

        private bool startInTray;

        
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

        public bool CanShowWindow()
        {
            return shellViewModel.IsActive && shellViewModel.WindowState == WindowState.Minimized ;
        }
        public void ShowWindow()
        {
            shellViewModel.WindowState = WindowState.Normal;
        }

        public bool CanStopScript()
        {
            if (LastScriptEvent == null)
                return false;
            return LastScriptEvent.Running;
        }

        public bool CanRunScript()
        {
            if (LastScriptEvent == null)
                return true;
            return !LastScriptEvent.Running;
        }
        public void RunScript()
        {
            shellViewModel.Menu.RunScript();
        }

        public void StopScript()
        {
            shellViewModel.Menu.StopScript();
        }
        public IEnumerable<IResult> Close()
        {
            yield return resultFactory.CloseApp();
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

        /// <summary>
        /// If true application starts minimized in the taskbar
        /// </summary>
        public ScriptStateChangedEvent LastScriptEvent
        {
            get { return lastScriptEvent; }
            set
            {
                if (lastScriptEvent == null)
                {
                    lastScriptEvent = value;
                    NotifyOfPropertyChange(() => LastScriptEvent);
                }
                else if (lastScriptEvent.Running != value.Running)
                {
                    lastScriptEvent = value;
                    NotifyOfPropertyChange(() => LastScriptEvent);
                }
            }
        }
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

        public void Handle(ScriptStateChangedEvent message)
        {
            LastScriptEvent = message;
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
