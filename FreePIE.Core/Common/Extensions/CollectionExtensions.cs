using System;
using System.Collections.Generic;
using System.Linq;

namespace FreePIE.Core.Common.Extensions
{
    public static class CollectionExtensions
    {
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            if(collection != null)
            {
                foreach(var item in collection)
                {
                    action(item);
                }
            }

            return collection;
        }

        public static void SyncCollectionTo<T>(this IList<T> collection, IEnumerable<T> other)
        {
            collection.Clear();
            other.ToList().ForEach(collection.Add);
        }

        public static IEnumerable<T> TakeAllButLast<T>(this IEnumerable<T> source)
        {
            var it = source.GetEnumerator();
            bool hasRemainingItems = false;
            bool isFirst = true;
            T item = default(T);

            do
            {
                hasRemainingItems = it.MoveNext();
                if (hasRemainingItems)
                {
                    if (!isFirst) yield return item;
                    item = it.Current;
                    isFirst = false;
                }
            } while (hasRemainingItems);
        }
    }
}
