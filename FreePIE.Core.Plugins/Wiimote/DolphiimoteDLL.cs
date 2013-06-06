using System.Threading;
using AHRS;
using FreePIE.Core.Plugins.SensorFusion;
using FreePIE.Core.Plugins.TrackIR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace FreePIE.Core.Plugins.Wiimote
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DolphiimoteAcceleration
    {
        public UInt16 x, y, z;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DolphiimoteNunchuck
    {
        public byte stick_x;
        public byte stick_y;

        public UInt16 x, y, z;
        public byte buttons;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DolphiimoteMotionplus
    {
        public UInt16 yaw_down_speed;
        public UInt16 roll_left_speed;
        public UInt16 pitch_left_speed;

        public byte slow_modes; //Yaw = 0x1, Roll = 0x2, Pitch = 0x4.
        public byte extension_connected;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DolphiimoteData
    {
        public UInt16 button_state;

        public UInt32 valid_data_flags;

        public DolphiimoteAcceleration acceleration;
        public DolphiimoteMotionplus motionplus;
        public DolphiimoteNunchuck nunchuck;
    }

    public class DolphiimoteWiimoteData : IWiimoteData
    {
        private DolphiimoteData data;
        private WiimoteCalibration calibration;
        private uint wiimoteNumber;
        private IMotionPlusFuser fuser;

        public byte WiimoteNumber { get; private set; }

        public Tuple<double, double, double> MotionPlus { get; private set; }

        public EulerAngles MotionPlusEulerAngles
        {
            get
            {
                return fuser.FusedValues;
            }
        }

        public Tuple<double, double, double> Acceleration { get; private set; }

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

        private Tuple<double, double, double> CalculateMotionPlus(DolphiimoteMotionplus motionplus)
        {
            return new Tuple<double, double, double>(TransformLinear((motionplus.slow_modes & 0x1) == 0x1 ? calibration.MotionPlusGainSlow : calibration.MotionPlusGainFast, calibration.MotionPlusOffset, data.motionplus.yaw_down_speed),
                                                     TransformLinear((motionplus.slow_modes & 0x4) == 0x4 ? calibration.MotionPlusGainSlow : calibration.MotionPlusGainFast, calibration.MotionPlusOffset, data.motionplus.pitch_left_speed),
                                                     TransformLinear((motionplus.slow_modes & 0x2) == 0x2 ? calibration.MotionPlusGainSlow : calibration.MotionPlusGainFast, calibration.MotionPlusOffset, data.motionplus.roll_left_speed));
        }

        public void Update(DolphiimoteData data)
        {
            this.data = data;
            Acceleration = new Tuple<double, double, double>(TransformLinear(calibration.AccelerationGain, calibration.AccelerationOffset, data.acceleration.x),
                                                             TransformLinear(calibration.AccelerationGain, calibration.AccelerationOffset, data.acceleration.y),
                                                             TransformLinear(calibration.AccelerationGain, calibration.AccelerationOffset, data.acceleration.z));
            if (IsDataValid(WiimoteDataValid.MotionPlus))
            {
                MotionPlus = CalculateMotionPlus(data.motionplus);
                fuser.HandleIMUData(MotionPlus.Item1, MotionPlus.Item2, MotionPlus.Item3, Acceleration.Item1, Acceleration.Item2, Acceleration.Item3);
            }
        }
    }

    public class EulerAngles
    {
        public double yaw { get; set; }
        public double pitch { get; set; }
        public double roll { get; set; }

        public EulerAngles(double yaw, double pitch, double roll)
        {
            this.yaw = yaw;
            this.pitch = pitch;
            this.roll = roll;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DolphiimoteCapabilities
    {
        public UInt64 extension_id;
        public UInt32 extension_type;

        public UInt16 available_capabilities;
        public UInt16 enabled_capabilities;
    }

    public class DolphiimoteDLL : IDisposable
    {
        private readonly NativeDll nativeDll;

        private readonly DolphiimoteInit dolphiimoteInit;
        private readonly DolphiimoteUpdate dolphiimoteUpdate;
        private readonly DolphiimoteDetermineCapabilities dolphiimoteDetermineCapabilities;
        private readonly DolphiimoteSetReportingMode dolphiimoteSetReportingMode;
        private readonly DolphiimoteShutdown dolphiimoteShutdown;
        private readonly DolphiimoteEnableCapabilities dolphiimoteEnableCapabilities;
        private bool init = false;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void DataCallback(byte wiimote, IntPtr data, IntPtr userdata);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void CapabilitiesCallback(byte wiimote, IntPtr data, IntPtr userdata);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void ConnectionCallback(byte wiimote, byte status);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void OnLog(IntPtr str, UInt32 length);

        private DataCallback dataCallback;
        private CapabilitiesCallback capabilitiesCallback;
        private ConnectionCallback connectionCallback;
        private OnLog onLog;

        public DolphiimoteDLL(string path)
        {
            nativeDll = new NativeDll(path);

            if (Marshal.SizeOf(typeof(DolphiimoteCallbacks)) != 20)
                throw new InvalidOperationException("DolphiimoteCallbacks wrong size.");

            if (Marshal.SizeOf(typeof(DolphiimoteData)) != 32)
                throw new InvalidOperationException("DolphiimoteData wrong size.");

            if (Marshal.SizeOf(typeof(DolphiimoteCapabilities)) != 16)
                throw new InvalidOperationException("DolphiimoteCapabilities wrong size.");

            dolphiimoteInit = nativeDll.GetDelegateFromFunction<DolphiimoteInit>("dolphiimote_init");
            dolphiimoteDetermineCapabilities = nativeDll.GetDelegateFromFunction<DolphiimoteDetermineCapabilities>("dolphiimote_determine_capabilities");
            dolphiimoteUpdate = nativeDll.GetDelegateFromFunction<DolphiimoteUpdate>("dolphiimote_update");
            dolphiimoteSetReportingMode = nativeDll.GetDelegateFromFunction<DolphiimoteSetReportingMode>("dolphiimote_set_reporting_mode");
            dolphiimoteShutdown = nativeDll.GetDelegateFromFunction<DolphiimoteShutdown>("dolphiimote_shutdown");
            dolphiimoteEnableCapabilities = nativeDll.GetDelegateFromFunction<DolphiimoteEnableCapabilities>("dolphiimote_enable_capabilities");
        }

        private T MarshalType<T>(IntPtr data) where T : struct
        {
            return (T)Marshal.PtrToStructure(data, typeof(T));
        }

        private IntPtr MarshalFunction(Delegate del)
        {
            return Marshal.GetFunctionPointerForDelegate(del);
        }

        public int Init(Action<byte, DolphiimoteData> dataCallback, Action<byte, bool> wiimoteConnectionChanged, Action<byte, DolphiimoteCapabilities> capabilitiesChanged, Action<string> onLog)
        {
            this.dataCallback = (wiimote, data, user) => dataCallback(wiimote, MarshalType<DolphiimoteData>(data));
            this.capabilitiesCallback = (wiimote, capabilities, user) => capabilitiesChanged(wiimote, MarshalType<DolphiimoteCapabilities>(capabilities));
            this.connectionCallback = (wiimote, status) => wiimoteConnectionChanged(wiimote, Convert.ToBoolean(status));
            this.onLog = (str, length) => onLog(Marshal.PtrToStringAnsi(str, (int)length));

            DolphiimoteCallbacks callbacks = new DolphiimoteCallbacks();

            callbacks.dataReceived = MarshalFunction(this.dataCallback);
            callbacks.capabilitiesChanged = MarshalFunction(this.capabilitiesCallback);
            callbacks.connectionChanged = MarshalFunction(this.connectionCallback);
            callbacks.onLog = MarshalFunction(this.onLog);
            callbacks.userData = IntPtr.Zero;

            return dolphiimoteInit(callbacks);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void DolphiimoteEnableCapabilities(byte wiimote_number, ushort capabilities);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate Int32 DolphiimoteInit(DolphiimoteCallbacks on_update);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate Int32 DolphiimoteShutdown();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void DolphiimoteUpdate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void DolphiimoteDetermineCapabilities(byte wiimote);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void DolphiimoteSetReportingMode(byte wiimote, byte mode);

        [StructLayout(LayoutKind.Sequential)]
        private struct DolphiimoteCallbacks
        {
            public IntPtr dataReceived;
            public IntPtr connectionChanged;
            public IntPtr capabilitiesChanged;
            public IntPtr onLog;
            public IntPtr userData;
        }

        public void Update()
        {
            dolphiimoteUpdate();
        }

        public void SetReportingMode(byte wiimote, byte mode)
        {
            dolphiimoteSetReportingMode(wiimote, mode);
        }

        public void DetermineCapabilities(byte i)
        {
            dolphiimoteDetermineCapabilities(i);
        }

        public void Shutdown()
        {
            dolphiimoteShutdown();
        }

        public void Dispose()
        {
            Shutdown();
            nativeDll.Dispose();
        }

        public void EnableCapabilities(byte wiimote, WiimoteCapabilities flags)
        {
            dolphiimoteEnableCapabilities(wiimote, (ushort)flags);
        }
    }
}
