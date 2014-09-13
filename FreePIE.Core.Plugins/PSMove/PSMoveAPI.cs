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

        [DllImport("libpsmoveapi_tracker", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr psmove_tracker_new();

        [DllImport("libpsmoveapi_tracker", CallingConvention = CallingConvention.Cdecl)]
        public static extern void psmove_tracker_free(IntPtr tracker);

        [DllImport("libpsmoveapi_tracker", CallingConvention = CallingConvention.Cdecl)]
        public static extern void psmove_tracker_set_mirror(IntPtr tracker, int enabled);

        [DllImport("libpsmoveapi_tracker", CallingConvention = CallingConvention.Cdecl)]
        public static extern PSMoveTracker_Status psmove_tracker_enable(IntPtr tracker, IntPtr move);

        [DllImport("libpsmoveapi_tracker", CallingConvention = CallingConvention.Cdecl)]
        public static extern void psmove_tracker_update_image(IntPtr tracker);

        [DllImport("libpsmoveapi_tracker", CallingConvention = CallingConvention.Cdecl)]
        public static extern int psmove_tracker_update(IntPtr tracker, IntPtr move);

        [DllImport("libpsmoveapi_tracker", CallingConvention = CallingConvention.Cdecl)]
        public static extern int psmove_tracker_get_auto_update_leds(IntPtr tracker, IntPtr move);

        [DllImport("libpsmoveapi_tracker", CallingConvention = CallingConvention.Cdecl)]
        public static extern void psmove_tracker_set_auto_update_leds(IntPtr tracker, IntPtr move, int update);

        [DllImport("libpsmoveapi_tracker", CallingConvention = CallingConvention.Cdecl)]
        public static extern int psmove_tracker_get_color(IntPtr tracker, IntPtr move, ref char r, ref char g, ref char b);

        [DllImport("libpsmoveapi_tracker", CallingConvention = CallingConvention.Cdecl)]
        public static extern void psmove_tracker_set_dimming(IntPtr tracker, float dimming);

        [DllImport("libpsmoveapi_tracker", CallingConvention = CallingConvention.Cdecl)]
        public static extern int psmove_tracker_set_camera_color(IntPtr tracker, IntPtr move, char r, char g, char b);

        /* 
         * Optional code and not required by default (see auto_update_leds above)
         * 
        if ( autoupdateleds ) {
            unsigned char r, g, b; 
            psmove_tracker_get_color(tracker, controllers[i], &r, &g, &b);
            psmove_set_leds(controllers[i], r, g, b);
            psmove_update_leds(controllers[i]);
        }
        */

        #endregion

        #region Data fusion

        [DllImport("libpsmoveapi_tracker", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr psmove_fusion_new(IntPtr tracker, float z_near, float z_far);

        [DllImport("libpsmoveapi_tracker", CallingConvention = CallingConvention.Cdecl)]
        public static extern void psmove_fusion_free(IntPtr fusion);

        [DllImport("libpsmoveapi_tracker", CallingConvention = CallingConvention.Cdecl)]
        public static extern void psmove_fusion_get_position(IntPtr fusion, IntPtr move, ref float x, ref float y, ref float z);

        #endregion

    }

}
