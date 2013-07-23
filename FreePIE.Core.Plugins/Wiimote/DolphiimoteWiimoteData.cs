using System;

namespace FreePIE.Core.Plugins.Wiimote
{
    public class DolphiimoteWiimoteData : IWiimoteData
    {
        private DolphiimoteData data;
        private WiimoteCalibration calibration;
        private uint wiimoteNumber;
        private IMotionPlusFuser fuser;

        public byte WiimoteNumber { get; private set; }

        public Gyro MotionPlus { get; private set; }

        public EulerAngles MotionPlusEulerAngles
        {
            get
            {
                return fuser.FusedValues;
            }
        }

        public Nunchuck Nunchuck { get; private set; }

        public Acceleration Acceleration { get; private set; }

        public DolphiimoteWiimoteData(byte wiimoteNumber, WiimoteCalibration calibration)
        {
            this.fuser = new SimpleIntegrationMotionPlusFuser();
            this.WiimoteNumber = wiimoteNumber;
            this.calibration = calibration;
        }

        public bool IsButtonPressed(WiimoteButtons b)
        {
            UInt16 value = (UInt16)b;
            return (data.button_state & value) == value;
        }

        private static double TransformLinear(double gain, double offset, double value)
        {
            return (value + offset) * gain;
        }

        public bool IsDataValid(WiimoteDataValid valid)
        {
            UInt32 value = (UInt32)valid;
            return (data.valid_data_flags & value) == value;
        }

        public bool IsNunchuckButtonPressed(NunchuckButtons nunchuckButtons)
        {
            UInt16 value = (UInt16)nunchuckButtons;
            return (data.nunchuck.buttons & value) == value;
        }

        private Gyro CalculateMotionPlus(DolphiimoteMotionplus motionplus)
        {
            return new Gyro(TransformLinear((motionplus.slow_modes & 0x1) == 0x1 ? calibration.MotionPlusGainSlow : calibration.MotionPlusGainFast, calibration.MotionPlusOffset, data.motionplus.yaw_down_speed),
                            TransformLinear((motionplus.slow_modes & 0x4) == 0x4 ? calibration.MotionPlusGainSlow : calibration.MotionPlusGainFast, calibration.MotionPlusOffset, data.motionplus.pitch_left_speed),
                            TransformLinear((motionplus.slow_modes & 0x2) == 0x2 ? calibration.MotionPlusGainSlow : calibration.MotionPlusGainFast, calibration.MotionPlusOffset, data.motionplus.roll_left_speed));
        }

        public void Update(DolphiimoteData rawData)
        {
            this.data = rawData;

            Acceleration = calibration.NormalizeAcceleration(DateTime.Now, rawData.acceleration.x, rawData.acceleration.y, rawData.acceleration.z);

            if (IsDataValid(WiimoteDataValid.MotionPlus))
            {
                MotionPlus = CalculateMotionPlus(rawData.motionplus);
                fuser.HandleIMUData(MotionPlus.x, MotionPlus.y, MotionPlus.z, Acceleration.x, Acceleration.y, Acceleration.z);
            }

            if (IsDataValid(WiimoteDataValid.Nunchuck))
            {
                Nunchuck = new Nunchuck
                    {
                        Acceleration = new Acceleration(rawData.nunchuck.x, rawData.nunchuck.y, rawData.nunchuck.z),
                        Stick = new NunchuckStick(rawData.nunchuck.stick_x, rawData.nunchuck.stick_y)
                    };
            }
        }
    }
}