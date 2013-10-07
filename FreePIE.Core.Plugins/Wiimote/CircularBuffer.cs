using System;
using System.Collections;
using System.Collections.Generic;

namespace FreePIE.Core.Plugins.Wiimote
{
    public class CircularBuffer<T> : IEnumerable<T>
    {
        private readonly T[] data;
        private int front;

        public int Size { get; private set; }

        public CircularBuffer(uint capacity)
        {
            data = new T[capacity];
            Size = 0;
            front = -1;
        }

        public void Push(T element)
        {
            front += 1;
            front = front % data.Length;
            data[front] = element;

            Size = Math.Min(data.Length, Size + 1);
        }

        private static uint Modulo(int i, int c)
        {
            var result = i % c;
            return (uint)(result >= 0 ? result : result + c);
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int c = 0, i = front; c < Size; c++, i--)
                yield return data[Modulo(i, Size)];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}