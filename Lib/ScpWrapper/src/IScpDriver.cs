using JonesCorp.Data;

namespace JonesCorp
{
    public interface IScpDriver: IWinUsbDevice
    {

        
        string Path { get; }

        bool PauseToggle();

        bool Resume();

        bool Suspend();

        bool Open(int instance = 0);

        //bool Open(string devicePath);

        /// <summary>
        /// Performs a stop and changes the status to Disconnected, 
        /// where as calling stop directly would set the state to reserved
        /// </summary>
        /// <returns></returns>
        bool Close();

        

        bool Stop();


        /// <summary>
        /// Plug the virtual controller In
        /// </summary>
        /// <param name="playerNumber"></param>
        /// <returns></returns>
        bool Plugin(int playerNumber);

        /// <summary>
        /// Unplug the virtual controller In
        /// </summary>
        /// <param name="playerNumber">the id of the controller int from 1 - 4</param>
        /// <returns></returns>
        bool Unplug(int playerNumber);

        /// <summary>
        /// Send the xbox report to the device driver. see offset enum for data locations <see cref="SCP_REPORT.DsOffset"/>
        /// </summary>
        /// <param name="xinput">28 xbox report</param>
        /// <param name="rumbleOutput">2 byte output buffer to fill with rumble data</param>
        /// <returns>0 if no error </returns>
        //public int Report(byte[] xinput, byte[] rumbleOutput)
        int Report(SCP_XINPUT_DATA xinput, byte[] rumbleOutput);

        /// <summary>
        /// <see cref="WinUsbDevice.DeviceIoControl(WinUsbDevice.IOCTL_BUSENUM,byte[],byte[],ref int,out int)"/>
        /// </summary>
        /// <param name="code"></param>
        /// <param name="inputbuffer"></param>
        /// <param name="bytesTransferred"></param>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        bool DeviceIoControl(WinUsbDevice.IOCTL_BUSENUM code,byte[] inputbuffer,  ref int bytesTransferred, out int errorCode);

        /// <summary>
        /// Send IOCTL code, and data to driver
        /// </summary>
        /// <param name="code"></param>
        /// <param name="inputbuffer"></param>
        /// <param name="outputBuffer"></param>
        /// <param name="bytesTransferred"></param>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        bool DeviceIoControl(WinUsbDevice.IOCTL_BUSENUM code, byte[] inputbuffer,byte[] outputBuffer, ref int bytesTransferred, out int errorCode);

        void Dispose();
    }
}