using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace FreePIE.Core.Plugins.OculusVR
{
    [Flags]
    public enum ovrTouchButton:uint
    {
        A = 0x00000001,             // A button on XBox controllers and right Touch controller. Select button on Oculus Remote.
        B = 0x00000002,             // B button on XBox controllers and right Touch controller. Back button on Oculus Remote.
        RThumb = 0x00000004,        // Right thumbstick on XBox controllers and Touch controllers. Not present on Oculus Remote.
//      RShoulder = 0x00000008,     // Right shoulder button on XBox controllers. Not present on Touch controllers or Oculus Remote.

        X = 0x00000100,             // X button on XBox controllers and left Touch controller. Not present on Oculus Remote.
        Y = 0x00000200,             // Y button on XBox controllers and left Touch controller. Not present on Oculus Remote.
        LThumb = 0x00000400,        // Left thumbstick on XBox controllers and Touch controllers. Not present on Oculus Remote.
  //    LShoulder = 0x00000800,     // Left shoulder button on XBox controllers. Not present on Touch controllers or Oculus Remote.

        //Up = 0x00010000,          // Up button on XBox controllers and Oculus Remote. Not present on Touch controllers.
        //Down = 0x00020000,        // Down button on XBox controllers and Oculus Remote. Not present on Touch controllers.
        //Left = 0x00040000,        // Left button on XBox controllers and Oculus Remote. Not present on Touch controllers.
        //Right = 0x00080000,       // Right button on XBox controllers and Oculus Remote. Not present on Touch controllers.
        Enter = 0x00100000,         // Start on XBox 360 controller. Menu on XBox One controller and Left Touch controller. Should be referred to as the Menu button in user-facing documentation.
//        Back = 0x00200000,        // Back on Xbox 360 controller. View button on XBox One controller. Not present on Touch controllers or Oculus Remote.
//        VolUp = 0x00400000,       // Volume button on Oculus Remote. Not present on XBox or Touch controllers.
//        VolDown = 0x00800000,     // Volume button on Oculus Remote. Not present on XBox or Touch controllers.
        Home = 0x01000000,          // Home button on XBox controllers. Oculus button on Touch controllers and Oculus Remote.
    }
    [Flags]
    public enum ovrTouch:uint
    {
        A = ovrTouchButton.A,
        B = ovrTouchButton.B,
        RThumb = ovrTouchButton.RThumb,
        RThumbRest = 0x00000008,
        RTrigger = 0x00000010,

        X = ovrTouchButton.X,
        Y = ovrTouchButton.Y,
        LThumb = ovrTouchButton.LThumb,
        LThumbRest = 0x00000800,
        LTrigger = 0x00001000,

        RIndexPointing = 0x00000020,
        RThumbUp = 0x00000040,
        LIndexPointing = 0x00002000,
        LThumbUp = 0x00004000,

    }

    public enum ovrControllerType : uint
    {
        None = 0x00,
        LTouch = 0x01,
        RTouch = 0x02,
        Touch = 0x03,
        Remote = 0x04,
        XBox = 0x10,
        Active = 0xff,      //Operate on or query whichever controller is active.

    }
    [Flags]
    public enum ovrStatus : uint
    {
        NotTracking = 0,
        OrientationTracked = 0x0001,    //< Orientation is currently tracked (connected and in use).
        PositionTracked = 0x0002,    //< Position is currently tracked (false if out of range).
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct OculusVr6Dof
    {
        public float yaw;
        public float pitch;
        public float roll;
        public float yawDegrees;
        public float pitchDegrees;
        public float rollDegrees;
        
        public float x;
        public float y;
        public float z;

        
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Pointf
    {
        public float x, y;
    }

	[StructLayout(LayoutKind.Sequential)]
    public struct OculusVrData
    {
        public OculusVr6Dof Head;
        public OculusVr6Dof LeftHand;
        public OculusVr6Dof RightHand;

        public uint Touches;
        public uint Buttons;

        public float LTrigger;
        public float RTrigger;

        public float LGrip;
        public float RGrip;

        public Pointf Lstick;
        public Pointf Rstick;

        /// The type of the controller this state is for.
        public uint ControllerType;

        public uint StatusHead;
        public uint StatusLeftHand;
        public uint StatusRightHand;

        public uint HmdMounted;
    }
}
