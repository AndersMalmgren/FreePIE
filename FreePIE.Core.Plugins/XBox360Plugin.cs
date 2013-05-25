using System;
using System.Collections.Generic;
using FreePIE.Core.Contracts;
using SlimDX.XInput;

namespace FreePIE.Core.Plugins {
      
   //==========================================================================
   //                          XBox360Plugin
   //==========================================================================
   [GlobalType(Type = typeof(XBox360PluginGlobal), IsIndexed=true)]
   public class XBox360Plugin : Plugin {

      List<XBox360PluginGlobal> devices;

      //----------------------------------------------------------------------- 
      public override object CreateGlobal()
      {
          devices = new List<XBox360PluginGlobal>();
          // XInput only supports 4 gamepads, so we can use a fixed size here
          UserIndex[] indices = { UserIndex.One, UserIndex.Two, UserIndex.Three, UserIndex.Four };
          foreach(UserIndex i in indices) {
              devices.Add(new XBox360PluginGlobal(i));
          }
          return devices.ToArray();
      }

      public override void DoBeforeNextExecute()
      {
          devices.ForEach(d => d.Update());
      }

      //-----------------------------------------------------------------------
      public override string FriendlyName {
         get { return "XBox360 Controller"; }
      }
   }

   //==========================================================================
   //                          FreeSpacePluginGlobal
   //==========================================================================
   [Global(Name = "xbox360")]
   public class XBox360PluginGlobal {
      private readonly UserIndex i;
      private Gamepad pad;

      //-----------------------------------------------------------------------
      public XBox360PluginGlobal(UserIndex i)
      {
         this.i = i;
      }

      internal void Update() {
          Controller c = new Controller(i);
          if (c.IsConnected) pad = c.GetState().Gamepad;
          else pad = new Gamepad();
      }

      //-----------------------------------------------------------------------
      public bool a {
          get { Update(); return ((pad.Buttons & GamepadButtonFlags.A) != 0); }
      }

      //-----------------------------------------------------------------------
      public bool b {
          get { Update(); return ((pad.Buttons & GamepadButtonFlags.B) != 0); }
      }

      //-----------------------------------------------------------------------
      public bool x {
          get { Update(); return ((pad.Buttons & GamepadButtonFlags.X) != 0); }
      }

      //-----------------------------------------------------------------------
      public bool y {
          get { Update(); return ((pad.Buttons & GamepadButtonFlags.Y) != 0); }
      }

      //-----------------------------------------------------------------------
      public bool leftShoulder {
          get { Update(); return ((pad.Buttons & GamepadButtonFlags.LeftShoulder) != 0); }
      }

      //-----------------------------------------------------------------------
      public bool rightShoulder {
          get { Update(); return ((pad.Buttons & GamepadButtonFlags.RightShoulder) != 0); }
      }

      //-----------------------------------------------------------------------
      public bool start {
          get { Update(); return ((pad.Buttons & GamepadButtonFlags.Start) != 0); }
      }

      //-----------------------------------------------------------------------
      public bool back {
          get { Update(); return ((pad.Buttons & GamepadButtonFlags.Back) != 0); }
      }

      //-----------------------------------------------------------------------
      public bool up {
          get { Update(); return ((pad.Buttons & GamepadButtonFlags.DPadUp) != 0); }
      }

      //-----------------------------------------------------------------------
      public bool down {
          get { Update(); return ((pad.Buttons & GamepadButtonFlags.DPadDown) != 0); }
      }

      //-----------------------------------------------------------------------
      public bool left {
          get { Update(); return ((pad.Buttons & GamepadButtonFlags.DPadLeft) != 0); }
      }

      //-----------------------------------------------------------------------
      public bool right {
          get { Update(); return ((pad.Buttons & GamepadButtonFlags.DPadRight) != 0); }
      }

      //-----------------------------------------------------------------------
      public double leftTrigger {
          get { Update(); return (pad.LeftTrigger / 255.0); }
      }

      //-----------------------------------------------------------------------
      public double rightTrigger {
          get { Update(); return (pad.RightTrigger / 255.0); }
      }

      //-----------------------------------------------------------------------
      public bool leftThumb {
          get { Update(); return ((pad.Buttons & GamepadButtonFlags.LeftThumb) != 0); }
      }

      //-----------------------------------------------------------------------
      public bool rightThumb {
          get { Update(); return ((pad.Buttons & GamepadButtonFlags.RightThumb) != 0); }
      }

      //-----------------------------------------------------------------------
      public double leftStickX {
         get {
             Update();
             if (pad.LeftThumbX < 0)
                 return pad.LeftThumbX / 32768.0;
             else
                 return pad.LeftThumbX / 32767.0;
         }
      }

      //-----------------------------------------------------------------------
      public double leftStickY {
         get {
             Update();
             if (pad.LeftThumbY < 0)
                 return pad.LeftThumbY / 32768.0;
             else
                 return pad.LeftThumbY / 32767.0;
         }
      }

      //-----------------------------------------------------------------------
      public double rightStickX {
          get {
              Update();
              if (pad.RightThumbX < 0)
                  return pad.RightThumbX / 32768.0;
              else
                  return pad.RightThumbX / 32767.0;
          }
      }

      //-----------------------------------------------------------------------
      public double rightStickY {
         get {
             Update();
             if (pad.RightThumbY < 0)
                 return pad.RightThumbY / 32768.0;
             else
                 return pad.RightThumbY / 32767.0;
         }
      }
   }
}
