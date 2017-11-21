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

    public class GlobalIndexer<T, TIndexOne, TIndexTwo> : GlobalIndexer<T, TIndexOne>
    {
        private readonly Func<TIndexTwo, int, T> _initializerTwo;
        private readonly Dictionary<TIndexTwo, Dictionary<int, T>> globals2;

        public GlobalIndexer(
            Func<TIndexOne, T> initializerOne,
            Func<TIndexTwo, int, T> initializerTwo
            ) : base(initializerOne)
        {
            _initializerTwo = initializerTwo;
            globals2 = new Dictionary<TIndexTwo, Dictionary<int, T>>();
        }


        public T this[TIndexTwo key, int index = 0]
        {
            get
            {
                Dictionary<int, T> theKey;
                T theglobal;
                if (!globals2.TryGetValue(key, out theKey))
                {
                    globals2[key] = theKey = new Dictionary<int, T>();
                }


                if (!theKey.TryGetValue(index, out theglobal))
                {
                    theKey[index] = theglobal = _initializerTwo(key, index);
                }


                return theglobal;
            }
        }
    }
    
}
