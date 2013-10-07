using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace FreePIE.Core.Plugins.Hydra
{
    public static class Sixense
    {
        public struct ControllerData
        {
            public float pos_x;
            public float pos_y;
            public float pos_z;
            public float rot_mat_00;
            public float rot_mat_01;
            public float rot_mat_02;
            public float rot_mat_10;
            public float rot_mat_11;
            public float rot_mat_12;
            public float rot_mat_20;
            public float rot_mat_21;
            public float rot_mat_22;
            public float joystick_x;
            public float joystick_y;
            public float trigger;
            public uint buttons;
            public byte sequence_number;
            public float rot_quat0;
            public float rot_quat1;
            public float rot_quat2;
            public float rot_quat3;
            public ushort firmware_revision;
            public ushort hardware_revision;
            public ushort packet_type;
            public ushort magnetic_frequency;
            public int enabled;
            public int controller_index;
            public byte is_docked;
            public byte which_hand;
            public byte hemi_tracking_enabled;
        }

        public struct ControllerAngles
        {
            public float yaw;
            public float pitch;
            public float roll;
        }

        public const int SUCCESS = 0;
        public const int FAILURE = 1;

        public const int BUTTON_BUMPER = (0x01 << 7);
        public const int BUTTON_JOYSTICK = (0x01 << 8);
        public const int BUTTON_1 = (0x01 << 5);
        public const int BUTTON_2 = (0x01 << 6);
        public const int BUTTON_3 = (0x01 << 3);
        public const int BUTTON_4 = (0x01 << 4);
        public const int BUTTON_START = (0x01 << 0);


        [DllImport("sixense.dll", SetLastError = false, EntryPoint = "sixenseInit",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int Init();

        [DllImport("sixense.dll", SetLastError = false, EntryPoint = "sixenseExit",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int Exit();

        [DllImport("sixense.dll", SetLastError = false, EntryPoint = "sixenseIsBaseConnected",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int IsBaseConnected(int base_num);

        [DllImport("sixense.dll", SetLastError = false, EntryPoint = "sixenseSetActiveBase",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetActiveBase(int base_num);

        [DllImport("sixense.dll", SetLastError = false, EntryPoint = "sixenseGetNewestData",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetNewestData(int which, out ControllerData data);
    }
}
