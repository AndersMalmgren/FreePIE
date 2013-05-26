using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using FreePIE.Core.Contracts;

namespace FreePIE.Core.Plugins {

   public static class Sixense {
      
      public struct ControllerData {
         public float pos_x;
         public float pos_y;
         public float pos_z;
         public float rot_mat_00;
         public float rot_mat_01;
         public float rot_mat_02;
         public float rot_mat_10;
         public float rot_mat_11;
         public float rot_mat_12;
         public float rot_mat_20;
         public float rot_mat_21;
         public float rot_mat_22;
         public float joystick_x;
         public float joystick_y;
         public float trigger;
         public uint buttons;
         public byte sequence_number;
         public float rot_quat0;
         public float rot_quat1;
         public float rot_quat2;
         public float rot_quat3;
         public ushort firmware_revision;
         public ushort hardware_revision;
         public ushort packet_type;
         public ushort magnetic_frequency;
         public int enabled;
         public int controller_index;
         public byte is_docked;
         public byte which_hand;
         public byte hemi_tracking_enabled;
      }

      public struct ControllerAngles {
         public float yaw;
         public float pitch;
         public float roll;
      }

      public const int SUCCESS = 0;
      public const int FAILURE = 1;

      public const int BUTTON_BUMPER   = (0x01<<7);
      public const int BUTTON_JOYSTICK = (0x01<<8);
      public const int BUTTON_1        = (0x01<<5);
      public const int BUTTON_2        = (0x01<<6);
      public const int BUTTON_3        = (0x01<<3);
      public const int BUTTON_4        = (0x01<<4);
      public const int BUTTON_START    = (0x01<<0);


      [DllImport("sixense.dll", SetLastError = false, EntryPoint = "sixenseInit", CallingConvention = CallingConvention.Cdecl)]
      public static extern int Init();

      [DllImport("sixense.dll", SetLastError = false, EntryPoint = "sixenseExit", CallingConvention = CallingConvention.Cdecl)]
      public static extern int Exit();

      [DllImport("sixense.dll", SetLastError = false, EntryPoint = "sixenseIsBaseConnected", CallingConvention = CallingConvention.Cdecl)]
      public static extern int IsBaseConnected(int base_num);

      [DllImport("sixense.dll", SetLastError = false, EntryPoint = "sixenseSetActiveBase", CallingConvention = CallingConvention.Cdecl)]
      public static extern int SetActiveBase(int base_num);

      [DllImport("sixense.dll", SetLastError = false, EntryPoint = "sixenseGetNewestData", CallingConvention = CallingConvention.Cdecl)]
      public static extern int GetNewestData(int which, out ControllerData data);
   }
      
   //==========================================================================
   //                          HydraPlugin
   //==========================================================================
   [GlobalType(Type = typeof(HydraPluginGlobal), IsIndexed = true)]
   public class HydraPlugin : Plugin {
      
      public Sixense.ControllerData[] Controller;
      public Sixense.ControllerAngles[] Angles;

      //----------------------------------------------------------------------- 
      public override object CreateGlobal() {

         var globals = new HydraPluginGlobal[2];
         globals[0] = new HydraPluginGlobal(0, this);
         globals[1] = new HydraPluginGlobal(1, this);
         return globals;
      }

      //-----------------------------------------------------------------------
      public override Action Start() {
         int r = Sixense.Init();
         if (r == Sixense.SUCCESS) {

            int attempts = 0;
            int base_found = 0;
            while (base_found == 0 && attempts < 2) {
               Thread.Sleep(1000);
               base_found = Sixense.IsBaseConnected(0);
            }

            if (base_found == 0) {
               Sixense.Exit();
               throw new Exception("Hydra not attached");
            }

            Controller = new Sixense.ControllerData[2];
            Controller[0] = new Sixense.ControllerData();
            Controller[1] = new Sixense.ControllerData();

            Angles = new Sixense.ControllerAngles[2];
            Angles[0] = new Sixense.ControllerAngles();
            Angles[1] = new Sixense.ControllerAngles();

            r = Sixense.SetActiveBase(0);
            return null;
         }
         else
            throw new Exception("Failed to initialize Hydra");
      }

      //-----------------------------------------------------------------------
      public override void Stop() {
         Sixense.Exit();
      }

