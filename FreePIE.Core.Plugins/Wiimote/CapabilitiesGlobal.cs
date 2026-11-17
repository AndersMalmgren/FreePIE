using System;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.Plugins.Wiimote
{
    [Documentation("The available and enabled Wiimote features and the connected extension")]
    public class CapabilitiesGlobal : Subscribable
    {
        private IWiimoteData data;

        public CapabilitiesGlobal(IWiimoteData data, out Action trigger) : base(out trigger)
        {
            this.data = data;
        }
        
        [Documentation("True if the feature has been enabled")]
        public Boolean has_enabled_capability(WiimoteCapabilities capability)
        {
            return (data.EnabledCapabilities & capability) == capability;
        }
        [Documentation("True if the Wiimote supports the feature")]
        public Boolean has_available_capability(WiimoteCapabilities capability)
        {
            return (data.AvailableCapabilities & capability) == capability;
        }
        [Documentation("True if the extension is connected")]
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
