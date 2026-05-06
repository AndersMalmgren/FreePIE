using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.Globals;

namespace FreePIE.Core.Plugins
{
    [StructLayout(LayoutKind.Sequential)]
    public struct XUSB_REPORT
    {
        public ushort wButtons;
        public byte bLeftTrigger;
        public byte bRightTrigger;
        public short sThumbLX;
        public short sThumbLY;
        public short sThumbRX;
        public short sThumbRY;
    };

    [Flags]
    public enum XUSB_BUTTON : ushort
    {
        XUSB_GAMEPAD_DPAD_UP            = 0x0001,
        XUSB_GAMEPAD_DPAD_DOWN          = 0x0002,
        XUSB_GAMEPAD_DPAD_LEFT          = 0x0004,
        XUSB_GAMEPAD_DPAD_RIGHT         = 0x0008,
        XUSB_GAMEPAD_START              = 0x0010,
        XUSB_GAMEPAD_BACK               = 0x0020,
        XUSB_GAMEPAD_LEFT_THUMB         = 0x0040,
        XUSB_GAMEPAD_RIGHT_THUMB        = 0x0080,
        XUSB_GAMEPAD_LEFT_SHOULDER      = 0x0100,
        XUSB_GAMEPAD_RIGHT_SHOULDER     = 0x0200,
        XUSB_GAMEPAD_GUIDE              = 0x0400,
        XUSB_GAMEPAD_A                  = 0x1000,
        XUSB_GAMEPAD_B                  = 0x2000,
        XUSB_GAMEPAD_X                  = 0x4000,
        XUSB_GAMEPAD_Y                  = 0x8000
    };

    [GlobalType(Type= typeof(ViGEmGlobal), IsIndexed = true)]
    public class ViGEmPlugin : Plugin
    {
        private List<ViGEmGlobalHolder> holders;

        public override object CreateGlobal()
        {
            holders = new List<ViGEmGlobalHolder>();
            return new GlobalIndexer<ViGEmGlobal, uint>(Create);
        }

        public override void Stop()
        {
            holders.ForEach(h => h.Dispose());
        }

        private ViGEmGlobal Create(uint index)
        {
            var holder = new ViGEmGlobalHolder(index + 1);
            holders.Add(holder);
            return holder.Global;
        }

        public override void DoBeforeNextExecute()
        {
            holders.ForEach(h => h.DoBeforeNextExecute());
        }

        public override string FriendlyName
        {
            get { return "Virtual Gamepad Emulation Framework (ViGEm)"; }
        }
    }

    public class ViGEmGlobalHolder : IDisposable
    {
        private XUSB_REPORT prev, next;
        private IntPtr client;
        private IntPtr pad;
        private bool connected = false;
        private bool added = false;

        // DLL begin
        [DllImport("ViGEmClient.dll")]
        private static extern IntPtr vigem_alloc();

        [DllImport("ViGEmClient.dll")]
        private static extern void vigem_free(IntPtr client);

        [DllImport("ViGEmClient.dll")]
        private static extern VIGEM_ERROR vigem_connect(IntPtr client);

        [DllImport("ViGEmClient.dll")]
        private static extern VIGEM_ERROR vigem_disconnect(IntPtr client);

        [DllImport("ViGEmClient.dll")]
        private static extern IntPtr vigem_target_x360_alloc();

        [DllImport("ViGEmClient.dll")]
        private static extern VIGEM_ERROR vigem_target_x360_update(IntPtr client, IntPtr target, XUSB_REPORT report);

        [DllImport("ViGEmClient.dll")]
        private static extern void vigem_target_free(IntPtr target);

        [DllImport("ViGEmClient.dll")]
        private static extern VIGEM_ERROR vigem_target_add(IntPtr client, IntPtr target);

        [DllImport("ViGEmClient.dll")]
        private static extern VIGEM_ERROR vigem_target_remove(IntPtr client, IntPtr target);

