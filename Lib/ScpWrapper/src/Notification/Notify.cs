using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScpDotNet.Notification
{
    internal class Notify
    {

        internal event EventHandler<NotifyEventArgs> DeviceChanged;

        #region Notification - RegisterNotify,UnregisterNotify (This might not belong here)



        /// <summary>
        /// Register for device notification
        /// </summary>
        /// <param name="mainWndHandle">handle</param>
        /// <param name="deviceGuid">the driver guid not the class guid</param>
        /// <param name="Handle">out puts a handle to the device so that you can unregister it later</param>
        /// <param name="Window">true to register for wndProc or false for service</param>
        /// <returns></returns>
        public static bool RegisterNotify(IntPtr mainWndHandle, Guid deviceGuid, ref IntPtr Handle, bool Window = true)
        {
            IntPtr devBroadcastDeviceInterfaceBuffer = IntPtr.Zero;

            try
            {
                var devBroadcastDeviceInterface = new WinUsbDevice.DEV_BROADCAST_DEVICEINTERFACE();
                int Size = Marshal.SizeOf(devBroadcastDeviceInterface);

                devBroadcastDeviceInterface.dbcc_size = Size;
                devBroadcastDeviceInterface.dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE;
                devBroadcastDeviceInterface.dbcc_reserved = 0;
                devBroadcastDeviceInterface.dbcc_classguid = deviceGuid;

                devBroadcastDeviceInterfaceBuffer = Marshal.AllocHGlobal(Size);
                Marshal.StructureToPtr(devBroadcastDeviceInterface, devBroadcastDeviceInterfaceBuffer, true);

                Handle = RegisterDeviceNotification(mainWndHandle, devBroadcastDeviceInterfaceBuffer, Window ? DEVICE_NOTIFY_WINDOW_HANDLE : DEVICE_NOTIFY_SERVICE_HANDLE);

                Marshal.PtrToStructure(devBroadcastDeviceInterfaceBuffer, devBroadcastDeviceInterface);

                return Handle != IntPtr.Zero;
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} {1}", ex.HelpLink, ex.Message);
                throw;
            }
            finally
            {
                if (devBroadcastDeviceInterfaceBuffer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(devBroadcastDeviceInterfaceBuffer);
                }
            }
        }

        /// <summary>
        /// the 
        /// </summary>
        /// <param name="Handle">The handle to the device , retrieved when you called  RegisterNotify. <see cref="RegisterNotify"/></param>
        /// <returns></returns>
        public static bool UnregisterNotify(IntPtr Handle)
        {
            try
            {
                return UnregisterDeviceNotification(Handle);
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} {1}", ex.HelpLink, ex.Message);
                throw;
            }
        }





        /// <summary>
        /// Handle the WndProc Message for Device Notification, call this inside of WndProc 
        /// and subscribe to this objects Notify Event <see cref="DeviceChanged"/>
        /// </summary>
        /// <param name="msg">the windows message</param>
        public void HandleWndProcMessage(ref Message msg)
        {
            if (msg.Msg == WM_DEVICECHANGE)
            {
                int dType = msg.WParam.ToInt32();

                switch (dType)
                {
                    case DBT_DEVICEARRIVAL:
                    case DBT_DEVICEQUERYREMOVE:
                    case DBT_DEVICEREMOVECOMPLETE:
                        Notified notified = (Notified)dType;

                        var hdr = (WinUsbDevice.DEV_BROADCAST_HDR)Marshal.PtrToStructure(msg.LParam, typeof(WinUsbDevice.DEV_BROADCAST_HDR));

                        if (hdr.dbch_devicetype == DBT_DEVTYP_DEVICEINTERFACE)
                        {
                            var deviceInterface = (WinUsbDevice.DEV_BROADCAST_DEVICEINTERFACE_M)Marshal.PtrToStructure(msg.LParam,
                                typeof(WinUsbDevice.DEV_BROADCAST_DEVICEINTERFACE_M));

                            string guid = new Guid(deviceInterface.dbcc_classguid).ToString().ToUpper();
                            string classguid = "{" + guid + "}";

                            string devicePath = new string(deviceInterface.dbcc_name);
                            devicePath = devicePath.Substring(0, devicePath.IndexOf('\0')).ToUpper();
                            //LogDebug(DateTime.Now, String.Format("Type: {dType} Class: {Class}{Environment.NewLine}Path: {Path}");

                            if (DeviceChanged != null)
                                DeviceChanged(this, new NotifyEventArgs(notified, classguid, devicePath));

                        }
                        break;
                }
            }


        }
        #endregion

        #region WinApi

        public const int WM_DEVICECHANGE = 0x0219;
        public const int DBT_DEVICEARRIVAL = 0x8000;
        public const int DBT_DEVICEQUERYREMOVE = 0x8001;
        public const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
        public const int DBT_DEVTYP_DEVICEINTERFACE = 0x0005;
        public const int DBT_DEVTYP_HANDLE = 0x0006;
        public const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x0000;
        public const int DEVICE_NOTIFY_SERVICE_HANDLE = 0x0001;
        public const int DEVICE_NOTIFY_ALL_INTERFACE_CLASSES = 0x0004;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        protected static extern IntPtr RegisterDeviceNotification(IntPtr hRecipient, IntPtr NotificationFilter, int Flags);

        [DllImport("user32.dll", SetLastError = true)]
        protected static extern bool UnregisterDeviceNotification(IntPtr Handle);


        #endregion
    }
}
