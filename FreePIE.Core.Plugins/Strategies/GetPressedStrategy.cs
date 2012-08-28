using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.Core.Plugins.Strategies
{
    class GetPressedStrategy
    {
        private readonly Predicate<int> isDown;
        private readonly Dictionary<int, bool> pressed;

        public GetPressedStrategy(Predicate<int> isDown)
        {
            this.isDown = isDown;
            pressed = new Dictionary<int, bool>();
        }

        public bool IsPressed(int code)
        {
            bool previouslyPressed = pressed.ContainsKey(code) && pressed[code];
            pressed[code] = isDown(code);

            if (previouslyPressed)
                return false;

            return pressed[code];
        }
    }
}
