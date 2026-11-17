using System;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.Plugins.Wiimote
{
    [Documentation("Battery and LED status, call request to update it")]
    public class StatusGlobal : Subscribable
    {
        private const int LED1_MASK = 0x10;
        private const int LED2_MASK = 0x20;
        private const int LED3_MASK = 0x40;
        private const int LED4_MASK = 0x80;
        private IWiimoteData data;
        private WiimotePlugin plugin;

        public StatusGlobal(WiimotePlugin plugin, IWiimoteData data, out Action trigger) : base(out trigger)
        {
            this.data = data;
            this.plugin = plugin;
        }

        [Documentation("Battery level in percent")]
        public int batteryPercentage
        {
            get
            {
                return data.BatteryPercentage;
            }
        }
        [Documentation("True if the LED is lit, LEDs are numbered 0 to 3")]
        public bool getLEDState(int led)
        {
            int mask = LED1_MASK;
            if (led == 1) mask = LED2_MASK;
            if (led == 2) mask = LED3_MASK;
            if (led == 3) mask = LED4_MASK;
            return (data.LEDStatus & mask) == mask;
        }

        [Documentation("Turns the LED on or off, LEDs are numbered 0 to 3")]
        public void setLEDState(int led, Boolean state)
        {
            if (getLEDState(led) == state) return;
            int mask = LED1_MASK;
            if (led == 1) mask = LED2_MASK;
            if (led == 2) mask = LED3_MASK;
            if (led == 3) mask = LED4_MASK;
            if (state)
                data.LEDStatus |= mask;
            else
                data.LEDStatus &= ~mask;
            plugin.SetLedState(data.WiimoteNumber, data.LEDStatus);
        }
        

        [Documentation("Requests a status update from the Wiimote")]
        public void request()
        {
            plugin.RequestStatus(data.WiimoteNumber);
        }
    }
}