        private enum VIGEM_ERROR : uint
        {
            //
            // API succeeded.
            //
            VIGEM_ERROR_NONE = 0x20000000,
            //
            // A compatible bus driver wasn't found on the system.
            //
            VIGEM_ERROR_BUS_NOT_FOUND = 0xE0000001,
            //
            // All device slots are occupied, no new device can be spawned.
            //
            VIGEM_ERROR_NO_FREE_SLOT = 0xE0000002,
            VIGEM_ERROR_INVALID_TARGET = 0xE0000003,
            VIGEM_ERROR_REMOVAL_FAILED = 0xE0000004,
            //
            // An attempt has been made to plug in an already connected device.
            //
            VIGEM_ERROR_ALREADY_CONNECTED = 0xE0000005,
            //
            // The target device is not initialized.
            //
            VIGEM_ERROR_TARGET_UNINITIALIZED = 0xE0000006,
            //
            // The target device is not plugged in.
            //
            VIGEM_ERROR_TARGET_NOT_PLUGGED_IN = 0xE0000007,
            //
            // It's been attempted to communicate with an incompatible driver version.
            //
            VIGEM_ERROR_BUS_VERSION_MISMATCH = 0xE0000008,
            //
            // Bus driver found but failed to open a handle.
            //
            VIGEM_ERROR_BUS_ACCESS_FAILED = 0xE0000009,
            VIGEM_ERROR_CALLBACK_ALREADY_REGISTERED = 0xE0000010,
            VIGEM_ERROR_CALLBACK_NOT_FOUND = 0xE0000011,
            VIGEM_ERROR_BUS_ALREADY_CONNECTED = 0xE0000012,
            VIGEM_ERROR_BUS_INVALID_HANDLE = 0xE0000013,
            VIGEM_ERROR_XUSB_USERINDEX_OUT_OF_RANGE = 0xE0000014,
            VIGEM_ERROR_INVALID_PARAMETER = 0xE0000015,
            //
            // The API is not supported by the driver.
            //
            VIGEM_ERROR_NOT_SUPPORTED = 0xE0000016,
            //
            // An unexpected Win32 API error occurred. Call GetLastError() for details.
            //
            VIGEM_ERROR_WINAPI = 0xE0000017,
            //
            // The specified timeout has been reached.
            //
            VIGEM_ERROR_TIMED_OUT = 0xE0000018,
            VIGEM_ERROR_IS_DISPOSING = 0xE0000019,
        };
        // DLL end

        public ViGEmGlobalHolder(uint index)
        {
            Index = index;
            Global = new ViGEmGlobal(this);

            client = vigem_alloc();
            if (client == null)
            {
                throw new Exception("vigem_alloc() returned null!");
            }
            VIGEM_ERROR status = vigem_connect(client);
            if (status != VIGEM_ERROR.VIGEM_ERROR_NONE)
            {
                throw new Exception(string.Format("vigem_connect(): {0}", status));
            }
            connected = true;

            pad = vigem_target_x360_alloc();
            if (pad == null)
            {
                throw new Exception("vigem_target_x360_alloc() returned null!");
            }
            status = vigem_target_add(client, pad);
            if (status != VIGEM_ERROR.VIGEM_ERROR_NONE)
            {
                throw new Exception(string.Format("vigem_target_add(): {0}", status));
            }
            added = true;
        }

        public void DoBeforeNextExecute()
        {
            if (!prev.Equals(next))
            {
                VIGEM_ERROR status = vigem_target_x360_update(client, pad, next);
                if (status != VIGEM_ERROR.VIGEM_ERROR_NONE)
                {
                    throw new Exception(string.Format("vigem_target_x360_update(): {0}", status));
                }
                prev = next;
            }
        }

        public ref XUSB_REPORT State() { return ref next; }
        public void setButton(XUSB_BUTTON btn, bool value)
        {
            if (value) {
                next.wButtons |= (ushort)btn;
            } else {
                next.wButtons &= (ushort)~btn;
            }
        }
        public bool getButton(XUSB_BUTTON btn) { return (next.wButtons & (ushort)btn) != 0; }

        public ViGEmGlobal Global { get; private set; }
        public uint Index { get; private set; }

        public void Dispose()
        {
            if (added)
            {
                vigem_target_remove(client, pad);
            }
            if (pad != null)
            {
                vigem_target_free(pad);
            }
            if (connected)
            {
                vigem_disconnect(client);
            }
            if (client != null)
            {
                vigem_free(client);
            }
        }
    }

    [Global(Name = "vxbox")]
    public class ViGEmGlobal
    {
        private readonly ViGEmGlobalHolder holder;

        public ViGEmGlobal(ViGEmGlobalHolder holder)
        {
            this.holder = holder;
        }

