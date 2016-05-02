using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.Plugins.PSMove
{
    [GlobalEnum]
    public enum MoveButton
    {
        T1 = 0x1,
        T2 = 0x4,
        TRIANGLE = 0x10,
        CIRCLE = 0x20,
        CROSS = 0x40,
        SQUARE = 0x80,
        SELECT = 0x100,
        START = 0x800,
        STICK = 0x200,
        UP = 0x1000,
        RIGHT = 0x2000,
        DOWN = 0x4000,
        LEFT = 0x8000,
        PS = 0x10000,
        MOVE = 0x80000,
        T = 0x100000
    };

    public static class Api
    {
        public delegate void MoveUpdateCallback(int id, [MarshalAs(UnmanagedType.Struct)] Vector3 position, [MarshalAs(UnmanagedType.Struct)] Quaternion orientation, int trigger);
        public delegate void NavUpdateCallback(int id, int trigger1, int trigger2, int stickX, int stickY);
        public delegate void MoveKeyCallback(int id, int keyCode);
        private static MoveUpdateCallback updateCallback;
        private static MoveKeyCallback keyDownCallback;
        private static MoveKeyCallback keyUpCallback;
        private static NavUpdateCallback navUpdateCallback;
        private static MoveKeyCallback navKeyDownCallback;
        private static MoveKeyCallback navKeyUpCallback;

        [StructLayout(LayoutKind.Sequential)]
        public struct Vector3
        {
            public float x;
            public float y;
            public float z;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Quaternion
        {
            public float w;
            public float x;
            public float y;
            public float z;
        }

        [DllImport("MF_CWrapper.dll")]
        public static extern void init();
        [DllImport("MF_CWrapper.dll")]
        public static extern int getMovesCount();
        [DllImport("MF_CWrapper.dll")]
        public static extern int getNavsCount();
        [DllImport("MF_CWrapper.dll")]
        public static extern void unsubscribeMove();
        [DllImport("MF_CWrapper.dll")]
        private static extern void subscribeMove(MoveUpdateCallback updateCallback, MoveKeyCallback keyDownCallback, MoveKeyCallback keyUpCallback, NavUpdateCallback navCallback, MoveKeyCallback navKeyDown, MoveKeyCallback navKeyUp);
        [DllImport("MF_CWrapper.dll")]
        [return: MarshalAs(UnmanagedType.Struct)]
        public static extern Quaternion getOrientation(int id);
        [DllImport("MF_CWrapper.dll")]
        [return: MarshalAs(UnmanagedType.Struct)]
        public static extern Vector3 getPosition(int id);
        [DllImport("MF_CWrapper.dll")]
        public static extern bool getButtonState(int id, [MarshalAs(UnmanagedType.I4)] MoveButton keyId);
        [DllImport("MF_CWrapper.dll")]
        public static extern int getTriggerValue(int id);
        [DllImport("MF_CWrapper.dll")]
        public static extern void setRumble(int id, int value);
        [DllImport("MF_CWrapper.dll")]
        public static extern int getNavTrigger1(int id);
        [DllImport("MF_CWrapper.dll")]
        public static extern int getNavTrigger2(int id);
        [DllImport("MF_CWrapper.dll")]
        public static extern int getNavStickX(int id);
        [DllImport("MF_CWrapper.dll")]
        public static extern int getNavStickY(int id);

        public static void SubscribeMoveUpdate(MoveUpdateCallback updateCallback, MoveKeyCallback keyDownCallback, MoveKeyCallback keyUpCallback, NavUpdateCallback navCallback, MoveKeyCallback navKeyDown, MoveKeyCallback navKeyUp)
        {
            Api.updateCallback = updateCallback;
            Api.keyDownCallback = keyDownCallback;
            Api.keyUpCallback = keyUpCallback;
            Api.navUpdateCallback = navCallback;
            Api.navKeyDownCallback = navKeyDown;
            Api.navKeyUpCallback = navKeyUp;

            subscribeMove(updateCallback, keyDownCallback, keyUpCallback, navCallback, navKeyDown, navKeyUp);
        }
    }
}
