using System;
using System.Collections.Generic;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.Plugins.Strategies
{
    public class GetPressedStrategy<T>
    {
        private Predicate<T> isDown;
        private readonly Dictionary<T, bool> pressed;
	    private readonly Dictionary<T, bool> currentState;

        public GetPressedStrategy(IScriptContext context)
        {
	        context.BeforeScriptExecuting += ResetState;
            pressed = new Dictionary<T, bool>();
			currentState = new Dictionary<T, bool>();
        }

	    public GetPressedStrategy<T> Init(Predicate<T> isDown)
	    {
			if(this.isDown != null)
				throw new Exception("GetPressedStrategy.Init can only be called once ");

			this.isDown = isDown;
		    return this;
	    }

		private void ResetState(object sender, EventArgs e)
		{
			currentState.Clear();
		}

        public bool IsPressed(T code)
        {
	        if (currentState.ContainsKey(code)) return currentState[code];

			return currentState[code] = GetState(code);
        }

	    private bool GetState(T code)
	    {
			bool previouslyPressed = pressed.ContainsKey(code) && pressed[code];
			pressed[code] = isDown(code);

			if(previouslyPressed)
				return false;

			return pressed[code];
	    }
    }
}
