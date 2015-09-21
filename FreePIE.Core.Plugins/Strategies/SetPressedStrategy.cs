using System;
using System.Collections.Generic;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.Plugins.Strategies
{
    public class SetPressedStrategy
    {
        private Action<int> onPress;
        private Action<int> onRelease;
        private readonly List<int> press;
        private readonly List<int> release;

        public SetPressedStrategy(IScriptContext context)
        {
            press = new List<int>();
            release = new List<int>();
            context.BeforeScriptExecuting += Execute;
        }

        public SetPressedStrategy Init(Action<int> onPress, Action<int> onRelease)
        {
            if(this.onPress != null)
                throw new Exception("SetPressedStrategy.Init can only be called once ");

            this.onPress = onPress;
            this.onRelease = onRelease;

            return this;
        }

        private void Execute(object sender, EventArgs e)
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
