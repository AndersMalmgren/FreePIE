using System;
using System.Collections;
using System.Collections.Generic;

namespace FreePIE.GUI.CodeCompletion.Data
{
    public class FixedSizeStack<T> : IEnumerable<T>
    {
        private readonly T[] array;
        private int head;
        private int tail;

        public FixedSizeStack(int size)
        {
            array = new T[size];
            head = 0;
            tail = -1;
        }

        private event EventHandler<EventArgs> Invalidated;
 
        private void OnInvalidated()
        {
            if (Invalidated != null)
                Invalidated(this, EventArgs.Empty);
        }

        private int UpdateIndex()
        {
            int lastIndex = array.Length - 1;

            if (tail == -1)
                return ++tail;

            if(head - tail <= 0 && tail != lastIndex) //we are not full
                return ++tail;

            if(tail == lastIndex)
            {
                head++;
                return tail = 0;
            }
            
            if(head > tail && head != lastIndex)
            {
                head += 1;
                return ++tail;
            }

            if(head == lastIndex)
            {
                head = 0;
                return ++tail;
            }
            
            throw new InvalidOperationException("Should not occur. Bug!");
        }

        public void Push(T value)
        {
            int pos = UpdateIndex();
            array[pos] = value;
            OnInvalidated();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Enumerate().GetEnumerator();
        }

        private List<T> Enumerate()
        {
            List<T> list = new List<T>();

            if (tail == -1)
                return list;

            if(tail == head)
                list.Add(array[0]);

            for (int i = tail; i != head; )
            {
                list.Add(array[i]);

                if (i == 0)
                    i = array.Length - 1;
                else i--;
            }

            return list;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
