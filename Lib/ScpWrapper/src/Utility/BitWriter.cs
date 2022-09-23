using System;

namespace JonesCorp.Utility
{
    public static class BitWriter
    {
        public static int CopyBytes(this byte[] source, int sourceoffset, byte[] dest, int destoffset, int length)
        {
            return CopyToBytes(source, sourceoffset, dest, destoffset, length);
        }

        public static short ReadShort(this byte[] sourceBuffer, int offset)
        {
            return ReadBytesToShort(sourceBuffer, offset);

        }

        public static int ReadInt(this byte[] sourceBuffer, int offset)
        {
            return ReadBytesToInt(sourceBuffer, offset);
        }

        public static long ReadLong(this byte[] sourceBuffer, int offset)
        {
            return ReadBytesToLong(sourceBuffer, offset);
        }

        public static ushort ReadUShort(this byte[] sourceBuffer, int offset)
        {
            return ReadBytesToUShort(sourceBuffer, offset);

        }

        public static uint ReadUInt(this byte[] sourceBuffer, int offset)
        {
            return ReadBytesToUInt(sourceBuffer, offset);
        }

        public static ulong ReadULong(this byte[] sourceBuffer, int offset)
        {
            return ReadBytesToULong(sourceBuffer, offset);
        }

        /*public static T ReadUEnum<T>(this byte[] sourceBuffer, int offset) where T : struct, IConvertible
        {
            return ReadBytesToUEnum<T>(sourceBuffer, offset);
        }*/


        public static void WriteBytes(this int source, byte[] dest, int offset)
        {
            WriteIntToBytes(source, dest, offset);
        }

        public static void WriteBytes(this short source, byte[] dest, int destoffset)
        {
            WriteShortToBytes(source, dest, destoffset);
        }

        public static void WriteBytes(this long source, byte[] dest, int offset)
        {
            WriteLongToBytes(source, dest, offset);
        }

        public static void WriteBytes(this uint source, byte[] dest, int offset)
        {
            WriteUIntToBytes(source, dest, offset);
        }

        public static void WriteBytes(this ushort source, byte[] dest, int destoffset)
        {
            WriteUShortToBytes(source, dest, destoffset);
        }

        public static void WriteBytes(this ulong source, byte[] dest, int offset)
        {
            WriteULongToBytes(source, dest, offset);
        }



        #region Byte Array

        /// <summary>
        /// copy bytes from a value array to a sdestination array
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sourceoffset"></param>
        /// <param name="dest"></param>
        /// <param name="destoffset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static int CopyToBytes(byte[] source, int sourceoffset, byte[] dest, int destoffset, int length)
        {
            int written = 0;

            if ((destoffset + length) > dest.Length)
                throw new Exception("Destination buffer is not long enough");

            for (var i = destoffset; i < (destoffset + length); i++)
            {
                if (sourceoffset < source.Length)
                {
                    dest[i] = source[sourceoffset];
                    sourceoffset++;
                    written++;
                }
                else
                    break;
            }

            return written;
        }



        #endregion

        #region Signed

        #region Short Array



        public static short ReadBytesToShort(byte[] sourceBuffer, int offset)
        {
            short retval = 0;

            retval = (short)ReadBytesToValue(sourceBuffer, offset, sizeof(short));

            return retval;

        }

        public static void WriteShortToBytes(short value, byte[] dest, int destoffset)
        {
            WriteValueToBytes(value, dest, destoffset, sizeof(short));

        }

        #endregion

        #region int

        public static int ReadBytesToInt(byte[] sourceBuffer, int offset)
        {
            int retval = 0;

            retval = (int)ReadBytesToValue(sourceBuffer, offset, sizeof(int));
            return retval;
        }

        /// <summary>
        /// Write 4 bytes to the destination buffer starting at an offset
        /// </summary>
        /// <param name="value"></param>
        /// <param name="dest"></param>
        /// <param name="offset"></param>
        public static void WriteIntToBytes(int value, byte[] dest, int offset)
        {
            WriteValueToBytes(value, dest, offset, sizeof(int));
        }

        #endregion

        #region Long 

        public static long ReadBytesToLong(byte[] sourceBuffer, int offset)
        {
            long retval = (long)ReadBytesToValue(sourceBuffer, offset, sizeof(long));
            return retval;

        }
        public static void WriteLongToBytes(long value, byte[] dest, int destoffset)
        {
            WriteValueToBytes(value, dest, destoffset, sizeof(long));

        }

