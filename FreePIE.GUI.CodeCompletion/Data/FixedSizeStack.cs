using System;
using System.Collections;
using System.Collections.Generic;

namespace FreePIE.GUI.CodeCompletion.Data
{
    public class FixedSizeStack<T> : IEnumerable<T>
    {
        private readonly T[] array;

        private int head;
        private int size;


        public FixedSizeStack(int size)
        {
            array = new T[size];
        }

        public void Push(T val)
        {
            size = Math.Min(array.Length, size + 1);

            head++;

            head = head % size;

            array[head] = val;
        }


        public IEnumerator<T> GetEnumerator()
        {
            List<T> list = new List<T>();

            for (int i = 0, j = head + 1; i < size; i++, j++)
                list.Add(array[j % size]);

            list.Reverse();

            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
