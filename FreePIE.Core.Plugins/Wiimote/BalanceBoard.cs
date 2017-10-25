using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreePIE.Core.Plugins.Wiimote
{
    public class BalanceBoardSensorRaw
    {

        public BalanceBoardSensorRaw(DolphiimoteBalanceBoardSensorRaw sensor)
        {
            this.topLeft = sensor.top_left;
            this.topRight = sensor.top_right;
            this.bottomLeft = sensor.bottom_left;
            this.bottomRight = sensor.bottom_right;
        }

        public BalanceBoardSensorRaw(int topLeft, int topRight, int bottomLeft, int bottomRight)
        {
            this.topLeft = topLeft;
            this.topRight = topRight;
            this.bottomLeft = bottomLeft;
            this.bottomRight = bottomRight;
        }
        public int topLeft { get; set; }
        public int topRight { get; set; }
        public int bottomLeft { get; set; }
        public int bottomRight { get; set; }
    }
    public class BalanceBoardSensor
    {
        public BalanceBoardSensor(DolphiimoteBalanceBoardSensor sensor)
        {
            this.topLeft = sensor.top_left;
            this.topRight = sensor.top_right;
            this.bottomLeft = sensor.bottom_left;
            this.bottomRight = sensor.bottom_right;
        }
        public BalanceBoardSensor(float topLeft, float topRight, float bottomLeft, float bottomRight)
        {
            this.topLeft = topLeft;
            this.topRight = topRight;
            this.bottomLeft = bottomLeft;
            this.bottomRight = bottomRight;
        }
        public float topLeft { get; set; }
        public float topRight { get; set; }
        public float bottomLeft { get; set; }
        public float bottomRight { get; set; }
    }
    public class BalanceBoard
    {
        public BalanceBoardSensorRaw Raw { get; set; }
        public BalanceBoardSensorRaw KG0Calibration { get; set; }
        public BalanceBoardSensorRaw KG17Calibration { get; set; }
        public BalanceBoardSensorRaw KG34Calibration { get; set; }
        public BalanceBoardSensor KG { get; set; }
        public BalanceBoardSensor LB { get; set; }
        public float KGWeight { get; set; }
        public float LBWeight { get; set; }
        public AnalogStick CenterOfGravity { get; set; }
    }
}
