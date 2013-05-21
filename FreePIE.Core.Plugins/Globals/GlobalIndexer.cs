using System;
using System.Collections.Generic;

namespace FreePIE.Core.Plugins.Globals
{
    public class GlobalIndexer<T>
    {
        private readonly Func<int, T> initilizer;
        private readonly Dictionary<int, T> globals;

        public GlobalIndexer(Func<int, T> initilizer)
        {
            this.initilizer = initilizer;
            globals = new Dictionary<int, T>();
        }

        public T this[int index]
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
