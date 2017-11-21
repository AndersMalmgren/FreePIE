using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreePIE.Core.Plugins.Strategies;

namespace FreePIE.Core.Plugins.Wiimote
{
    public class WiimoteButtonState : Subscribable
    {
        private IWiimoteData data;
        private GetPressedStrategy<WiimoteButtons> buttonPressed;

        public WiimoteButtonState(IWiimoteData data, out Action trigger) : base(out trigger)
        {
            this.data = data;
            buttonPressed = new GetPressedStrategy<WiimoteButtons>(button_down);
        }

        public bool button_down(WiimoteButtons b)
        {
            return data.IsButtonPressed(b);
        }

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

    public class NunchuckButtonState
    {
        private IWiimoteData data;

        public NunchuckButtonState(IWiimoteData data)
        {
            this.data = data;
        }

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

    public class GuitarButtonState
    {
        private IWiimoteData data;

        public GuitarButtonState(IWiimoteData data)
        {
            this.data = data;
        }

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

    public class ClassicControllerButtonState
    {
        private IWiimoteData data;

        public ClassicControllerButtonState(IWiimoteData data)
        {
            this.data = data;
        }

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
