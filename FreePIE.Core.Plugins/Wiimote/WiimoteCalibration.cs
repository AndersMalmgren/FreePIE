using System;
using System.Collections.Generic;

namespace FreePIE.Core.Plugins.Wiimote
{
    public class CalibratedValue<T>
    {
        public CalibratedValue(bool didCalibrate, T value)
        {
            DidCalibrate = didCalibrate;
            Value = value;
        }

        public T Value { get; private set; }

        public bool DidCalibrate { get; private set; }
    }

    public class WiimoteCalibration
    {
        private readonly TimeSeries accelerationMagnitudes;
        private readonly TimeSeries nunchuckAccelerationMagnitudes;
        private readonly TimeSeries nunchuckStick;

        private const uint WiimoteStationaryDeltaEpsilon = 3;

        public WiimoteCalibration()
        {
            accelerationMagnitudes = new TimeSeries(1024);
            nunchuckAccelerationMagnitudes = new TimeSeries(1024);
            nunchuckStick = new TimeSeries(256);
        }

        private double EuclideanDistance(ushort a, ushort b, ushort c)
        {
            return Math.Sqrt(a*a + b*b + c*c);
        }

        private double EuclideanDistance(byte a, byte b)
        {
            return Math.Sqrt(a * a + b * b);
        }

        private bool IsStationary()
        {
            return accelerationMagnitudes.Size > 10 && accelerationMagnitudes.DurationStable(WiimoteStationaryDeltaEpsilon) > TimeSpan.FromMilliseconds(500);
        }

        private void TakeAccelerationCalibrationSnapshot(ushort accX, ushort accY, ushort accZ)
        {
            var offset = (accX + accY) / 2d;
            var gravity = accZ - offset;
            
            Acceleration = new LinearCalibration(9.81 / gravity, offset);
        }

        private static double TransformLinear(LinearCalibration calibration, double value)
        {
            if (calibration == null)
                return 0;

            return (value - calibration.Offset) * calibration.Gain;
        }

        private bool AccelerationCalibrated { get { return Acceleration != null; } }

        private bool MotionPlusCalibrated { get { return MotionPlus != null; } }

        public CalibratedValue<Acceleration> NormalizeAcceleration(DateTime measured, ushort x, ushort y, ushort z)
        {
            accelerationMagnitudes.Add(measured, EuclideanDistance(x, y, z));

            bool calibrated = false;

            if (IsStationary() && !AccelerationCalibrated)
            {
                TakeAccelerationCalibrationSnapshot(x, y, z);
                calibrated = true;
            }

            return new CalibratedValue<Acceleration>(calibrated, new Acceleration(TransformLinear(Acceleration, x),
                                                                                  TransformLinear(Acceleration, y),
                                                                                  TransformLinear(Acceleration, z)));
        }

        public CalibratedValue<Gyro> NormalizeMotionplus(DateTime measured, ushort yaw, ushort pitch, ushort roll)
        {
            bool calibrated = false;

            if (IsStationary() && !MotionPlusCalibrated && MotionPlusInsidePermissibleRange(yaw, pitch, roll))
            {
                TakeMotionPlusCalibrationSnapshot(yaw, pitch, roll);
                calibrated = true;
            }

            return  MotionPlusCalibrated ? new CalibratedValue<Gyro>(calibrated, new Gyro(TransformLinear(MotionPlus.X, yaw),
                                                                                          TransformLinear(MotionPlus.Y, pitch),
                                                                                          TransformLinear(MotionPlus.Z, roll)))
                                         : new CalibratedValue<Gyro>(false, new Gyro(0, 0, 0));
        }

        private bool ValueInsideRange(int value, int min, int max)
        {
            return min < value && value < max;
        }

        private bool MotionPlusInsidePermissibleRange(ushort yaw, ushort pitch, ushort roll)
        {
            const int max = 8000 + 1000;
            const int min = 8000 - 1000;

            return ValueInsideRange(yaw, min, max) && ValueInsideRange(pitch, min, max) && ValueInsideRange(roll, min, max);
        }

