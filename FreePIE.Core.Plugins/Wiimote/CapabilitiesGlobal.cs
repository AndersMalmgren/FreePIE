using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.Core.Plugins.Wiimote
{
    public class CapabilitiesGlobal : Subscribable
    {
        private IWiimoteData data;

        public CapabilitiesGlobal(IWiimoteData data, out Action trigger) : base(out trigger)
        {
            this.data = data;
        }
        
        public Boolean has_enabled_capability(WiimoteCapabilities capability)
        {
            return (data.EnabledCapabilities & capability) == capability;
        }
        public Boolean has_available_capability(WiimoteCapabilities capability)
        {
            return (data.AvailableCapabilities & capability) == capability;
        }
        public Boolean has_extension(WiimoteExtensions extension)
        {
            return data.ExtensionType == extension;
        }
        public override string ToString()
        {
            String ret = "Available Capabilities: ";
            if (has_available_capability(WiimoteCapabilities.Extension))
            {
                ret += "Extension ";
            }
            if (has_available_capability(WiimoteCapabilities.IR))
            {
                ret += "IR ";
            }
            if (has_available_capability(WiimoteCapabilities.MotionPlus))
            {
                ret += "MotionPlus ";
            }
            ret += "\nEnabled Capabilities: ";
            if (has_enabled_capability(WiimoteCapabilities.Extension))
            {
                ret += "Extension ";
            }
            if (has_enabled_capability(WiimoteCapabilities.IR))
            {
                ret += "IR ";
            }
            if (has_enabled_capability(WiimoteCapabilities.MotionPlus))
            {
                ret += "MotionPlus ";
            }

            if (has_available_capability(WiimoteCapabilities.Extension))
            {
                ret += "\nConnected Extension: "+data.ExtensionType;
                ret += " ID: " + data.ExtensionID.ToString("x12").ToUpper();
            }
            return ret;
        }
    }
}
