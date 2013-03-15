using System;
using System.Collections.Generic;
using FreePIE.Core.Contracts;
using SlimDX.XInput;

namespace FreePIE.Core.Plugins {
      
   //==========================================================================
   //                          XBox360Plugin
   //==========================================================================
   [GlobalType(Type = typeof(XBox360PluginGlobal))]
   public class XBox360Plugin : Plugin {

      Controller XBoxController = new Controller(0);
      Gamepad Controller;
      bool connected;
      
       //----------------------------------------------------------------------- 
      public override object CreateGlobal() {
         return new XBox360PluginGlobal(this);
      }

      //-----------------------------------------------------------------------
      public override Action Start() {
         return null;
      }

      //-----------------------------------------------------------------------
      public override void Stop() {
      }

      //-----------------------------------------------------------------------
      public override string FriendlyName {
         get { return "XBox360 Controller"; }
      }

      //-----------------------------------------------------------------------
      public override bool GetProperty(int index, IPluginProperty property) {
         return false;
      }

      //-----------------------------------------------------------------------
      public override bool SetProperties(Dictionary<string, object> properties) {
         return true;
      }

      //-----------------------------------------------------------------------
      public override void DoBeforeNextExecute() {
         //This method will be executed each iteration of the script
          connected = XBoxController.IsConnected;
          if (XBoxController.IsConnected == true)
              Controller = XBoxController.GetState().Gamepad;
      }

      //-----------------------------------------------------------------------
      public bool A {
         get {
            if (connected == false)
                return false;
            return ((Controller.Buttons & GamepadButtonFlags.A) != 0);
         }
      }

      //-----------------------------------------------------------------------
      public bool B {
         get {
            if (connected == false)
                return false;
            return ((Controller.Buttons & GamepadButtonFlags.B) != 0);
         }
      }

      //-----------------------------------------------------------------------
      public bool X {
         get {
            if (connected == false)
                return false;
            return ((Controller.Buttons & GamepadButtonFlags.X) != 0);
         }
      }

      //-----------------------------------------------------------------------
      public bool Y {
         get {
            if (connected == false)
                return false;
            return ((Controller.Buttons & GamepadButtonFlags.Y) != 0);
         }
      }

      //-----------------------------------------------------------------------
      public bool LeftShoulder {
         get {
            if (connected == false)
                return false;
            return ((Controller.Buttons & GamepadButtonFlags.LeftShoulder) != 0);
         }
      }

      //-----------------------------------------------------------------------
      public bool RightShoulder {
         get {
            if (connected == false)
                return false;
            return ((Controller.Buttons & GamepadButtonFlags.RightShoulder) != 0);
         }
      }

      //-----------------------------------------------------------------------
      public bool StartBtn {
         get {
            if (connected == false)
                return false;
            return ((Controller.Buttons & GamepadButtonFlags.Start) != 0);
         }
      }

      //-----------------------------------------------------------------------
      public bool Back {
         get {
            if (connected == false)
                return false;
            return ((Controller.Buttons & GamepadButtonFlags.Back) != 0);
         }
      }

      //-----------------------------------------------------------------------
      public bool Up {
         get {
            if (connected == false)
                return false;
            return ((Controller.Buttons & GamepadButtonFlags.DPadUp) != 0);
         }
      }

      //-----------------------------------------------------------------------
      public bool Down {
         get {
            if (connected == false)
                return false;
            return ((Controller.Buttons & GamepadButtonFlags.DPadDown) != 0);
         }
      }

      //-----------------------------------------------------------------------
      public bool Left {
         get {
            if (connected == false)
                return false;
            return ((Controller.Buttons & GamepadButtonFlags.DPadLeft) != 0);
         }
      }

      //-----------------------------------------------------------------------
      public bool Right {
         get {
            if (connected == false)
                return false;
            return ((Controller.Buttons & GamepadButtonFlags.DPadRight) != 0);
         }
      }

      //-----------------------------------------------------------------------
      public double LeftTrigger {
         get {
            if (connected == false)
                return 0.0;
            return (Controller.LeftTrigger / 255.0);
         }
      }

      //-----------------------------------------------------------------------
      public double RightTrigger {
         get {
            if (connected == false)
                return 0.0;
            return (Controller.RightTrigger / 255.0);
         }
      }

