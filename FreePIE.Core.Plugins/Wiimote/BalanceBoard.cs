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
            return String.Format("kg: {0}, lb: {1}, raw: {2}, calibration: {3}",kg,lb,raw,calibration);
        }
    }
    public class BalanceBoardSensorCalibration
    {
        public UInt16 kg00 { get; set; }
        public UInt16 kg17 { get; set; }
        public UInt16 kg34 { get; set; }
        public override string ToString()
        {
            return String.Format("0kg: {0}, 17kg: {1}, 34kg: {2}", kg00, kg17, kg34);
        }
    }
    public class BalanceBoardWeight
    {
        public float kg { get; set; }
        public float lb { get; set; }
        public int raw { get; set; }
        public override string ToString()
        {
            return String.Format("kg: {0}, lb: {1}, raw: {2}", kg, lb, raw);
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
            return String.Format("topLeft: {0}, topRight: {1}, bottomLeft: {2}, bottomRight: {3}", topLeft, topRight, bottomLeft, bottomRight);
        }
    }
    public class BalanceBoard
    {
        public BalanceBoardSensorList sensors { get; set; }
        public BalanceBoardWeight weight { get; set; }
        public AnalogStick centerOfGravity { get; set; }
        public override string ToString()
        {
            return String.Format("sensors: {0}, totalWeight: {1}, centerOfGravity: {2}", sensors, weight, centerOfGravity);
        }
    }

}
