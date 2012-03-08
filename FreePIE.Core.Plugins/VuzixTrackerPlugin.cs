using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreePIE.Core.Contracts;
using System.Runtime.InteropServices;

namespace FreePIE.Core.Plugins {

   //==========================================================================
   //                          Vuzix (iWearDrv interface)
   //==========================================================================
   static class VuzixAPI {
   
      [DllImport("iWearDrv.dll", SetLastError = false, EntryPoint = "IWROpenTracker", CallingConvention = CallingConvention.Cdecl)]
      public static extern int IWROpenTracker();

      [DllImport("iWearDrv.dll", SetLastError = false, EntryPoint = "IWRCloseTracker", CallingConvention = CallingConvention.Cdecl)]
      public static extern void IWRCloseTracker();

      //[DllImport("iWearDrv.dll", SetLastError = false, EntryPoint = "IWRGetProductID")]
      //public static extern ushort IWRGetProductID();

      [DllImport("iWearDrv.dll", SetLastError = false, EntryPoint = "IWRGetTracking", CallingConvention = CallingConvention.Cdecl)]
      public static extern int IWRGetTracking(out int yaw, out int pitch, out int roll);

      [DllImport("iWearDrv.dll", SetLastError = false, EntryPoint = "IWRGet6DTracking", CallingConvention = CallingConvention.Cdecl)]
      public static extern int IWRGet6DTracking(out int yaw, out int pitch, out int roll, out int x, out int y, out int z);

      [DllImport("iWearDrv.dll", SetLastError = false, EntryPoint = "IWRSetFilterState", CallingConvention = CallingConvention.Cdecl)]
      public static extern void IWRSetFilterState(Boolean on);
   }

   //==========================================================================
   [LuaGlobalEnum]
   public enum VuzixDataUnit {
      RAW = 0,
      DEGREES = 1,
      RADIANS = 2
   }

   //==========================================================================
   //                          VuzixTrackerPlugin
   //==========================================================================
   [LuaGlobalType(Type = typeof(VuzixTrackerPluginGlobal))]
   public class VuzixTrackerPlugin : Plugin {

      bool Sampled = false;
      int YawSample;
      int YawSpins = 0;
      int PitchSample;
      int RollSample;
      int XSample;
      int YSample;
      int ZSample;
      bool ContinousYaw = false;
      double DataModeScale;

      //-----------------------------------------------------------------------
      public VuzixTrackerPlugin() {
         SetDataMode(VuzixDataUnit.DEGREES);
      }

      //----------------------------------------------------------------------- 
      public override object CreateGlobal() {
          return new VuzixTrackerPluginGlobal(this);
      }

      //-----------------------------------------------------------------------
      public override Action Start() {
         // This method is called just before script starts
         int err = VuzixAPI.IWROpenTracker();
         if (err == 0) {
            // Turn on Vuzix signal filtering
            // TODO: I should expose this to allow scripts to do their own filtering
            VuzixAPI.IWRSetFilterState(true); 

            // Grab a single test sample just to make sure everything is linked up properly
            SampleVuzixTracker();
            
         }
         else {
            throw new Exception("Failed to connect to Vuzix Tracker");
         }

         return null;
      }

      //-----------------------------------------------------------------------
      public override void Stop() {
         VuzixAPI.IWRCloseTracker();
      }

      //-----------------------------------------------------------------------
      public override string FriendlyName {
         get { return "Vuzix"; }
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
         Sampled = false;
      }

      //-----------------------------------------------------------------------
      public void SetDataMode(VuzixDataUnit mode) {
         
         switch (mode) {
            case VuzixDataUnit.RAW:
               DataModeScale = 1.0;
               break;

            case VuzixDataUnit.DEGREES:
               DataModeScale = 180.0 / 32768.0;
               break;

            case VuzixDataUnit.RADIANS:
               DataModeScale = Math.PI / 32768.0;
               break;

            default:
               DataModeScale = 1.0;
               break;
         }
      }

      //-----------------------------------------------------------------------
      public void SetContinousYaw(bool continous) {
// TODO: finish continuous yaw
      }

      //-----------------------------------------------------------------------
      void SampleVuzixTracker() {
         VuzixAPI.IWRGet6DTracking(out YawSample, out PitchSample, out RollSample, out XSample, out YSample, out ZSample);
         Sampled = true;
      }

      //-----------------------------------------------------------------------
      public double Yaw {
         get {
            if (!Sampled)
               SampleVuzixTracker();

            double yaw = YawSample * DataModeScale;
            return yaw;
         }
      }

      //-----------------------------------------------------------------------
      public double Roll {
         get {
            if (!Sampled)
               SampleVuzixTracker();

            double roll = RollSample * DataModeScale;
            return roll;
         }
      }

      //-----------------------------------------------------------------------
      public double Pitch {
         get {
            if (!Sampled)
               SampleVuzixTracker();

            double pitch = PitchSample * DataModeScale;
            return pitch;
         }
      }

      //-----------------------------------------------------------------------
      public int X {
         get {
            if (!Sampled)
               SampleVuzixTracker();

            return XSample;
         }
      }

      //-----------------------------------------------------------------------
      public int Y {
         get {
            if (!Sampled)
               SampleVuzixTracker();

            return YSample;
         }
      }

      //-----------------------------------------------------------------------
      public int Z {
         get {
            if (!Sampled)
               SampleVuzixTracker();

            return ZSample;
         }
      }
   }

   //==========================================================================
   //                          VuzixTrackerPluginGlobal
   //==========================================================================
   [LuaGlobal(Name = "vuzix")]
   public class VuzixTrackerPluginGlobal {
      private readonly VuzixTrackerPlugin Vuzix;

      //-----------------------------------------------------------------------
      public VuzixTrackerPluginGlobal(VuzixTrackerPlugin plugin) {
         Vuzix = plugin;
      }

      //-----------------------------------------------------------------------
      public void setDataMode(int mode) {
         Vuzix.SetDataMode((VuzixDataUnit)mode);
      }

      //-----------------------------------------------------------------------
      public void setContinuousYaw(int continuous) {
         Vuzix.SetContinousYaw((continuous != 0));
      }

      //-----------------------------------------------------------------------
      public double getYaw() {
         return Vuzix.Yaw;
      }

      //-----------------------------------------------------------------------
      public double getPitch() {
         return Vuzix.Pitch;
      }

      //-----------------------------------------------------------------------
      public double getRoll() {
         return Vuzix.Roll;
      }

      //-----------------------------------------------------------------------
      public double getX() {
         return Vuzix.X;
      }

      //-----------------------------------------------------------------------
      public double getY() {
         return Vuzix.Y;
      }

      //-----------------------------------------------------------------------
      public double getZ() {
         return Vuzix.Z;
      }
   }
}
