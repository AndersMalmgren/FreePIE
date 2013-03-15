using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FreePIE.Core.Common.Events
{
    public class WeakReferenceList<T> : ICollection<T> where T : class
    {
        private readonly List<WeakReference> list = new List<WeakReference>();

        public IEnumerator<T> GetEnumerator()
        {
            return GetAlive().Select(item => item.Target as T).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            CleanDeadReferences();
            list.Add(new WeakReference(item));
        }

        public void Clear()
        {
            list.Clear();
        }

        public bool Contains(T item)
        {
            return GetAlive().Any(weakItem => weakItem.Target.Equals(item));
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotFiniteNumberException();
        }

        public bool Remove(T item)
        {
            CleanDeadReferences();
            return list.RemoveAll(weakItem => weakItem.Target.Equals(item)) > 0;
        }

        public int Count
        {
            get { return GetAlive().Count(); }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public int IndexOf(T item)
        {
            var weakItem = list.First(x => x.Target == item);
            return list.IndexOf(weakItem);
        }

        public void Insert(int index, T item)
        {
            CleanDeadReferences();
            list.Insert(index, new WeakReference(item));
        }

        private IEnumerable<WeakReference> GetAlive()
        {
            return list.ToList().Where(item => item.IsAlive);
        }

        private void CleanDeadReferences()
        {
            list.RemoveAll(item => !item.IsAlive);
        }
    }
}
