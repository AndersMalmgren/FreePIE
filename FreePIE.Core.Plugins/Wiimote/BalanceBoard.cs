using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreePIE.Core.Plugins.Wiimote
{
    public class BalanceBoardSensor
    {
        public float kg { get; set; }
        public float lb { get; set; }
        public UInt16 raw { get; set; }
        public BalanceBoardSensorCalibration calibration { get; set; }
        public override string ToString()
        {
            return String.Format("KG: {0}, LB: {1}, Raw: {2}, Calibration: {3}",kg,lb,raw,calibration);
        }
    }
    public class BalanceBoardSensorCalibration
    {
        public UInt16 kg00 { get; set; }
        public UInt16 kg17 { get; set; }
        public UInt16 kg34 { get; set; }
        public override string ToString()
        {
            return String.Format("0KG: {0}, 17KG: {1}, 34KG: {2}", kg00, kg17, kg34);
        }
    }
    public class BalanceBoardWeight
    {
        public float kg { get; set; }
        public float lb { get; set; }
        public int raw { get; set; }
        public override string ToString()
        {
            return String.Format("KG: {0}, LB: {1}, Raw: {2}", kg, lb, raw);
        }
    }
    public class BalanceBoardSensorList
    {
        public BalanceBoardSensor topLeft { get; set; }
        public BalanceBoardSensor topRight { get; set; }
        public BalanceBoardSensor bottomLeft { get; set; }
        public BalanceBoardSensor bottomRight { get; set; }
        public override string ToString()
        {
            return String.Format("Top Left: {0}, Top Right: {1}, Bottom Left: {2}, Bottom Right: {3}", topLeft, topRight, bottomLeft, bottomRight);
        }
    }
    public class BalanceBoard
    {
        public BalanceBoardSensorList sensors { get; set; }
        public BalanceBoardWeight weight { get; set; }
        public AnalogStick centerOfGravity { get; set; }
        public override string ToString()
        {
            return String.Format("Sensors: {0}, Total Weight: {1}, Center Of Gravity: {2}", sensors, weight, centerOfGravity);
        }
    }

}
