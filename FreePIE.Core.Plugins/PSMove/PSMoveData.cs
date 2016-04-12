using System;
using FreePIE.Core.Contracts;
using System.Runtime.InteropServices;

namespace FreePIE.Core.Plugins.PSMove
{

    #region Enums&Structs: Basic PSMove

    public enum PSMoveConnectionType
    {
        Bluetooth,
        USB,
        Unknown,
    };

    [GlobalEnum]
    public enum PSMoveButton
    {
        L2 = 1 << 0x00,
        R2 = 1 << 0x01,
        L1 = 1 << 0x02,
        R1 = 1 << 0x03,
        Triangle = 1 << 0x04,
        Circle = 1 << 0x05,
        Cross = 1 << 0x06,
        Square = 1 << 0x07,
        Select = 1 << 0x08,
        L3 = 1 << 0x09, // 512
        R3 = 1 << 0x0A,
        Start = 1 << 0x0B,
        Up = 1 << 0x0C,
        Right = 1 << 0x0D,
        Down = 1 << 0x0E,
        Left = 1 << 0x0F,
        PS = 1 << 0x10,
        Move = 1 << 0x13,
        Trigger = 1 << 0x14
    };

    public enum PSMove_Battery_Level
    {
        Batt_MIN = 0x00, /*!< Battery is almost empty (< 20%) */
        Batt_20Percent = 0x01, /*!< Battery has at least 20% remaining */
        Batt_40Percent = 0x02, /*!< Battery has at least 40% remaining */
        Batt_60Percent = 0x03, /*!< Battery has at least 60% remaining */
        Batt_80Percent = 0x04, /*!< Battery has at least 80% remaining */
        Batt_MAX = 0x05, /*!< Battery is fully charged (not on charger) */
        Batt_CHARGING = 0xEE, /*!< Battery is currently being charged */
        Batt_CHARGING_DONE = 0xEF, /*!< Battery is fully charged (on charger) */
    };

    public enum PSMove_Frame
    {
        Frame_FirstHalf = 0, /*!< The older frame */
        Frame_SecondHalf, /*!< The most recent frame */
    };

    #endregion

    #region Enums&Structs: Tracker

    public struct PSMoveTrackerRGBImage
    {
        IntPtr data;
        int width;
        int height;
    };

    public enum PSMoveTracker_Status
    {
        Tracker_NOT_CALIBRATED, /*!< Controller not registered with tracker */
        Tracker_CALIBRATION_ERROR, /*!< Calibration failed (check lighting, visibility) */
        Tracker_CALIBRATED, /*!< Color calibration successful, not currently tracking */
        Tracker_TRACKING, /*!< Calibrated and successfully tracked in the camera */
    };

    public enum PSMoveTracker_Exposure
    {
        Exposure_LOW, /*!< Very low exposure: Good tracking, no environment visible */
        Exposure_MEDIUM, /*!< Middle ground: Good tracking, environment visibile */
        Exposure_HIGH, /*!< High exposure: Fair tracking, but good environment */
        Exposure_INVALID, /*!< Invalid exposure value (for returning failures) */
    };

    #endregion

    #region Custom Added Types for FreePIE

    public class Vector3
    {
        public double x { get; set; }
        public double y { get; set; }
        public double z { get; set; }
        public double magnitude { get { return Math.Sqrt((x*x)+(y*y)+(z*z)); } }

        public Vector3()
        {
            this.x = this.y = this.z = 0;
        }

        public Vector3(double x, double y, double z)
        {
            Update(x, y, z);
        }

        public Vector3(Vector3 v)
        {
            Update(v.x, v.y, v.z);
        }
        
        internal void Update(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Vector3 operator +(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
        }

        public static Vector3 operator -(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        }
    }

    public class RGB_Color
    {
        internal char r, g, b;

        public RGB_Color()
        {
            SetColor(0, 0, 0);
        }

        public RGB_Color(int r, int g, int b)
        {
            SetColor(r, g, b);
        }

        public void SetColor(int r, int g, int b)
        {
            R = r;
            G = g;
            B = b;
        }

        public int R { get { return (int)r; } set { this.r = ClamptoChar(value); } }
        public int G { get { return (int)g; } set { this.g = ClamptoChar(value); } }
        public int B { get { return (int)b; } set { this.b = ClamptoChar(value); } }
        
        static internal char ClamptoChar(int x)
        {
            if (x < 0) {
                x = 0;
            } else if (x > 255) {
                x = 255;
            }
            return (char)x;
        }
    }

    #endregion
}
