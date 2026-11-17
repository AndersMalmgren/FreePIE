using System;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.Strategies;

namespace FreePIE.Core.Plugins.Wiimote
{
    [Documentation("The Wiimote buttons")]
    public class WiimoteButtonState : Subscribable
    {
        private IWiimoteData data;
        private GetPressedStrategy<WiimoteButtons> buttonPressed;

        public WiimoteButtonState(IWiimoteData data, out Action trigger) : base(out trigger)
        {
            this.data = data;
            buttonPressed = new GetPressedStrategy<WiimoteButtons>(button_down);
        }

        [Documentation("True while the button is down")]
        public bool button_down(WiimoteButtons b)
        {
            return data.IsButtonPressed(b);
        }

        [Documentation("True once when the button goes down, false until it has been released and pressed again")]
        public bool button_pressed(WiimoteButtons b)
        {
            return buttonPressed.IsPressed(b);
        }
        public override string ToString()
        {
            String ret = "";
            foreach (WiimoteButtons b in Enum.GetValues(typeof(WiimoteButtons)))
            {
                if (button_down(b))
                {
                    ret += b + " ";
                }
            }
            return ret;
        }
    }

    [Documentation("The Nunchuck buttons")]
    public class NunchuckButtonState
    {
        private IWiimoteData data;

        public NunchuckButtonState(IWiimoteData data)
        {
            this.data = data;
        }

        [Documentation("True while the button is down")]
        public bool button_down(NunchuckButtons b)
        {
            return data.IsNunchuckButtonPressed(b);
        }
        public override string ToString()
        {
            String ret = "";
            foreach (NunchuckButtons b in Enum.GetValues(typeof(NunchuckButtons)))
            {
                if (button_down(b))
                {
                    ret += b + " ";
                }
            }
            return ret;
        }
    }

    [Documentation("The guitar buttons")]
    public class GuitarButtonState
    {
        private IWiimoteData data;

        public GuitarButtonState(IWiimoteData data)
        {
            this.data = data;
        }

        [Documentation("True while the button is down")]
        public bool button_down(GuitarButtons b)
        {
            return data.IsGuitarButtonPressed(b);
        }

        public override string ToString()
        {
            String ret = "";
            foreach (GuitarButtons b in Enum.GetValues(typeof(GuitarButtons)))
            {
                if (button_down(b))
                {
                    ret += b + " ";
                }
            }
            return ret;
        }
    }

    [Documentation("The Classic Controller buttons")]
    public class ClassicControllerButtonState
    {
        private IWiimoteData data;

        public ClassicControllerButtonState(IWiimoteData data)
        {
            this.data = data;
        }

        [Documentation("True while the button is down")]
        public bool button_down(ClassicControllerButtons b)
        {
            return data.IsClassicControllerButtonPressed(b);
        }

        public override string ToString()
        {
            String ret = "";
            foreach (ClassicControllerButtons b in Enum.GetValues(typeof(ClassicControllerButtons)))
            {
                if (button_down(b))
                {
                    ret += b + " ";
                }
            }
            return ret;
        }
    }
}
