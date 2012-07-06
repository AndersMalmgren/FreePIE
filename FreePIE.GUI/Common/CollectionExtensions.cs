using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.GUI.Common
{
    public static class CollectionExtensions
    {
        public static void SyncCollectionTo<T>(this IList<T> collection, IEnumerable<T> other)
        {
            collection.Clear();
            other.ToList().ForEach(collection.Add);
        }
    }
}