        private void TakeMotionPlusCalibrationSnapshot(ushort yaw, ushort pitch, ushort roll)
        {
            // const double gain = 1.0 / (8192.0 / 595.0) // this is gain according to wiibrew?
            const double gain = 1.0 / 20.0; // this is gain according to wiic

            MotionPlus = new ThreePointCalibration(new LinearCalibration(gain, yaw),
                                                   new LinearCalibration(gain, pitch),
                                                   new LinearCalibration(gain, roll));
        }

        private ThreePointCalibration MotionPlus { get; set; }
        private LinearCalibration Acceleration { get; set; }
        private TwoPointCalibration NunchuckStick { get; set; }
        private LinearCalibration NunchuckAcceleration { set; get; }

        private class ThreePointCalibration
        {
            public LinearCalibration X { get; private set; }
            public LinearCalibration Y { get; private set; }
            public LinearCalibration Z { get; private set; }

            public ThreePointCalibration(LinearCalibration x, LinearCalibration y, LinearCalibration z)
            {
                X = x;
                Y = y;
                Z = z;
            }
        }

        private class TwoPointCalibration
        {
            public LinearCalibration X { get; private set; }
            public LinearCalibration Y { get; private set; }

            public TwoPointCalibration(LinearCalibration x, LinearCalibration y)
            {
                X = x;
                Y = y;
            }
        }

        private class LinearCalibration
        {
            public LinearCalibration(double gain, double offset)
            {
                Gain = gain;
                Offset = offset;
            }

            public double Gain { get; private set; }

            public double Offset { get; private set; }
        }

        private bool IsNunchuckStickStationary()
        {
            return nunchuckStick.Size > 10 && nunchuckStick.DurationStable(WiimoteStationaryDeltaEpsilon) > TimeSpan.FromMilliseconds(250);
        }

        public NunchuckStick NormalizeNunchuckStick(DateTime measured, byte stickX, byte stickY)
        {
            nunchuckStick.Add(measured, EuclideanDistance(stickX, stickY));

            if (IsNunchuckStickStationary() && !NunchuckStickCalibrated)
                TakeNunchuckStickCalibrationSnapshot(stickX, stickY);

            return NunchuckStickCalibrated ? new NunchuckStick(TransformLinear(NunchuckStick.X, stickX), TransformLinear(NunchuckStick.Y, stickY)) : new NunchuckStick(0, 0);
        }

        public Acceleration NormalizeNunchuckAcceleration(DateTime measured, ushort x, ushort y, ushort z)
        {
            nunchuckAccelerationMagnitudes.Add(measured, EuclideanDistance(x, y, z));

            if (IsNunchuckStationary() && !NunchuckAccelerationCalibrated)
                TakeNunchuckAccelerationCalibrationSnapshot(x, y, z);

            return new Acceleration(TransformLinear(NunchuckAcceleration, x),
                                    TransformLinear(NunchuckAcceleration, y),
                                    TransformLinear(NunchuckAcceleration, z));
        }

        private void TakeNunchuckAccelerationCalibrationSnapshot(ushort x, ushort y, ushort z)
        {
            var offset = (x + y) / 2d;
            var gravity = z - offset;

            NunchuckAcceleration = new LinearCalibration(9.81 / gravity, offset);
        }

        protected bool NunchuckAccelerationCalibrated
        {
            get { return NunchuckAcceleration != null; }
        }

        private bool IsNunchuckStationary()
        {
            return nunchuckAccelerationMagnitudes.Size > 10 && nunchuckAccelerationMagnitudes.DurationStable(WiimoteStationaryDeltaEpsilon) > TimeSpan.FromMilliseconds(1000);
        }

        private bool NunchuckStickCalibrated
        {
            get { return NunchuckStick != null; }
        }

        private void TakeNunchuckStickCalibrationSnapshot(byte stickX, byte stickY)
        {
            NunchuckStick = new TwoPointCalibration(new LinearCalibration(1, stickX), new LinearCalibration(1, stickY));
        }
    }
}
