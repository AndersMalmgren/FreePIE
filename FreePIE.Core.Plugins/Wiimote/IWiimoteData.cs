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
        CalibratedValue<Acceleration> Acceleration { get; }
        CalibratedValue<Gyro> MotionPlus { get; }
        EulerAngles MotionPlusEulerAngles { get; }
        Nunchuck Nunchuck { get; }
        ClassicController ClassicController { get; }
        Guitar Guitar { get; }
        BalanceBoard BalanceBoard { get; }
        bool IsDataValid(WiimoteDataValid valid);
        bool IsNunchuckButtonPressed(NunchuckButtons nunchuckButtons);
        bool IsClassicControllerButtonPressed(ClassicControllerButtons classicControllerButtons);
        bool IsGuitarButtonPressed(GuitarButtons guitarButtons);
    }

    public class Gyro
    {
        public Gyro(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public double x { get; private set; }
        public double y { get; private set; }
        public double z { get; private set; }
    }
}
