using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreePIE.Core.Contracts;
using System.Runtime.InteropServices;
using SlimDX;
using SlimDX.XInput;

namespace FreePIE.Core.Plugins {
      
   //==========================================================================
   //                          XBox360Plugin
   //==========================================================================
   [GlobalType(Type = typeof(XBox360PluginGlobal))]
   public class XBox360Plugin : Plugin {

      Controller XBoxController = new Controller(0);
      Gamepad Controller;
      
      //-----------------------------------------------------------------------
      public XBox360Plugin() {
      }

      //----------------------------------------------------------------------- 
      public override object CreateGlobal() {
         return new XBox360PluginGlobal(this);
      }

      //-----------------------------------------------------------------------
      public override Action Start() {
         if (XBoxController.IsConnected)
            return null;
         else
            throw new Exception("XBox360 Controller not connected");
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
         Controller = XBoxController.GetState().Gamepad;
      }

      //-----------------------------------------------------------------------
      public bool A {
         get {
            return ((Controller.Buttons & GamepadButtonFlags.A) != 0);
         }
      }

      //-----------------------------------------------------------------------
      public bool B {
         get {
            return ((Controller.Buttons & GamepadButtonFlags.B) != 0);
         }
      }

      //-----------------------------------------------------------------------
      public bool X {
         get {
            return ((Controller.Buttons & GamepadButtonFlags.X) != 0);
         }
      }

      //-----------------------------------------------------------------------
      public bool Y {
         get {
            return ((Controller.Buttons & GamepadButtonFlags.Y) != 0);
         }
      }

      //-----------------------------------------------------------------------
      public bool LeftShoulder {
         get {
            return ((Controller.Buttons & GamepadButtonFlags.LeftShoulder) != 0);
         }
      }

      //-----------------------------------------------------------------------
      public bool RightShoulder {
         get {
            return ((Controller.Buttons & GamepadButtonFlags.RightShoulder) != 0);
         }
      }

      //-----------------------------------------------------------------------
      public bool StartBtn {
         get {
            return ((Controller.Buttons & GamepadButtonFlags.Start) != 0);
         }
      }

      //-----------------------------------------------------------------------
      public bool Back {
         get {
            return ((Controller.Buttons & GamepadButtonFlags.Back) != 0);
         }
      }

      //-----------------------------------------------------------------------
      public bool Up {
         get {
            return ((Controller.Buttons & GamepadButtonFlags.DPadUp) != 0);
         }
      }

      //-----------------------------------------------------------------------
      public bool Down {
         get {
            return ((Controller.Buttons & GamepadButtonFlags.DPadDown) != 0);
         }
      }

      //-----------------------------------------------------------------------
      public bool Left {
         get {
            return ((Controller.Buttons & GamepadButtonFlags.DPadLeft) != 0);
         }
      }

      //-----------------------------------------------------------------------
      public bool Right {
         get {
            return ((Controller.Buttons & GamepadButtonFlags.DPadRight) != 0);
         }
      }

      //-----------------------------------------------------------------------
      public double LeftTrigger {
         get {
            return (Controller.LeftTrigger / 255.0);
         }
      }

      //-----------------------------------------------------------------------
      public double RightTrigger {
         get {
            return (Controller.RightTrigger / 255.0);
         }
      }

      //-----------------------------------------------------------------------
      public bool LeftThumb {
         get {
            return ((Controller.Buttons & GamepadButtonFlags.LeftThumb) != 0);
         }
      }

      //-----------------------------------------------------------------------
      public double LeftStickX {
         // Return -1 to +1
         get {
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
            if (Controller.LeftThumbY < 0)
               return Controller.LeftThumbY / 32768.0;
            else
               return Controller.LeftThumbY / 32767.0;
         }
      }

      //-----------------------------------------------------------------------
      public bool RightThumb {
         get {
            return ((Controller.Buttons & GamepadButtonFlags.RightThumb) != 0);
         }
      }

      //-----------------------------------------------------------------------
      public double RightStickX {
         // Return -1 to +1
         get {
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
      public bool A {
         get { return Device.A; }
      }

      //-----------------------------------------------------------------------
      public bool B {
         get { return Device.B; }
      }

      //-----------------------------------------------------------------------
      public bool X {
         get { return Device.X; }
      }

      //-----------------------------------------------------------------------
      public bool Y {
         get { return Device.Y; }
      }

      //-----------------------------------------------------------------------
      public bool LeftShoulder {
         get { return Device.LeftShoulder; }
      }

      //-----------------------------------------------------------------------
      public bool RightShoulder {
         get { return Device.RightShoulder; }
      }

      //-----------------------------------------------------------------------
      public bool Start {
         get { return Device.StartBtn; }
      }

      //-----------------------------------------------------------------------
      public bool Back {
         get { return Device.Back; }
      }

      //-----------------------------------------------------------------------
      public bool Up {
         get { return Device.Up; }
      }

      //-----------------------------------------------------------------------
      public bool Down {
         get { return Device.Down; }
      }

      //-----------------------------------------------------------------------
      public bool Left {
         get { return Device.Left; }
      }

      //-----------------------------------------------------------------------
      public bool Right {
         get { return Device.Right; }
      }

      //-----------------------------------------------------------------------
      public double LeftTrigger {
         get { return Device.LeftTrigger; }
      }

      //-----------------------------------------------------------------------
      public double RightTrigger {
         get { return Device.RightTrigger; }
      }

      //-----------------------------------------------------------------------
      public bool LeftThumb {
         get { return Device.LeftThumb; }
      }

      //-----------------------------------------------------------------------
      public double LeftStickX {
         get { return Device.LeftStickX; }
      }

      //-----------------------------------------------------------------------
      public double LeftStickY {
         get { return Device.LeftStickY; }
      }

      //-----------------------------------------------------------------------
      public bool RightThumb {
         get { return Device.RightThumb; }
      }

      //-----------------------------------------------------------------------
      public double RightStickX {
         get { return Device.RightStickX; }
      }

      //-----------------------------------------------------------------------
      public double RightStickY {
         get { return Device.RightStickY; }
      }
   }
}
