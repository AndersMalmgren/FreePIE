using System;

namespace FreePIE.Core.Plugins.Wiimote
{
    public class DolphiimoteWiimoteData : IWiimoteData
    {
        private readonly WiimoteCalibration calibration;
        private readonly IMotionPlusFuser fuser;
        private DolphiimoteData data;

        public bool PlayingSound { get; set; }

        public byte WiimoteNumber { get; private set; }

        public CalibratedValue<Gyro> MotionPlus { get; private set; }

        public EulerAngles MotionPlusEulerAngles
        {
            get
            {
                return fuser.FusedValues;
            }
        }

        public BalanceBoard BalanceBoard { get; private set; }
        public Nunchuck Nunchuck { get; private set; }
        public ClassicController ClassicController { get; private set; }
        public Guitar Guitar { get; private set; }
        public CalibratedValue<Acceleration> Acceleration { get; private set; }
        public WiimoteCapabilities EnabledCapabilities { get; set; }
        public WiimoteCapabilities AvailableCapabilities { get; set; }
        public WiimoteExtensions ExtensionType { get; set; }
        public int BatteryPercentage { get; set; }
        public int LEDStatus { get; set; }
        public ulong ExtensionID { get; set; }
        byte map(byte value, byte istart, byte istop, byte ostart, byte ostop)
        {
            return (byte)(ostart + (float)(ostop - ostart) * ((float)(value - istart) / (float)(istop - istart)));
        }
        private static byte CLASSIC_LEFT_STICK_MAX = 63;
        private static byte CLASSIC_OTHER_MAX = 31;
        public DolphiimoteWiimoteData(byte wiimoteNumber, WiimoteCalibration calibration, IMotionPlusFuser fuser)
        {
            WiimoteNumber = wiimoteNumber;

            this.calibration = calibration;
            this.fuser = fuser;

            MotionPlus = new CalibratedValue<Gyro>(false, new Gyro(0, 0, 0));
            Acceleration = new CalibratedValue<Acceleration>(false, new Acceleration(0, 0, 0));

            Nunchuck = new Nunchuck
            {
                Acceleration = new Acceleration(0, 0, 0),
                Stick = new AnalogStick(0, 0)
            };

            ClassicController = new ClassicController
            {
                LeftStick = new AnalogStick(0, 0),
                RightStick = new AnalogStick(0, 0),
                RightTrigger = new AnalogTrigger(0),
                LeftTrigger = new AnalogTrigger(0)
            };

            Guitar = new Guitar
            {
                Stick = new AnalogStick(0, 0),
                TapBar = new TapBar(0x0F),
                Whammy = new AnalogTrigger(0),
                IsGH3 = false
            };
            BalanceBoardSensor def = new BalanceBoardSensor
            {
                calibration = new BalanceBoardSensorCalibration
                {
                    kg00 = 0,
                    kg17 = 0,
                    kg34 = 0
                },
                kg = 0,
                lb = 0,
                raw = 0
            };
            BalanceBoard = new BalanceBoard
            {
                sensors = new BalanceBoardSensorList
                {
                    bottomLeft = def,
                    bottomRight = def,
                    topLeft = def,
                    topRight = def
                },
                weight = new BalanceBoardWeight
                {
                    kg = 0,
                    lb = 0,
                    raw = 0
                },
                centerOfGravity = new AnalogStick(0, 0)
            };
            BatteryPercentage = 0;
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
            UInt16 orig = (UInt16)Nunchuck.Buttons;
            return (orig & value) == value;
        }

        public bool IsClassicControllerButtonPressed(ClassicControllerButtons classicControllerButtons)
        {
            UInt16 value = (UInt16)classicControllerButtons;
            UInt16 orig = (UInt16)ClassicController.Buttons;
            return (orig & value) == value;
        }
        public bool IsGuitarButtonPressed(GuitarButtons guitarButtons)
        {
            UInt16 value = (UInt16)guitarButtons;
            UInt16 orig = (UInt16)Guitar.Buttons;
            return (orig & value) == value;
        }

        private CalibratedValue<Gyro> CalculateMotionPlus(DolphiimoteMotionplus motionplus)
        {
            //const double fastModeFactor = 2000.0 / 440.0; //According to wiibrew
            const double fastModeFactor = 20.0 / 4.0; //According to wiic
            var gyro = calibration.NormalizeMotionplus(DateTime.Now, motionplus.yaw_down_speed,
                                                                     motionplus.pitch_left_speed,
                                                                     motionplus.roll_left_speed);

            return new CalibratedValue<Gyro>(gyro.DidCalibrate, new Gyro((motionplus.slow_modes & 0x1) == 0x1 ? gyro.Value.x : gyro.Value.x * fastModeFactor,
                                                                         (motionplus.slow_modes & 0x4) == 0x4 ? gyro.Value.y : gyro.Value.y * fastModeFactor,
                                                                         (motionplus.slow_modes & 0x2) == 0x2 ? gyro.Value.z : gyro.Value.z * fastModeFactor));
        }

