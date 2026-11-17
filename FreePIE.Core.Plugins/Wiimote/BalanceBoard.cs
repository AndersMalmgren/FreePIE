using System;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.Plugins.Wiimote
{
    [Documentation("A Balance Board weight sensor")]
    public class BalanceBoardSensor
    {
        [Documentation("Weight on the sensor in kilograms")]
        public float kg { get; set; }
        [Documentation("Weight on the sensor in pounds")]
        public float lb { get; set; }
        [Documentation("Raw sensor value")]
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
    [Documentation("Total weight on the Balance Board")]
    public class BalanceBoardWeight
    {
        [Documentation("Total weight in kilograms")]
        public float kg { get; set; }
        [Documentation("Total weight in pounds")]
        public float lb { get; set; }
        [Documentation("Raw sensor value")]
        public int raw { get; set; }
        public override string ToString()
        {
            return String.Format("kg: {0}, lb: {1}, raw: {2}", kg, lb, raw);
        }
    }
    [Documentation("The four Balance Board weight sensors")]
    public class BalanceBoardSensorList
    {
        [Documentation("The top left sensor")]
        public BalanceBoardSensor topLeft { get; set; }
        [Documentation("The top right sensor")]
        public BalanceBoardSensor topRight { get; set; }
        [Documentation("The bottom left sensor")]
        public BalanceBoardSensor bottomLeft { get; set; }
        [Documentation("The bottom right sensor")]
        public BalanceBoardSensor bottomRight { get; set; }
        public override string ToString()
        {
            return String.Format("topLeft: {0},\ntopRight: {1},\nbottomLeft: {2},\nbottomRight: {3}", topLeft, topRight, bottomLeft, bottomRight);
        }
    }
    public class BalanceBoard
    {
        public BalanceBoardSensorList sensors { get; set; }
        public BalanceBoardWeight weight { get; set; }
        public AnalogStick centerOfGravity { get; set; }
        public override string ToString()
        {
            return String.Format("weight: {1},\ncenterOfGravity: {2},\n{0}", sensors, weight, centerOfGravity);
        }
    }

}
