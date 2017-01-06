using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using Hardcodet.Wpf.TaskbarNotification;

namespace FreePIE.GUI.Common.TrayIcon
{
    public interface ITrayIcon : IDisposable
    {
        TaskbarIcon TaskbarTrayIcon { get; }
        void ShowWindow();
        void HideWindow();
        void ExitApplication();


        void ShowBalloonTip(string title, string message, BalloonIcon balloon);

        void ShowBalloonTip(UIElement rootModel, PopupAnimation animation, int timeout = 4000);

        void CloseBalloon();

        void OnTrayIconLoaded(object source);
    }
}
