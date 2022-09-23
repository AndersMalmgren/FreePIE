using JonesCorp.Data;

namespace JonesCorp
{
    public interface IWinUsbDevice
    {
        ConnectionState CurrentState { get; }
        string Path { get; }

        /// <summary>
        /// Open a handle to the device
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        bool Open(int instance = 0);

        /// <summary>
        /// if state is connected unplugs any devices and sets state to reserved
        /// </summary>
        /// <returns></returns>
        bool Close();

        /// <summary>
        /// Return false if device is not active
        /// </summary>
        /// <returns></returns>
        bool Start();

        bool Stop();
    }
}