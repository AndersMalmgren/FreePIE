using System;
using System.Collections.Generic;

namespace FreePIE.Core.Plugins.Strategies
{
    public class SetPressedStrategy<TKey>
    {
        private readonly Action<TKey> onPress;
        private readonly Action<TKey> onRelease;
        private readonly List<TKey> press;
        private readonly List<TKey> release;
        private readonly Dictionary<TKey, bool> states;

        public SetPressedStrategy(Action<TKey> onPress, Action<TKey> onRelease)
        {
            this.onPress = onPress;
            this.onRelease = onRelease;
            press = new List<TKey>();
            release = new List<TKey>();
            states = new Dictionary<TKey, bool>();
        }

        public void Do()
        {
            release.ForEach(onRelease);
            release.Clear();

            press.ForEach(Press);
            press.Clear();
        }

        public void Add(TKey code)
        {
            if(!press.Contains(code))
                press.Add(code);
        }

        public void Add(TKey code, bool state)
        {
            if(state && (!states.ContainsKey(code) || !states[code]))
                Add(code);

            states[code] = state;
        }

        private void Press(TKey code)
        {
            onPress(code);
            release.Add(code);
        }

    }

    public class SetPressedStrategy : SetPressedStrategy<int>
    {
        public SetPressedStrategy(Action<int> onPress, Action<int> onRelease) : base(onPress, onRelease){}
    }
}
