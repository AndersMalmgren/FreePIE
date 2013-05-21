using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.Core.Plugins.Yei3Space
{
    public enum TssError
    {
        TSS_NO_ERROR,                   // The API call successfuly executed
        TSS_INVALID_COMMAND,            // The API call was made on a device type that does not suppport the attemped command
        TSS_INVALID_ID,                 // The TSS_ID parameter passed in to an API call is not associated with a connected 3-Space device
        TSS_INVALID_IDX,                // The index passed into a Wireless Sensor API is out of range 0-14
        TSS_WARNING_OLD_FIRMWARE,       // Function can complete but is emulated, upgrading is highly recomended
        TSS_ERROR_OLD_FIRMWARE,         // Function is not available on this firmware, update it
        TSS_ERROR_USB_CONNECTION,       // When creating a 3-Space device that is connected over USB there was an issue communicating with the intended device
        TSS_ERROR_WIR_CONNECTION,       // When creating a 3-Space device that is communicating through a 3-Space Dongle there was an issue communicating with the intended device
        TSS_ERROR_WRITE,                // The API call executed failed to write all the data necisary to execute the command to the intended serial port
        TSS_ERROR_READ,                 // The API call executed failed to read all the data necisary to execute the command to the intended serial port
        TSS_ERROR_NO_SENSOR_FOUND,      // No sensor was found when looking for sensors
        TSS_ERROR_STREAM_SLOTS_FULL,    // The sensor's stream slots are full
        TSS_ERROR_STREAM_CONFIG,        // The sensor's stream configuration is corrupted
        TSS_ERROR_INCORRECT_SIZE,       // The size is incorrect
        TSS_ERROR_TIMEOUT,              // The command's timeout has been reached
        TSS_ERROR_MEMORY                // A memory error occured in the API
    }

    [Flags]
    public enum TssFind
    {
        TSS_FIND_BTL = 0x00000001,
        TSS_FIND_USB = 0x00000002,
        TSS_FIND_DNG = 0x00000004,
        TSS_FIND_WL = 0x00000008,
        TSS_FIND_EM = 0x00000010,
        TSS_FIND_DL = 0x00000020,
        TSS_FIND_BT = 0x00000040,
        TSS_FIND_UNKNOWN = unchecked((int)0x80000000),
        TSS_FIND_ALL_KNOWN = 0x7fffffff,
        TSS_FIND_ALL = unchecked((int)0xffffffff)
    }

    public enum TssDeviceIdMask
    {
        TSS_NO_DEVICE_ID = 0x00800000,
        TSS_BOOTLOADER_ID = 0x01000000,
        TSS_DONGLE_ID = 0x02000000,
        TSS_USB_ID = 0x04000000,
        TSS_EMBEDDED_ID = 0x08000000,
        TSS_WIRELESS_ID = 0x10000000,
        TSS_WIRELESS_W_ID = 0x20000000,
        TSS_DATALOGGER_ID = 0x40000000,
        TSS_BLUETOOTH_ID = unchecked((int)0x80000000),
        TSS_NO_DONGLE_ID = unchecked((int)0xfd000000),
        TSS_ALL_SENSORS_ID = unchecked((int)0xff000000)
    }

    public enum TssTimestampMode
    {
        TSS_TIMESTAMP_NONE,
        TSS_TIMESTAMP_SENSOR,
        TSS_TIMESTAMP_SYSTEM
    }
}