        #endregion

        /// <summary>
        /// Read specified amount bytes from a buffer as a value type based on length
        /// </summary>
        /// <param name="sourceBuffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private static int ReadBytesToValue(byte[] sourceBuffer, int offset, int length)
        {
            int retval = 0;
            int step = 0;
            for (int i = offset; i < length + offset && i < sourceBuffer.Length; i++)
            {
                retval |= sourceBuffer[i] << step;
                step = step + 8;
            }

            return retval;
        }

        /// <summary>
        /// Write a value to a buffer for a certain length
        /// </summary>
        /// <param name="value"></param>
        /// <param name="dest"></param>
        /// <param name="destoffset"></param>
        /// <param name="length"></param>
        private static void WriteValueToBytes(long value, byte[] dest, int destoffset, int length)
        {
            int step = 0;
            int start = destoffset;
            int end = start + length;
            for (var Index = start; Index < end; Index++)
            {
                if (Index >= dest.Length)
                    break;

                dest[Index] = (byte)((value >> (step * 8)) & 0xFF);
                step++;
            }

        }
        #endregion


        #region Unsigned

        #region UShort

        public static ushort ReadBytesToUShort(byte[] sourceBuffer, int offset)
        {
            ushort retval = (ushort)ReadBytesToUValue(sourceBuffer, offset, sizeof(ushort));

            return retval;
        }

        public static void WriteUShortToBytes(ushort value, byte[] dest, int destoffset)
        {
            WriteUValueToBytes(value, dest, destoffset, sizeof(ushort));

        }

        #endregion



        #region Uint

        public static uint ReadBytesToUInt(byte[] sourceBuffer, int offset)
        {
            uint retval = 0;

            retval = (uint)ReadBytesToValue(sourceBuffer, offset, sizeof(uint));
            return retval;
        }

        /// <summary>
        /// Write 4 bytes to the destination buffer starting at an offset
        /// </summary>
        /// <param name="value"></param>
        /// <param name="dest"></param>
        /// <param name="offset"></param>
        public static void WriteUIntToBytes(uint value, byte[] dest, int offset)
        {
            WriteUValueToBytes(value, dest, offset, sizeof(uint));
        }

        #endregion

        #region Long 

        public static ulong ReadBytesToULong(byte[] sourceBuffer, int offset)
        {
            ulong retval = (ulong)ReadBytesToValue(sourceBuffer, offset, sizeof(ulong));
            return retval;

        }
        public static void WriteULongToBytes(ulong value, byte[] dest, int destoffset)
        {
            WriteUValueToBytes(value, dest, destoffset, sizeof(ulong));

        }

        #endregion

        /// <summary>
        /// Read specified amount bytes from a buffer as a value type based on length
        /// </summary>
        /// <param name="sourceBuffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private static ulong ReadBytesToUValue(byte[] sourceBuffer, int offset, int length)
        {
            ulong retval = 0;
            int step = 0;
            for (int i = offset; i < length + offset && i < sourceBuffer.Length; i++)
            {
                retval |= (ulong)sourceBuffer[i] << step;
                step = step + 8;
            }

            return retval;
        }

        /// <summary>
        /// Write a value to a buffer for a certain length
        /// </summary>
        /// <param name="value"></param>
        /// <param name="dest"></param>
        /// <param name="destoffset"></param>
        /// <param name="length"></param>
        private static void WriteUValueToBytes(ulong value, byte[] dest, int destoffset, int length)
        {
            int step = 0;
            int start = destoffset;
            int end = start + length;
            for (var Index = start; Index < end; Index++)
            {
                if (Index >= dest.Length)
                    break;

                dest[Index] = (byte)((value >> (step * 8)) & 0xFF);
                step++;
            }

        }
        
        #endregion

        #region Generics

        public static T ReadBytesToUEnum<T>(byte[] sourceBuffer, int offset) where T : struct, IConvertible
        {
            T retval = default(T);
            Type t = typeof(T);

            uint v = ReadBytesToUInt(sourceBuffer, offset);

            if (t.IsEnum)
                retval = (T)Enum.ToObject(t, v.ToString());
            return retval;
        }


        public static void WriteBytesFromUEnum<T>(T value, byte[] dest, int offset)
        {
            uint val = 0;
            Type t = typeof(T);

            if (t.IsEnum)
                val = (uint)Convert.ChangeType(value, t);

            WriteUValueToBytes(val, dest, offset, sizeof(uint));
        }

        #endregion
    }
}
