using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using JonesCorp.Data;
//using log4net;

namespace JonesCorp
{
    
    public abstract partial class WinUsbDevice: IDisposable, IWinUsbDevice
    {
        //private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IntPtr _fileHandle = IntPtr.Zero;

        private IntPtr _winUsbHandle = (IntPtr)INVALID_HANDLE_VALUE;


        private readonly Guid _class;
        
        public ConnectionState CurrentState { get; set; } = ConnectionState.Disconnected;

        public string Path { get; private set; } = string.Empty;

        
        
        public WinUsbDevice(string deviceClass)
        {
            _class = new Guid(deviceClass);
        }

        /// <summary>
        /// Handle opening the device
        /// </summary>
        /// <returns>return true if your opening operations are succesful, 
        /// or false if they were not. Returning true will result in the 
        /// device being set to the Opened state</returns>
        protected abstract bool OnOpen(int instance);

        /// <summary>
        /// Open a handle to the device
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public bool Open(int instance = 0)
        {
            string devicePath = string.Empty;
            


            if (CurrentState == ConnectionState.Disconnected && OnOpen(instance))
            {
                Debug.WriteLine($"Looking for DevicePath for DeviceClass: {_class}\r\nInstance: {instance}");
                if (Find(_class, ref devicePath, instance))
                {
                    Debug.WriteLine($"Found device path {devicePath}");
                    Open(devicePath);
                }
                else
                {
                    Debug.WriteLine($"Could not find DevicePath for DeviceClass: {_class}\r\nInstance: {instance}");
                }
            }

            return CurrentState == ConnectionState.Opened;
        }

        /// <summary>
        /// set the device path and retrieve a file handle to the scp driver
        /// and set isActive to true if successful
        /// </summary>
        /// <param name="devicePath">the device path of the scp driver</param>
        /// <returns></returns>
        protected bool Open(string devicePath)
        {
            if (CurrentState == ConnectionState.Disconnected)
            {
                Debug.WriteLine($"Geting device handle for devicepath -- {devicePath} ...");
                
                if (GetDeviceHandle(devicePath, ref _fileHandle))
                {
                    this.Path = devicePath;
                    CurrentState = ConnectionState.Opened;
                    
                    if (!WinUsbWrapper.WinUsb_Initialize(_fileHandle, ref _winUsbHandle))
                        Debug.WriteLine($"WinUsbWrapper.WinUsb_Initialize({_fileHandle}, ref {_winUsbHandle}); -FAILED..no matter");
                }
                else
                {
                    Debug.WriteLine($"Could not get device handle for devicepath -- {devicePath}");
                }
            }

            return CurrentState == ConnectionState.Opened;
        }

        /// <summary>
        /// Handle closing the device
        /// </summary>
        /// <returns>return true if your close operations are succesful, 
        /// or false if they were not. Returning true will result in the 
        /// device being set to the stopped state</returns>
        protected abstract bool OnClose();

        /// <summary>
        /// if state is connected unplugs any devices and sets state to reserved
        /// </summary>
        /// <returns></returns>
        public bool Close()
        {
            if (CurrentState == ConnectionState.Started)
            {
                Debug.WriteLine("Attempted to Close a device that has not been stopped first . \r\nState is now {CurrentState} .\r\n Will try to stop...");

                if (!Stop())
                {
                    Debug.WriteLine($"Device was running during Close() . Attempt to stop was unsuccessful. State is now {CurrentState}");

                    return false;
                }
                else
                {
                    Debug.WriteLine($"Stop during close was successful. State is now {CurrentState}");
                }
            }

            Debug.WriteLine($"Closing Device .... State is now {CurrentState}");
            if (CurrentState == ConnectionState.Opened && OnClose())
            {
                var retval = true;

                if (_winUsbHandle != (IntPtr) INVALID_HANDLE_VALUE)
                {
                    Debug.WriteLine(
                        $"_winUsbHandle is valid. Calling WinUsb_Free({_winUsbHandle} .... State is now {CurrentState}");

                    if (WinUsbWrapper.WinUsb_Free(_winUsbHandle))
                    {
                        Debug.WriteLine($"WinUsb_Free() was SUCCESSFUL .... State is now {CurrentState}");

                    }
                    else
                    {
                        retval = false;
                        Debug.WriteLine($"call to WinUsb_Free() FAILED.. State is now {CurrentState}");
                    }
                }
                else
                {
                    retval = false;
                    Debug.WriteLine(
                        $"_winUsbHandle was not valid. Did not call WinUsb_Free({_winUsbHandle} .... State is now {CurrentState}");

                }
                if (retval)
                    CurrentState = ConnectionState.Disconnected;
            }
            else
            {
                Debug.WriteLine($"Closing Device Failed. State is now {CurrentState}");
               
            }

            

            return CurrentState == ConnectionState.Disconnected;
        }


        /// <summary>
        /// Handle starting the device
        /// </summary>
        /// <returns>return true if your start operations are succesful, 
        /// or false if they were not. Returning true will result in the 
        /// device being set to the connected state</returns>
        protected virtual bool OnStart()
        {
            return true;
        }

        

        /// <summary>
        /// Return false if device is not active
        /// </summary>
        /// <returns></returns>
        public bool Start()
        {
            if (CurrentState == ConnectionState.Opened && OnStart())
                CurrentState = ConnectionState.Started;

            return CurrentState == ConnectionState.Started;
        }

