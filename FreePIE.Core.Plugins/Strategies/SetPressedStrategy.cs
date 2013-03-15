using System;
using System.Collections.Generic;

namespace FreePIE.Core.Plugins.Strategies
{
    public class SetPressedStrategy
    {
        private readonly Action<int> onPress;
        private readonly Action<int> onRelease;
        private readonly List<int> press;
        private readonly List<int> release;

        public SetPressedStrategy(Action<int> onPress, Action<int> onRelease)
        {
            this.onPress = onPress;
            this.onRelease = onRelease;
            press = new List<int>();
            release = new List<int>();
        }

        public void Do()
        {
            release.ForEach(onRelease);
            release.Clear();

            press.ForEach(Press);
            press.Clear();
        }

        public void Add(int code)
        {
            if(!press.Contains(code))
                press.Add(code);
        }

        private void Press(int code)
        {
            onPress(code);
            release.Add(code);
        }

    }
}