        // be consistent with the xbox plugin
        public bool a
        {
            get { return holder.getButton(XUSB_BUTTON.XUSB_GAMEPAD_A); }
            set { holder.setButton(XUSB_BUTTON.XUSB_GAMEPAD_A, value); }
        }
        public bool b
        {
            get { return holder.getButton(XUSB_BUTTON.XUSB_GAMEPAD_B); }
            set { holder.setButton(XUSB_BUTTON.XUSB_GAMEPAD_B, value); }
        }
        public bool x
        {
            get { return holder.getButton(XUSB_BUTTON.XUSB_GAMEPAD_X); }
            set { holder.setButton(XUSB_BUTTON.XUSB_GAMEPAD_X, value); }
        }
        public bool y
        {
            get { return holder.getButton(XUSB_BUTTON.XUSB_GAMEPAD_Y); }
            set { holder.setButton(XUSB_BUTTON.XUSB_GAMEPAD_Y, value); }
        }
        public bool leftShoulder
        {
            get { return holder.getButton(XUSB_BUTTON.XUSB_GAMEPAD_LEFT_SHOULDER); }
            set { holder.setButton(XUSB_BUTTON.XUSB_GAMEPAD_LEFT_SHOULDER, value); }
        }
        public bool rightShoulder
        {
            get { return holder.getButton(XUSB_BUTTON.XUSB_GAMEPAD_RIGHT_SHOULDER); }
            set { holder.setButton(XUSB_BUTTON.XUSB_GAMEPAD_RIGHT_SHOULDER, value); }
        }
        public bool leftThumb
        {
            get { return holder.getButton(XUSB_BUTTON.XUSB_GAMEPAD_LEFT_THUMB); }
            set { holder.setButton(XUSB_BUTTON.XUSB_GAMEPAD_LEFT_THUMB, value); }
        }
        public bool rightThumb
        {
            get { return holder.getButton(XUSB_BUTTON.XUSB_GAMEPAD_RIGHT_THUMB); }
            set { holder.setButton(XUSB_BUTTON.XUSB_GAMEPAD_RIGHT_THUMB, value); }
        }
        public bool start
        {
            get { return holder.getButton(XUSB_BUTTON.XUSB_GAMEPAD_START); }
            set { holder.setButton(XUSB_BUTTON.XUSB_GAMEPAD_START, value); }
        }
        public bool back
        {
            get { return holder.getButton(XUSB_BUTTON.XUSB_GAMEPAD_BACK); }
            set { holder.setButton(XUSB_BUTTON.XUSB_GAMEPAD_BACK, value); }
        }
        public bool up
        {
            get { return holder.getButton(XUSB_BUTTON.XUSB_GAMEPAD_DPAD_UP); }
            set { holder.setButton(XUSB_BUTTON.XUSB_GAMEPAD_DPAD_UP, value); }
        }
        public bool down
        {
            get { return holder.getButton(XUSB_BUTTON.XUSB_GAMEPAD_DPAD_DOWN); }
            set { holder.setButton(XUSB_BUTTON.XUSB_GAMEPAD_DPAD_DOWN, value); }
        }
        public bool left
        {
            get { return holder.getButton(XUSB_BUTTON.XUSB_GAMEPAD_DPAD_LEFT); }
            set { holder.setButton(XUSB_BUTTON.XUSB_GAMEPAD_DPAD_LEFT, value); }
        }
        public bool right
        {
            get { return holder.getButton(XUSB_BUTTON.XUSB_GAMEPAD_DPAD_RIGHT); }
            set { holder.setButton(XUSB_BUTTON.XUSB_GAMEPAD_DPAD_RIGHT, value); }
        }

        public double leftTrigger
        {
            get { return holder.State().bLeftTrigger / 255.0; }
            set { holder.State().bLeftTrigger = Convert.ToByte(value * 255.0); }
        }
        public double rightTrigger
        {
            get { return holder.State().bRightTrigger / 255.0; }
            set { holder.State().bRightTrigger = Convert.ToByte(value * 255.0); }
        }

        public double leftStickX
        {
            get
            {
                short value = holder.State().sThumbLX;
                return value < 0 ? value / 32768.0 : value / 32767.0;   // XXX: non-uniform?
            }
            set
            {
                holder.State().sThumbLX = Convert.ToInt16(value < 0 ? value * 32768.0 : value * 32767.0);
            }
        }
        public double leftStickY
        {
            get
            {
                short value = holder.State().sThumbLY;
                return value < 0 ? value / 32768.0 : value / 32767.0;   // XXX: non-uniform?
            }
            set
            {
                holder.State().sThumbLY = Convert.ToInt16(value < 0 ? value * 32768.0 : value * 32767.0);
            }
        }
        public double rightStickX
        {
            get
            {
                short value = holder.State().sThumbRX;
                return value < 0 ? value / 32768.0 : value / 32767.0;   // XXX: non-uniform?
            }
            set
            {
                holder.State().sThumbRX = Convert.ToInt16(value < 0 ? value * 32768.0 : value * 32767.0);
            }
        }
        public double rightStickY
        {
            get
            {
                short value = holder.State().sThumbRY;
                return value < 0 ? value / 32768.0 : value / 32767.0;   // XXX: non-uniform?
            }
            set
            {
                holder.State().sThumbRY = Convert.ToInt16(value < 0 ? value * 32768.0 : value * 32767.0);
            }
        }

        // raw analog values
        public byte lt
        {
            get { return holder.State().bLeftTrigger; }
            set { holder.State().bLeftTrigger = value; }
        }
        public byte rt
        {
            get { return holder.State().bRightTrigger; }
            set { holder.State().bRightTrigger = value; }
        }
        public short lx
        {
            get { return holder.State().sThumbLX; }
            set { holder.State().sThumbLX = value; }
        }
        public short ly
        {
            get { return holder.State().sThumbLY; }
            set { holder.State().sThumbLY = value; }
        }
        public short rx
        {
            get { return holder.State().sThumbRX; }
            set { holder.State().sThumbRX = value; }
        }
        public short ry
        {
            get { return holder.State().sThumbRY; }
            set { holder.State().sThumbRY = value; }
        }
    }
}
