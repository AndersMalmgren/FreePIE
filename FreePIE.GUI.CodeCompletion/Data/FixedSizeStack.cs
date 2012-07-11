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
            head = tail = 0;
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
            return new CircularBufferIterator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class CircularBufferIterator : IEnumerator<T>
        {
            private readonly FixedSizeStack<T> buffer;
            private int current;
            private bool invalid;
            private bool started;

            public CircularBufferIterator(FixedSizeStack<T> buffer)
            {
                this.buffer = buffer;
                this.buffer.Invalidated += BackingBufferInvalidated;
                this.current = buffer.tail;
            }

            void BackingBufferInvalidated(object sender, EventArgs e)
            {
                this.invalid = true;
            }

            private void CheckForInvalidated()
            {
                if(invalid)
                    throw new InvalidOperationException("The enumerator has been invalidated by operations on the underlying buffer.");
            }

            public bool MoveNext()
            {
                CheckForInvalidated();

                if (current == buffer.head)
                    return false;

                if (current == 0)
                    current = buffer.array.Length - 1;
                else if(started)
                    current--;
                else
                {
                    started = true;
                }

                return true;
            }

            public void Reset()
            {
                started = false;
                invalid = false;
                this.current = buffer.tail;
            }

            public T Current
            {
                get
                {
                    CheckForInvalidated();
                    return buffer.array[current];
                }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public void Dispose()
            {
                this.buffer.Invalidated -= BackingBufferInvalidated;
            }
        }
    }
}