        /// <summary>
        /// Handle stopping the device
        /// </summary>
        /// <returns>return true if your stop operations are succesful, 
        /// or false if they were not. Returning true will result in the 
        /// device being set to the opened state</returns>
        protected abstract bool OnStop();

        public bool Stop()
        {
            if(CurrentState == ConnectionState.Started && OnStop())
                CurrentState = ConnectionState.Opened;

            return CurrentState == ConnectionState.Opened;
        }

        /// <summary>
        /// <see cref="DeviceIoControl(IOCTL_BUSENUM,  byte[], byte[],  ref int,  out int)"/>
        /// </summary>
        /// <param name="code"></param>
        /// <param name="inputbuffer"></param>
        /// <param name="bytesTransferred"></param>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        public bool DeviceIoControl(IOCTL_BUSENUM code,byte[] inputbuffer,  ref int bytesTransferred, out int errorCode)
        {
            return DeviceIoControl(code, inputbuffer, null, ref bytesTransferred, out errorCode);
        }


        /// <summary>
        /// Send IOCTL code, and data to driver
        /// </summary>
        /// <param name="code"></param>
        /// <param name="inputbuffer"></param>
        /// <param name="outputBuffer"></param>
        /// <param name="bytesTransferred"></param>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        public bool DeviceIoControl(IOCTL_BUSENUM code, byte[] inputbuffer,byte[] outputBuffer, ref int bytesTransferred, out int errorCode)
        {
            errorCode = 0;

            int len = (outputBuffer != null) ? outputBuffer.Length : 0;
            bool retval = DeviceIoControl(_fileHandle, code, inputbuffer, inputbuffer.Length, outputBuffer, len, ref bytesTransferred, IntPtr.Zero);
            if (!retval)
            {
                errorCode = Marshal.GetLastWin32Error();
                Debug.WriteLine($"DeviceIOControl ErrorCode : {errorCode}");
            }
            return (errorCode == 0);
        }
        

        private static bool GetDeviceHandle(string path, ref IntPtr fileHandle)
        {
            fileHandle = CreateFile(path, (GENERIC_WRITE | GENERIC_READ), FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL | FILE_FLAG_OVERLAPPED, 0);

            if (fileHandle == IntPtr.Zero || fileHandle == (IntPtr)INVALID_HANDLE_VALUE)
            {
                fileHandle = IntPtr.Zero;

                var lastError = Marshal.GetLastWin32Error();

                var exc = new Win32Exception(lastError);
                Debug.WriteLine("error: {0} - {1}", lastError, exc.Message);
                //throw exc;
            }

            return !(fileHandle == IntPtr.Zero);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="path"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        private static bool Find(Guid target, ref string path, int instance = 0)
        {
            IntPtr detailDataBuffer = IntPtr.Zero;
            IntPtr deviceInfoSet = IntPtr.Zero;
            Debug.WriteLine($"Find target :{target.ToString()}\r\n int instance = {instance})");

            try
            {
                SP_DEVICE_INTERFACE_DATA deviceInterfaceData = new SP_DEVICE_INTERFACE_DATA(), da = new SP_DEVICE_INTERFACE_DATA();
                int bufferSize = 0, memberIndex = 0;

                deviceInfoSet = SetupDiGetClassDevs(ref target, IntPtr.Zero, IntPtr.Zero, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);

                deviceInterfaceData.cbSize = da.cbSize = Marshal.SizeOf(deviceInterfaceData);

                while (SetupDiEnumDeviceInterfaces(deviceInfoSet, IntPtr.Zero, ref target, memberIndex, ref deviceInterfaceData))
                {
                    
                    SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref deviceInterfaceData, IntPtr.Zero, 0, ref bufferSize, ref da);
                    {
                        detailDataBuffer = Marshal.AllocHGlobal(bufferSize);

                        Marshal.WriteInt32(detailDataBuffer, (IntPtr.Size == 4) ? (4 + Marshal.SystemDefaultCharSize) : 8);

                        if (SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref deviceInterfaceData, detailDataBuffer, bufferSize, ref bufferSize, ref da))
                        {
                            IntPtr pdevicePathName = detailDataBuffer + 4;

                            string ptrToStringAuto = Marshal.PtrToStringAuto(pdevicePathName);
                            if (ptrToStringAuto != null)
                                path = ptrToStringAuto.ToUpper();

                            Marshal.FreeHGlobal(detailDataBuffer);

                            if (memberIndex == instance)
                            {
                                Debug.WriteLine($"Found {path}");
                                return true;
                            }
                        }
                        else Marshal.FreeHGlobal(detailDataBuffer);
                    }

                    memberIndex++;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex.HelpLink} {ex.Message}");
                throw;
            }
            finally
            {
                if (deviceInfoSet != IntPtr.Zero)
                {
                    SetupDiDestroyDeviceInfoList(deviceInfoSet);
                }
            }

            return false;
        }
        /*

        private bool UsbEndpointDirectionIn(int addr)
        {
            return (addr & 0x80) == 0x80;
        }

        private bool UsbEndpointDirectionOut(int addr)
        {
            return (addr & 0x80) == 0x00;
        }
        */

        public void Dispose()
        {
            Debug.WriteLine($"Disposing WinUsbDevice current state is {CurrentState}...");
            bool s = Stop();

            bool c = Close();

        }

    }
}