      //-----------------------------------------------------------------------
      public bool LeftThumb {
         get {
            if (connected == false)
                return false;
            return ((Controller.Buttons & GamepadButtonFlags.LeftThumb) != 0);
         }
      }

      //-----------------------------------------------------------------------
      public double LeftStickX {
         // Return -1 to +1
         get {
            if (connected == false)
               return 0.0;
            if (Controller.LeftThumbX < 0)
               return Controller.LeftThumbX / 32768.0;
            else
               return Controller.LeftThumbX / 32767.0;
         }
      }

      //-----------------------------------------------------------------------
      public double LeftStickY {
         // Return -1 to +1
         get {
            if (connected == false)
               return 0.0;
            if (Controller.LeftThumbY < 0)
               return Controller.LeftThumbY / 32768.0;
            else
               return Controller.LeftThumbY / 32767.0;
         }
      }

      //-----------------------------------------------------------------------
      public bool RightThumb {
         get {
            if (connected == false)
               return false;
            return ((Controller.Buttons & GamepadButtonFlags.RightThumb) != 0);
         }
      }

      //-----------------------------------------------------------------------
      public double RightStickX {
         // Return -1 to +1
         get {
            if (connected == false)
               return 0.0;
            if (Controller.RightThumbX < 0)
               return Controller.RightThumbX / 32768.0;
            else
               return Controller.RightThumbX / 32767.0;
         }
      }

      //-----------------------------------------------------------------------
      public double RightStickY {
         // Return -1 to +1
         get {
            if (connected == false)
               return 0.0;
            if (Controller.RightThumbY < 0)
               return Controller.RightThumbY / 32768.0;
            else
               return Controller.RightThumbY / 32767.0;
         }
      }
   }

   //==========================================================================
   //                          FreeSpacePluginGlobal
   //==========================================================================
   [Global(Name = "xbox360")]
   public class XBox360PluginGlobal {
      private readonly XBox360Plugin Device;

      //-----------------------------------------------------------------------
      public XBox360PluginGlobal(XBox360Plugin plugin) {
         Device = plugin;
      }

      //-----------------------------------------------------------------------
      public bool a {
         get { return Device.A; }
      }

      //-----------------------------------------------------------------------
      public bool b {
         get { return Device.B; }
      }

      //-----------------------------------------------------------------------
      public bool x {
         get { return Device.X; }
      }

      //-----------------------------------------------------------------------
      public bool y {
         get { return Device.Y; }
      }

      //-----------------------------------------------------------------------
      public bool leftShoulder {
         get { return Device.LeftShoulder; }
      }

      //-----------------------------------------------------------------------
      public bool rightShoulder {
         get { return Device.RightShoulder; }
      }

      //-----------------------------------------------------------------------
      public bool start {
         get { return Device.StartBtn; }
      }

      //-----------------------------------------------------------------------
      public bool back {
         get { return Device.Back; }
      }

      //-----------------------------------------------------------------------
      public bool up {
         get { return Device.Up; }
      }

      //-----------------------------------------------------------------------
      public bool down {
         get { return Device.Down; }
      }

      //-----------------------------------------------------------------------
      public bool left {
         get { return Device.Left; }
      }

      //-----------------------------------------------------------------------
      public bool right {
         get { return Device.Right; }
      }

      //-----------------------------------------------------------------------
      public double leftTrigger {
         get { return Device.LeftTrigger; }
      }

      //-----------------------------------------------------------------------
      public double rightTrigger {
         get { return Device.RightTrigger; }
      }

      //-----------------------------------------------------------------------
      public bool leftThumb {
         get { return Device.LeftThumb; }
      }

      //-----------------------------------------------------------------------
      public double leftStickX {
         get { return Device.LeftStickX; }
      }

      //-----------------------------------------------------------------------
      public double leftStickY {
         get { return Device.LeftStickY; }
      }

      //-----------------------------------------------------------------------
      public bool rightThumb {
         get { return Device.RightThumb; }
      }

      //-----------------------------------------------------------------------
      public double rightStickX {
         get { return Device.RightStickX; }
      }

      //-----------------------------------------------------------------------
      public double rightStickY {
         get { return Device.RightStickY; }
      }
   }
}
