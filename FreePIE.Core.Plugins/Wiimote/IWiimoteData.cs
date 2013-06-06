using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.Core.Plugins.Wiimote
{
    public interface IWiimoteData
    {
        byte WiimoteNumber { get; }
        bool IsButtonPressed(WiimoteButtons b);
        Tuple<double, double, double> Acceleration { get; }
        Tuple<double, double, double> MotionPlus { get; }
        EulerAngles MotionPlusEulerAngles { get; }
        bool IsDataValid(WiimoteDataValid valid);
    }
}
