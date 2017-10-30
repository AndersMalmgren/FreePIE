using FreePIE.Core.Plugins.TrackIR;
using System;
using System.Runtime.InteropServices;

namespace FreePIE.Core.Plugins.Wiimote
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DolphiimoteAcceleration
    {
        public UInt16 x, y, z;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DolphiimoteClassicController
    {
        public byte left_stick_x;
        public byte left_stick_y;

        public byte right_stick_x;
        public byte right_stick_y;

        public byte left_trigger;
        public byte right_trigger;

        public UInt16 buttons;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DolphiimoteGuitar
    {
        public byte is_gh3;

        public byte stick_x;
        public byte stick_y;

        public byte tap_bar;
        public byte whammy_bar;

        public UInt16 buttons;
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
    public struct DolphiimoteBalanceBoardSensorRaw
    {
        public UInt16 top_right;
        public UInt16 bottom_right;
        public UInt16 top_left;
        public UInt16 bottom_left;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct DolphiimoteBalanceBoardSensor
    {
        public float top_right;
        public float bottom_right;
        public float top_left;
        public float bottom_left;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct DolphiimoteBalanceBoard
    {
        public DolphiimoteBalanceBoardSensorRaw raw;
        public DolphiimoteBalanceBoardSensorRaw calibration_kg0;
        public DolphiimoteBalanceBoardSensorRaw calibration_kg17;
        public DolphiimoteBalanceBoardSensorRaw calibration_kg34;
        public DolphiimoteBalanceBoardSensor kg;
        public DolphiimoteBalanceBoardSensor lb;
        public float weight_kg;
        public float weight_lb;
        public float center_of_gravity_x;
        public float center_of_gravity_y;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct DolphiimoteData
    {
        public UInt16 button_state;

        public UInt32 valid_data_flags;

        public DolphiimoteAcceleration acceleration;
        public DolphiimoteMotionplus motionplus;
        public DolphiimoteNunchuck nunchuck;
        public DolphiimoteClassicController classic_controller;
        public DolphiimoteGuitar guitar;
        public DolphiimoteBalanceBoard balance_board;
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
        public override string ToString()
        {
            return String.Format("yaw: {0}, pitch: {1}, roll: {2}", yaw, pitch, roll);
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

    public class DolphiimoteDll : IDisposable
    {
        private readonly NativeDll nativeDll;

        private readonly DolphiimoteInit dolphiimoteInit;
        private readonly DolphiimoteUpdate dolphiimoteUpdate;
        private readonly DolphiimoteDetermineCapabilities dolphiimoteDetermineCapabilities;
        private readonly DolphiimoteSetReportingMode dolphiimoteSetReportingMode;
        private readonly DolphiimoteShutdown dolphiimoteShutdown;
        private readonly DolphiimoteEnableCapabilities dolphiimoteEnableCapabilities;

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
        private OnLog logCallback;

        public DolphiimoteDll(string path)
        {
            nativeDll = new NativeDll(path);

            if (Marshal.SizeOf(typeof(DolphiimoteCallbacks)) != 20)
                throw new InvalidOperationException("DolphiimoteCallbacks wrong size.");

            if (Marshal.SizeOf(typeof(DolphiimoteData)) != 128)
                throw new InvalidOperationException("DolphiimoteData wrong size. Expected: 128, got:"+ Marshal.SizeOf(typeof(DolphiimoteData)));

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

        public int Init(Action<byte, DolphiimoteData> newData, Action<byte, bool> wiimoteConnectionChanged, Action<byte, DolphiimoteCapabilities> capabilitiesChanged, Action<string> newLogMessage)
        {
            dataCallback = (wiimote, data, user) => newData(wiimote, MarshalType<DolphiimoteData>(data));
            capabilitiesCallback = (wiimote, capabilities, user) => capabilitiesChanged(wiimote, MarshalType<DolphiimoteCapabilities>(capabilities));
            connectionCallback = (wiimote, status) => wiimoteConnectionChanged(wiimote, Convert.ToBoolean(status));
            logCallback = (str, length) => newLogMessage(Marshal.PtrToStringAnsi(str, (int)length));

            var callbacks = new DolphiimoteCallbacks
            {
                dataReceived = MarshalFunction(dataCallback),
                capabilitiesChanged = MarshalFunction(capabilitiesCallback),
                connectionChanged = MarshalFunction(connectionCallback),
                onLog = MarshalFunction(logCallback),
                userData = IntPtr.Zero
            };

            return dolphiimoteInit(callbacks);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void DolphiimoteEnableCapabilities(byte wiimote, ushort capabilities);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate Int32 DolphiimoteInit(DolphiimoteCallbacks onUpdate);

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
