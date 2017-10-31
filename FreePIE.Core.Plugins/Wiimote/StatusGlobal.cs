using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.Core.Plugins.Wiimote
{
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

        public int batteryPercentage
        {
            get
            {
                return data.BatteryPercentage;
            }
        }
        public bool getLEDState(int led)
        {
            int mask = LED1_MASK;
            if (led == 2) mask = LED2_MASK;
            if (led == 3) mask = LED3_MASK;
            if (led == 4) mask = LED4_MASK;
            return (data.LEDStatus & mask) == data.LEDStatus;
        }

        public void setLEDState(int led, Boolean state)
        {
            int mask = LED1_MASK;
            if (led == 2) mask = LED2_MASK;
            if (led == 3) mask = LED3_MASK;
            if (led == 4) mask = LED4_MASK;
            if (state)
                data.LEDStatus |= mask;
            else
                data.LEDStatus &= ~mask;
            plugin.SetLedState(data.WiimoteNumber, data.LEDStatus);
        }
        

        public void request()
        {
            plugin.RequestStatus(data.WiimoteNumber);
        }
    }
}
