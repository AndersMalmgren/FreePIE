using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.Core.Plugins.Yei3Space
{
    public enum TssStreaming
    {
        // Streaming Commands
        TSS_GET_TARED_ORIENTATION_AS_QUATERNION = 0x00,
        TSS_GET_TARED_ORIENTATION_AS_EULER_ANGLES = 0x01,
        TSS_GET_TARED_ORIENTATION_AS_ROTATION_MATRIX = 0x02,
        TSS_GET_TARED_ORIENTATION_AS_AXIS_ANGLE = 0x03,
        TSS_GET_TARED_ORIENTATION_AS_TWO_VECTOR = 0x04,
        TSS_GET_DIFFERENCE_QUATERNION = 0x05,
        TSS_GET_UNTARED_ORIENTATION_AS_QUATERNION = 0x06,
        TSS_GET_UNTARED_ORIENTATION_AS_EULER_ANGLES = 0x07,
        TSS_GET_UNTARED_ORIENTATION_AS_ROTATION_MATRIX = 0x08,
        TSS_GET_UNTARED_ORIENTATION_AS_AXIS_ANGLE = 0x09,
        TSS_GET_UNTARED_ORIENTATION_AS_TWO_VECTOR = 0x0a,
        TSS_GET_TARED_TWO_VECTOR_IN_SENSOR_FRAME = 0x0b,
        TSS_GET_UNTARED_TWO_VECTOR_IN_SENSOR_FRAME = 0x0c,
        TSS_GET_ALL_NORMALIZED_COMPONENT_SENSOR_DATA = 0x20,
        TSS_GET_NORMALIZED_GYRO_RATE = 0x21,
        TSS_GET_NORMALIZED_ACCELEROMETER_VECTOR = 0x22,
        TSS_GET_NORMALIZED_COMPASS_VECTOR = 0x23,
        TSS_GET_ALL_CORRECTED_COMPONENT_SENSOR_DATA = 0x25,
        TSS_GET_CORRECTED_GYRO_RATE = 0x26,
        TSS_GET_CORRECTED_ACCELEROMETER_VECTOR = 0x27,
        TSS_GET_CORRECTED_COMPASS_VECTOR = 0x28,
        TSS_GET_CORRECTED_LINEAR_ACCELERATION_IN_GLOBAL_SPACE = 0x29,
        TSS_GET_TEMPERATURE_C = 0x2b,
        TSS_GET_TEMPERATURE_F = 0x2c,
        TSS_GET_CONFIDENCE_FACTOR = 0x2d,
        TSS_GET_ALL_RAW_COMPONENT_SENSOR_DATA = 0x40,
        TSS_GET_RAW_GYROSCOPE_RATE = 0x41,
        TSS_GET_RAW_ACCELEROMETER_DATA = 0x42,
        TSS_GET_RAW_COMPASS_DATA = 0x43,
        TSS_GET_BATTERY_VOLTAGE = 0xc9,
        TSS_GET_BATTERY_PERCENT_REMAINING = 0xca,
        TSS_GET_BATTERY_STATUS = 0xcb,
        TSS_GET_BUTTON_STATE = 0xfa,
        TSS_NULL = 0xff
    }
    
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
