using System;
using System.Collections.Generic;

namespace FreePIE.Core.Plugins.Strategies
{
    class GetReleasedStrategy<T>
    {
        private readonly Predicate<T> isDown;
        private readonly Dictionary<T, bool> released;

        public GetReleasedStrategy(Predicate<T> isDown)
        {
            this.isDown = isDown;
            released = new Dictionary<T, bool>();
        }

        public bool IsReleased(T code)
        {
            bool previouslyReleased; 

            if (!released.ContainsKey(code))
            {
                released[code] = true;
                return false;
            }else previouslyReleased = released[code];

            released[code] = !isDown(code);
            if (previouslyReleased)
                return false;

            return released[code];
        }
    }
}
