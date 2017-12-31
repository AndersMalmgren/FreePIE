using System;
using System.Runtime.InteropServices;

namespace JonesCorp.Utility
{
    //public class XInputDevice
    internal static class Utility
    {
        
        /// <summary>
        /// Convert a struct to byte array
        /// </summary>
        /// <typeparam name="T">a struct</typeparam>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] GetBytes<T>(this T str) where T:struct 
        {
            int size = Marshal.SizeOf(str);
            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(str, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            return arr;
        }

        /// <summary>
        /// Marshal an array of bytes to a Struct
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static T FromBytes<T>(this byte[] arr) where T : struct 
        {
            T str = new T();

            int size = Marshal.SizeOf(str);
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(arr, 0, ptr, size);

            str = (T)Marshal.PtrToStructure(ptr, str.GetType());
            Marshal.FreeHGlobal(ptr);

            return str;
        }

        #region Math

        internal static int Scale(int Value, bool Flip)
        {
            Value -= 0x80;

            if (Value == -128) Value = -127;

            if (Flip) Value *= -1;

            return (int)((float)Value * 258.00787401574803149606299212599f);
        }

        internal static short DirectionToAxis(bool positive, bool negative, bool flip)
        {
            short retval = 0;
            short flipper = (short)(flip ? -1 : 1);

            if (positive && !negative)
            {
                retval = (short)(32767 * flipper);
            }
            else if (!positive && negative)
            {
                retval = (short)(-32767 * flipper);
            }

            return retval;
        }

        internal static bool DeadZone(int R, int X, int Y)
        {
            X -= 0x80; if (X == -128) X = -127;
            Y -= 0x80; if (Y == -128) Y = -127;

            return R * R >= X * X + Y * Y;
        }

        #endregion
    }
}