        public void Update(DolphiimoteData rawData)
        {
            this.data = rawData;

            Acceleration = calibration.NormalizeAcceleration(DateTime.Now, rawData.acceleration.x, rawData.acceleration.y, rawData.acceleration.z);

            if (IsDataValid(WiimoteDataValid.MotionPlus))
            {
                MotionPlus = CalculateMotionPlus(rawData.motionplus);
                fuser.HandleIMUData(MotionPlus.Value.x, MotionPlus.Value.y, MotionPlus.Value.z, Acceleration.Value.x, Acceleration.Value.y, Acceleration.Value.z);
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
                                                                                 rawData.nunchuck.z),
                    Buttons = (NunchuckButtons)data.nunchuck.buttons
                    };
            }
            if (IsDataValid(WiimoteDataValid.ClassicController))
            {
                ClassicController = new ClassicController
                {
                    RightStick = calibration.NormalizeClassicControllerRightStick(DateTime.Now,
                                                               map(rawData.classic_controller.right_stick_x, 0, CLASSIC_OTHER_MAX, 0, 200),
                                                               map(rawData.classic_controller.right_stick_y, 0, CLASSIC_OTHER_MAX, 0, 200)),
                    LeftStick = calibration.NormalizeClassicControllerLeftStick(DateTime.Now,
                                                               map(rawData.classic_controller.left_stick_x, 0, CLASSIC_LEFT_STICK_MAX, 0, 200),
                                                               map(rawData.classic_controller.left_stick_y, 0, CLASSIC_LEFT_STICK_MAX, 0, 200)),
                    RightTrigger = calibration.NormalizeClassicControllerRightTrigger(DateTime.Now,
                                                                map(rawData.classic_controller.right_trigger, 0, CLASSIC_OTHER_MAX, 0, 100)),
                    LeftTrigger = calibration.NormalizeClassicControllerLeftTrigger(DateTime.Now,
                                                                map(rawData.classic_controller.left_trigger, 0, CLASSIC_OTHER_MAX, 0, 100)),
                    Buttons = (ClassicControllerButtons)data.classic_controller.buttons
                };
            }
            if (IsDataValid(WiimoteDataValid.Guitar))
            {
                Guitar = new Guitar
                {
                    Stick = calibration.NormalizeGuitarStick(DateTime.Now,
                                                               rawData.guitar.stick_x,
                                                               rawData.guitar.stick_y),
                    Whammy = calibration.NormalizeGuitarWhammy(DateTime.Now,
                                                               rawData.guitar.whammy_bar),
                    IsGH3 = rawData.guitar.is_gh3 == 1,
                    TapBar = new TapBar(rawData.guitar.tap_bar),
                    Buttons = (GuitarButtons)data.guitar.buttons
                };
            }
            if (IsDataValid(WiimoteDataValid.BalanceBoard))
            {
                BalanceBoard = new BalanceBoard
                {
                    sensors = new BalanceBoardSensorList
                    {
                        bottomLeft = new BalanceBoardSensor
                        {
                            calibration = new BalanceBoardSensorCalibration
                            {
                                kg00 = rawData.balance_board.calibration_kg0.bottom_left,
                                kg17 = rawData.balance_board.calibration_kg17.bottom_left,
                                kg34 = rawData.balance_board.calibration_kg34.bottom_left
                            },
                            kg = rawData.balance_board.kg.bottom_left,
                            lb = rawData.balance_board.lb.bottom_left,
                            raw = rawData.balance_board.raw.bottom_left
                        },
                        bottomRight = new BalanceBoardSensor
                        {
                            calibration = new BalanceBoardSensorCalibration
                            {
                                kg00 = rawData.balance_board.calibration_kg0.bottom_right,
                                kg17 = rawData.balance_board.calibration_kg17.bottom_right,
                                kg34 = rawData.balance_board.calibration_kg34.bottom_right
                            },
                            kg = rawData.balance_board.kg.bottom_right,
                            lb = rawData.balance_board.lb.bottom_right,
                            raw = rawData.balance_board.raw.bottom_right
                        },
                        topLeft = new BalanceBoardSensor
                        {
                            calibration = new BalanceBoardSensorCalibration
                            {
                                kg00 = rawData.balance_board.calibration_kg0.top_left,
                                kg17 = rawData.balance_board.calibration_kg17.top_left,
                                kg34 = rawData.balance_board.calibration_kg34.top_left
                            },
                            kg = rawData.balance_board.kg.top_left,
                            lb = rawData.balance_board.lb.top_left,
                            raw = rawData.balance_board.raw.top_left
                        },
                        topRight = new BalanceBoardSensor
                        {
                            calibration = new BalanceBoardSensorCalibration
                            {
                                kg00 = rawData.balance_board.calibration_kg0.top_right,
                                kg17 = rawData.balance_board.calibration_kg17.top_right,
                                kg34 = rawData.balance_board.calibration_kg34.top_right
                            },
                            kg = rawData.balance_board.kg.top_right,
                            lb = rawData.balance_board.lb.top_right,
                            raw = rawData.balance_board.raw.top_right
                        },
                    },
                    weight = new BalanceBoardWeight
                    {
                        kg = rawData.balance_board.weight_kg,
                        lb = rawData.balance_board.weight_lb,
                        raw = rawData.balance_board.raw.bottom_left + rawData.balance_board.raw.bottom_right + rawData.balance_board.raw.top_left + rawData.balance_board.raw.top_right
                    },
                    centerOfGravity = new AnalogStick(rawData.balance_board.center_of_gravity_x, rawData.balance_board.center_of_gravity_y)
                };
            }
        }
    }
}