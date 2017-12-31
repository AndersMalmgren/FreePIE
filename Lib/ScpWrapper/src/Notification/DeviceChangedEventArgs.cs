using System;

namespace ScpDotNet.Notification
{
    public class NotifyEventArgs : EventArgs
    {
        public readonly Notified NotificationType;
        public readonly string DriverGuid;
        public readonly string DevicePath;

        public NotifyEventArgs(Notified notificationType, string driverGuid, string devicepath)
        {
            NotificationType = notificationType;
            DriverGuid = driverGuid;
            DevicePath = devicepath;
        }

    }
}