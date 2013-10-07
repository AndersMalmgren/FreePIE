using System;

namespace FreePIE.Core.Plugins.Wiimote
{
    public class DolphiimoteWiimoteData : IWiimoteData
    {
        private readonly WiimoteCalibration calibration;
        private readonly IMotionPlusFuser fuser;
        private DolphiimoteData data;

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
            fuser = new SimpleIntegrationMotionPlusFuser();
            WiimoteNumber = wiimoteNumber;
            this.calibration = calibration;

            MotionPlus = new Gyro(0, 0, 0);
            Acceleration = new Acceleration(0, 0, 0);
            Nunchuck = new Nunchuck
            {
                Acceleration = new Acceleration(0, 0, 0),
                Stick = new NunchuckStick(0, 0)
            };
        }

        public bool IsButtonPressed(WiimoteButtons b)
        {
            UInt16 value = (UInt16)b;
            return (data.button_state & value) == value;
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
            //const double fastModeFactor = 2000.0 / 440.0; //According to wiibrew
            const double fastModeFactor = 20.0 / 4.0; //According to wiic
            var gyro = calibration.NormalizeMotionplus(DateTime.Now, motionplus.yaw_down_speed,
                                                                     motionplus.pitch_left_speed,
                                                                     motionplus.roll_left_speed);

            return new Gyro((motionplus.slow_modes & 0x1) == 0x1 ? gyro.x : gyro.x * fastModeFactor,
                            (motionplus.slow_modes & 0x4) == 0x4 ? gyro.y : gyro.y * fastModeFactor,
                            (motionplus.slow_modes & 0x2) == 0x2 ? gyro.z : gyro.z * fastModeFactor);
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
                        Stick = calibration.NormalizeNunchuckStick(DateTime.Now,
                                                                   rawData.nunchuck.stick_x,
                                                                   rawData.nunchuck.stick_y),
                        Acceleration = calibration.NormalizeNunchuckAcceleration(DateTime.Now,
                                                                                 rawData.nunchuck.x,
                                                                                 rawData.nunchuck.y,
                                                                                 rawData.nunchuck.z)
                    };
            }
        }
    }
}