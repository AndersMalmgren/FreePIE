using System;
using System.Runtime.InteropServices;

namespace FreePIE.Core.Plugins.PSMove
{

    public class PSMoveAPI
    {

        #region Connection and pair

        [DllImport("libpsmoveapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern int psmove_count_connected();

        [DllImport("libpsmoveapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr psmove_connect();

        [DllImport("libpsmoveapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr psmove_connect_by_id(int id);

        [DllImport("libpsmoveapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern int psmove_pair(IntPtr move);

        [DllImport("libpsmoveapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern PSMoveConnectionType psmove_connection_type(IntPtr move);

        [DllImport("libpsmoveapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern void psmove_disconnect(IntPtr move);

        [DllImport("libpsmoveapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern void psmove_reinit();

        #endregion

        #region LED & Rumble

        [DllImport("libpsmoveapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern void psmove_set_rate_limiting(IntPtr move, int enabled);

        [DllImport("libpsmoveapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern void psmove_set_leds(IntPtr move, char r, char g, char b);
        
        [DllImport("libpsmoveapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern void psmove_set_rumble(IntPtr move, char rumble);

        [DllImport("libpsmoveapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern int psmove_update_leds(IntPtr move);

        #endregion

        #region Poll and buttons

        [DllImport("libpsmoveapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint psmove_poll(IntPtr move);

        [DllImport("libpsmoveapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint psmove_get_buttons(IntPtr move);

        [DllImport("libpsmoveapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint psmove_get_button_events(IntPtr move, ref uint pressed, ref uint released);

        [DllImport("libpsmoveapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern char psmove_get_trigger(IntPtr move);

        #endregion

        #region Battery and temps

        [DllImport("libpsmoveapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern float psmove_get_temperature(IntPtr move);

        [DllImport("libpsmoveapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern PSMove_Battery_Level psmove_get_battery(IntPtr move);

        #endregion

        #region Orientation IMU

        [DllImport("libpsmoveapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern int psmove_has_calibration(IntPtr move);

        [DllImport("libpsmoveapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern void psmove_dump_calibration(IntPtr move);

        [DllImport("libpsmoveapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern void psmove_get_accelerometer(IntPtr move, ref int ax, ref int ay, ref int az);
        [DllImport("libpsmoveapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern void psmove_get_accelerometer_frame(IntPtr move, PSMove_Frame frame, ref float ax, ref float ay, ref float az);

        [DllImport("libpsmoveapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern void psmove_get_gyroscope(IntPtr move, ref int gx, ref int gy, ref int gz);
        [DllImport("libpsmoveapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern void psmove_get_gyroscope_frame(IntPtr move, PSMove_Frame frame, ref float gx, ref float gy, ref float gz);

        [DllImport("libpsmoveapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern void psmove_get_magnetometer(IntPtr move, ref int mx, ref int my, ref int mz);

        [DllImport("libpsmoveapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern int psmove_has_orientation(IntPtr move);


        [DllImport("libpsmoveapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern void psmove_enable_orientation(IntPtr move, int enabled);

        [DllImport("libpsmoveapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern void psmove_reset_orientation(IntPtr move);

        [DllImport("libpsmoveapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern void psmove_get_orientation(IntPtr move, ref float q0, ref float q1, ref float q2, ref float q3);

        #endregion

        #region Misc & Utility

        [DllImport("libpsmoveapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern string psmove_get_serial(IntPtr move);

        //[DllImport("libpsmoveapi", CallingConvention = CallingConvention.Cdecl)]
        //public static extern int psmove_pair_custom(IntPtr move, string btaddr_string);

        [DllImport("libpsmoveapi", CallingConvention = CallingConvention.Cdecl)]
        public static extern long psmove_util_get_ticks();

        //[DllImport("libpsmoveapi", CallingConvention = CallingConvention.Cdecl)]
        //public static extern IntPtr psmove_util_get_file_path(IntPtr filename);

        #endregion

        #region Camera Tracker

        // TODO: add camera tracker hooks

        #endregion

    }

}