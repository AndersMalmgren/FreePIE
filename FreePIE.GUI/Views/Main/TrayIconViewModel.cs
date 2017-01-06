using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Caliburn.Micro;
using FreePIE.Core.Model.Events;
using FreePIE.GUI.Common.CommandLine;
using FreePIE.GUI.Common.TrayIcon;
using FreePIE.GUI.Events.Command;
using FreePIE.GUI.Result;
using FreePIE.GUI.Shells;
using Hardcodet.Wpf.TaskbarNotification;
using IEventAggregator = FreePIE.Core.Common.Events.IEventAggregator;

namespace FreePIE.GUI.Views.Main
{
    public class TrayIconViewModel : Screen, ITrayIcon, 
        Core.Common.Events.IHandle<TrayNotificationEvent>, 
        Core.Common.Events.IHandle<TrayEvent>
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IResultFactory resultFactory;
        private readonly IWindowManager windowManager;
        private readonly MainShellViewModel shellViewModel;
        private readonly IParser parser;

        public TrayIconViewModel(IResultFactory resultFactory, IEventAggregator eventAggregator, IWindowManager windowManager,
            MainShellViewModel shellViewModel,
            IParser parser)
        {
            this.resultFactory = resultFactory;
            this.windowManager = windowManager;
            this.parser = parser;
            this.shellViewModel = shellViewModel;

            this.eventAggregator = eventAggregator;
            this.eventAggregator.Subscribe(this);
        }

        #region ITrayIcon Implementation

        protected override void OnActivate()
        {
            base.OnActivate();

            NotifyOfPropertyChange(() => CanShowWindow);
            NotifyOfPropertyChange(() => CanHideWindow);
        }

        public bool CanShowWindow => (!shellViewModel.IsActive);

        /// <summary>
        /// If true application starts minimized in the taskbar
        /// </summary>
        private bool StartInTray { get; set; }
        public void OnTrayIconLoaded(object source)
        {
            TaskbarTrayIcon = ((ContentControl)source).Content as TaskbarIcon;
            
            parser.ParseAndExecute();

            if (CanShowWindow && !StartInTray)
                ShowWindow();
            //int i = Array.IndexOf(w.startupArgs, "/open");


        }

        public void ShowWindow()
        {
            windowManager.ShowWindow(shellViewModel);

            NotifyOfPropertyChange(() => CanShowWindow);
            NotifyOfPropertyChange(() => CanHideWindow);
        }

        public bool CanHideWindow
        {
            get
            {
                return (shellViewModel.IsActive);
            }
        }

        public void HideWindow()
        {
            shellViewModel.TryClose();

            NotifyOfPropertyChange(() => CanShowWindow);
            NotifyOfPropertyChange(() => CanHideWindow);
        }

        public void ExitApplication()
        {
             resultFactory.CloseApp().Execute(null);
        }

        public IEnumerable<IResult> Close()
        {
            yield return resultFactory.CloseApp();
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
            TaskbarTrayIcon?.ShowBalloonTip(title, message, balloon);
        }

        public void ShowBalloonTip(UIElement rootModel, PopupAnimation animation, int timeout = 4000)
        {
            TaskbarTrayIcon?.ShowCustomBalloon(rootModel, animation, timeout);
        }


        /*public void ShowBalloonTip(object rootModel, PopupAnimation animation, TimeSpan? timeout = null)
        {
            Icon.ShowBalloonTip(rootModel, animation, timeout);
        }*/

        public void CloseBalloon()
        {
            TaskbarTrayIcon?.CloseBalloon();
        }
        #endregion

        public void Dispose()
        {
            TaskbarTrayIcon?.Dispose();
        }

        /// <summary>
        /// Show a balloon notification in the taskbar
        /// </summary>
        /// <param name="message"></param>
        public void Handle(TrayNotificationEvent message)
        {
            TaskbarTrayIcon?.ShowBalloonTip(message.Title, message.message, BalloonIcon.Info);
        }

        public void Handle(TrayEvent message)
        {
            //prevent main window from showing automatically
            StartInTray = true;
        }

        #endregion
    }
}
