using System;
using System.Collections.Generic;

namespace FreePIE.Core.Plugins.Globals
{
    public class GlobalIndexer<T> : GlobalIndexer<T, int>
    {
        public GlobalIndexer(Func<int, T> initilizer) : base(initilizer)
        {
        }
    }

    public class GlobalIndexer<T, TIndex>
    {
        private readonly Func<TIndex, T> initilizer;
        private readonly Dictionary<TIndex, T> globals;

        public GlobalIndexer(Func<TIndex, T> initilizer)
        {
            this.initilizer = initilizer;
            globals = new Dictionary<TIndex, T>();
        }

        public T this[TIndex index]
        {
            get
            {
                if (!globals.ContainsKey(index))
                {
                    globals[index] = initilizer(index);
                }

                return globals[index];
            }
        } 
    }
}