      //-----------------------------------------------------------------------
      public override string FriendlyName {
         get { return "Razer Hydra"; }
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
         for (int i=0; i<2; i++) {
            Sixense.GetNewestData(i, out Controller[i]);

            // Convert quaternions to clock-wise Euler angles
            float q0 = Controller[i].rot_quat0;
            float q1 = Controller[i].rot_quat1;
            float q2 = Controller[i].rot_quat2;
            float q3 = Controller[i].rot_quat3;

            Angles[i].yaw   = -(float)Math.Atan2(2*q1*q3 - 2*q0*q2, 1 - 2*q1*q1 - 2*q2*q2);
            Angles[i].pitch = (float)Math.Atan2(2*q0*q3 - 2*q1*q2, 1 - 2*q0*q0 - 2*q2*q2);
            Angles[i].roll  = -(float)Math.Asin(2*q0*q1 + 2*q2*q3);

            // !!! Roll seems messed up.  At +-90 degree roll it starts to roll back down to 0 and
            // pitch simultaneously !!!  I don't know if this is a bug with the Hydra or with the 
            // calculations of my Euler angles
         }
      }
   }

   //==========================================================================
   //                          FreeSpacePluginGlobal
   //==========================================================================
   [Global(Name = "hydra")]
   public class HydraPluginGlobal {

      private readonly HydraPlugin Hydra;
      private readonly int Index;
     
      //-----------------------------------------------------------------------
      public HydraPluginGlobal(int index, HydraPlugin plugin) {
         Hydra = plugin;
         Index = index;
      }
      
      //-----------------------------------------------------------------------
      public char side {
         get {
            switch (Hydra.Controller[Index].which_hand) {
               case 1: return 'L';
               case 2: return 'R';
               default: return '?';
            }
         }
      }

      //-----------------------------------------------------------------------
      public bool one {
         get {
            return (Hydra.Controller[Index].buttons & Sixense.BUTTON_1) != 0;
         }
      }
      //-----------------------------------------------------------------------
      public bool two {
         get {
            return (Hydra.Controller[Index].buttons & Sixense.BUTTON_2) != 0;
         }
      }

      //-----------------------------------------------------------------------
      public bool three {
         get {
            return (Hydra.Controller[Index].buttons & Sixense.BUTTON_3) != 0;
         }
      }

      //-----------------------------------------------------------------------
      public bool four {
         get {
            return (Hydra.Controller[Index].buttons & Sixense.BUTTON_4) != 0;
         }
      }

      //-----------------------------------------------------------------------
      public bool start {
         get {
            return (Hydra.Controller[Index].buttons & Sixense.BUTTON_START) != 0;
         }
      }

      //-----------------------------------------------------------------------
      public bool bumper {
         get {
            return (Hydra.Controller[Index].buttons & Sixense.BUTTON_BUMPER) != 0;
         }
      }

      //-----------------------------------------------------------------------
      public float trigger {
         get { return Hydra.Controller[Index].trigger; }
      }

      //-----------------------------------------------------------------------
      public bool joybutton {
         get {
            return (Hydra.Controller[Index].buttons & Sixense.BUTTON_JOYSTICK) != 0;
         }
      }

      //-----------------------------------------------------------------------
      public float joyx {
         get {
            return Hydra.Controller[Index].joystick_x;
         }
      }

      //-----------------------------------------------------------------------
      public float joyy {
         get {
            return Hydra.Controller[Index].joystick_y;
         }
      }

      //-----------------------------------------------------------------------
      public float x {
         get {
            return Hydra.Controller[Index].pos_x;
         }
      }

      //-----------------------------------------------------------------------
      public float y {
         get {
            return Hydra.Controller[Index].pos_y;
         }
      }

      //-----------------------------------------------------------------------
      public float z {
         get {
            return Hydra.Controller[Index].pos_z;
         }
      }

      //-----------------------------------------------------------------------
      public float yaw {
         get {
            return Hydra.Angles[Index].yaw;
         }
      }

      //-----------------------------------------------------------------------
      public float pitch {
         get {
            return Hydra.Angles[Index].pitch;
         }
      }

      //-----------------------------------------------------------------------
      public float roll {
         get {
            return Hydra.Angles[Index].roll;
         }
      }

      //-----------------------------------------------------------------------
      public float q0 {
         get {
            return Hydra.Controller[Index].rot_quat0;
         }
      }

      //-----------------------------------------------------------------------
      public float q1 {
         get {
            return Hydra.Controller[Index].rot_quat1;
         }
      }

      //-----------------------------------------------------------------------
      public float q2 {
         get {
            return Hydra.Controller[Index].rot_quat2;
         }
      }

      //-----------------------------------------------------------------------
      public float q3 {
         get {
            return Hydra.Controller[Index].rot_quat3;
         }
      }
   }
}
