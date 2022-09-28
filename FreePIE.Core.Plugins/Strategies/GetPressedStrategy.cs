using System;
using System.Collections.Generic;

namespace FreePIE.Core.Plugins.Strategies
{
    class GetPressedStrategy<T>
    {
        private readonly Predicate<T> isDown;
        private readonly Dictionary<T, bool> pressed;

        public GetPressedStrategy(Predicate<T> isDown)
        {
            this.isDown = isDown;
            pressed = new Dictionary<T, bool>();
        }

        public bool IsPressed(T code)
        {
            bool previouslyPressed = pressed.ContainsKey(code) && pressed[code];
            pressed[code] = isDown(code); 

            return !previouslyPressed && pressed[code];    // Pressed = false before and true now
        }
    }
}

