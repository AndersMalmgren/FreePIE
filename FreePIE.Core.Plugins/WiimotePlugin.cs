using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreePIE.Core.Contracts;
using WiimoteLib;

namespace FreePIE.Core.Plugins {
   
   
   
   //==========================================================================
   //                          WiimotePlugin
   //==========================================================================
   [LuaGlobalType(Type = typeof(WiimotePluginGlobal))]
   public class WiimotePlugin : Plugin {

      const int MAX_WIIMOTES = 4;

      WiimoteCollection Motes = null;
      WiimoteLib.ButtonState[] Buttons = null;

      //-----------------------------------------------------------------------
      public WiimotePlugin() {
      }

      //----------------------------------------------------------------------- 
      public override object CreateGlobal() {

         WiimotePluginGlobal[] plugins = new WiimotePluginGlobal[MAX_WIIMOTES];
         for (int i=0; i<MAX_WIIMOTES; i++) {
            plugins[i] = new WiimotePluginGlobal(this, i);
         }

         return plugins;
      }

      //-----------------------------------------------------------------------
      public override Action Start() {
         // This method is called just before script starts

         // Find and connect to all Wiimotes.  Wiimotes must have been 
         // previously attached via a bluetooth manager
         WiimoteCollection motes = new WiimoteCollection();
			motes.FindAllWiimotes();

         if (motes.Count > 0) {

            Buttons = new WiimoteLib.ButtonState[motes.Count];

            // Connect and start each wiimote
            for (int i=0; i<motes.Count; i++) {
               Wiimote wii = motes.ElementAt(i);
               wii.Connect();
               // set the wiimote leds to the wiimote number
               wii.SetLEDs(i+1);   // set the wiimote leds to the wiimote number
               Buttons[i] = wii.WiimoteState.ButtonState;
               wii.WiimoteChanged += OnWiimoteChanged;        // set the event handler
               wii.SetReportType(InputReport.Buttons, false); // start the data callbacks
            }

            Motes = motes;
         }
         else {
            throw new Exception("No Wiimotes found");
         }

         return null;
      }

      //-----------------------------------------------------------------------
      public override void Stop() {
         if (Motes != null) {

            for (int i=0; i<Motes.Count; i++) {
               Wiimote wii = Motes.ElementAt(i);
               wii.SetLEDs(false, false, false, false);
               wii.Disconnect();
            }
            Motes = null;
         }
      }

      //-----------------------------------------------------------------------
      public override string FriendlyName {
         get { return "Wiimote"; }
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
      }

      //-----------------------------------------------------------------------
      void OnWiimoteChanged(object sender, WiimoteChangedEventArgs args) {
     
         if (Motes == null)
            return;

         Wiimote wii = (Wiimote)sender;
         for (int i=0; i<Motes.Count; i++) {
            // Find which Wiimote the data originated from and update the button state
            if (wii == Motes.ElementAt(i)) {
               lock (Buttons) {
                  Buttons[i] = args.WiimoteState.ButtonState;
               }
            }
         }
      }

      //-----------------------------------------------------------------------
      public bool GetUp(int index) {
         lock (Buttons) {
            return Buttons[index].Up;
         }
      }

      //-----------------------------------------------------------------------
      public bool GetDown(int index) {
         lock (Buttons) {
            return Buttons[index].Down;
         }
      }

      //-----------------------------------------------------------------------
      public bool GetLeft(int index) {
         lock (Buttons) {
            return Buttons[index].Left;
         }
      }

      //-----------------------------------------------------------------------
      public bool GetRight(int index) {
         lock (Buttons) {
            return Buttons[index].Right;
         }
      }

      //-----------------------------------------------------------------------
      public bool GetA(int index) {
         lock (Buttons) {
            return Buttons[index].A;
         }
      }

      //-----------------------------------------------------------------------
      public bool GetB(int index) {
         lock (Buttons) {
            return Buttons[index].B;
         }
      }

      //-----------------------------------------------------------------------
      public bool GetMinus(int index) {
         lock (Buttons) {
            return Buttons[index].Minus;
         }
      }

      //-----------------------------------------------------------------------
      public bool GetHome(int index) {
         lock (Buttons) {
            return Buttons[index].Home;
         }
      }

      //-----------------------------------------------------------------------
      public bool GetPlus(int index) {
         lock (Buttons) {
            return Buttons[index].Plus;
         }
      }

      //-----------------------------------------------------------------------
      public bool GetOne(int index) {
         lock (Buttons) {
            return Buttons[index].One;
         }
      }

      //-----------------------------------------------------------------------
      public bool GetTwo(int index) {
         lock (Buttons) {
            return Buttons[index].Two;
         }
      }
   }

   //==========================================================================
   //                          WiimotePluginGlobal
   //==========================================================================
   [LuaGlobal(Name = "wiimote")]
   public class WiimotePluginGlobal {
      private readonly WiimotePlugin Wii;
      private readonly int index;

      //-----------------------------------------------------------------------
      public WiimotePluginGlobal(WiimotePlugin plugin, int index) {
         Wii = plugin;
         this.index = index;
      }

      //-----------------------------------------------------------------------
      public bool getUp() {
         return Wii.GetUp(index);
      }

      //-----------------------------------------------------------------------
      public bool getDown() {
         return Wii.GetDown(index);
      }

      //-----------------------------------------------------------------------
      public bool getLeft() {
         return Wii.GetLeft(index);
      }

      //-----------------------------------------------------------------------
      public bool getRight() {
         return Wii.GetRight(index);
      }

      //-----------------------------------------------------------------------
      public bool getA() {
         return Wii.GetA(index);
      }

      //-----------------------------------------------------------------------
      public bool getB() {
         return Wii.GetB(index);
      }

      //-----------------------------------------------------------------------
      public bool getMinus() {
         return Wii.GetMinus(index);
      }

      //-----------------------------------------------------------------------
      public bool getHome() {
         return Wii.GetHome(index);
      }

      //-----------------------------------------------------------------------
      public bool getPlus() {
         return Wii.GetPlus(index);
      }

      //-----------------------------------------------------------------------
      public bool getOne() {
         return Wii.GetOne(index);
      }

      //-----------------------------------------------------------------------
      public bool getTwo() {
         return Wii.GetTwo(index);
      }
   }
}
