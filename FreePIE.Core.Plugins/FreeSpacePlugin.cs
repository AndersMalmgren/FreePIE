using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreePIE.Core.Contracts;
//using HillcrestLabs.Freespace;
using System.Runtime.InteropServices;
using System.Threading;

namespace FreePIE.Core.Plugins {
   
   //==========================================================================
   //                           PFS
   //==========================================================================
   static class PFS {
      // PieFreeSpace is a small proxy dll between the freespacelib interface and
      // FreePIE, instead of porting or interfacing the entire freespacelib
      // It is not thread safe so the caller must handle synchronization

      
      /** Success (no error) */
      public const int FREESPACE_SUCCESS = 0;

      /** Input/output error */
      public const int FREESPACE_ERROR_IO = -1;

      /** Access denied (insufficient permissions) */
      public const int FREESPACE_ERROR_ACCESS = -3;

      /** No such device (it may have been disconnected) */
      public const int FREESPACE_ERROR_NO_DEVICE = -4;

      /** Entity not found */
      public const int FREESPACE_ERROR_NOT_FOUND = -5;

      /** Resource busy */
      public const int FREESPACE_ERROR_BUSY = -6;

      /** Operation timed out */
      public const int FREESPACE_ERROR_TIMEOUT = -7;

      /** Pipe error */
      public const int FREESPACE_ERROR_PIPE = -9;

      /** System call interrupted (perhaps due to signal) */
      public const int FREESPACE_ERROR_INTERRUPTED = -10;

      /** Out of memory */
      public const int FREESPACE_ERROR_OUT_OF_MEMORY = -11;

      /** Amount to send was larger than the max */
      public const int FREESPACE_ERROR_SEND_TOO_LARGE = -20;

      /** Invalid or uninitialized device handle */
      public const int FREESPACE_ERROR_INVALID_DEVICE = -21;

      /** Receive buffer was too small */
      public const int FREESPACE_ERROR_RECEIVE_BUFFER_TOO_SMALL = -22;

      /** Unknown error when trying to create or start a thread */
      public const int FREESPACE_ERROR_COULD_NOT_CREATE_THREAD = -23;

      /** Buffer was too small */
      public const int FREESPACE_ERROR_BUFFER_TOO_SMALL = -24;

      /** No data was received */
      public const int FREESPACE_ERROR_NO_DATA = -25;

      /** No data was received */
      public const int FREESPACE_ERROR_MALFORMED_MESSAGE = -26;

	   /** Invalid HID protocol version */
	   public const int FREESPACE_ERROR_INVALID_HID_PROTOCOL_VERSION = -27;

      /** Any uncategorized or unplanned error */
      public const int FREESPACE_ERROR_UNEXPECTED = -98;

      [DllImport("piefreespace.dll", SetLastError = false, EntryPoint = "PFS_Connect", CallingConvention = CallingConvention.Cdecl)]
      public static extern int Connect();

      [DllImport("piefreespace.dll", SetLastError = false, EntryPoint = "PFS_Close", CallingConvention = CallingConvention.Cdecl)]
      public static extern void Close();

      [DllImport("piefreespace.dll", SetLastError = false, EntryPoint = "PFS_GetOrientation", CallingConvention = CallingConvention.Cdecl)]
      public static extern int GetOrientation(out float yaw, out float pitch, out float roll);
   }
   
   //==========================================================================
   //                          FreeSpacePlugin
   //==========================================================================
   [LuaGlobalType(Type = typeof(FreeSpacePluginGlobal))]
   public class FreeSpacePlugin : Plugin {

      Thread PollThread = null;
      Object PollThreadLock = new Object();
      bool Stopped = false;
      
      float YawSample = 0;
      float ContinuousYaw = 0;
      float PitchSample;
      float RollSample;
      public bool ContinousYawMode { get; set; }

      //-----------------------------------------------------------------------
      public FreeSpacePlugin() {
         ContinousYawMode = false;
      }

      //----------------------------------------------------------------------- 
      public override object CreateGlobal() {
          return new FreeSpacePluginGlobal(this);
      }

      //-----------------------------------------------------------------------
      public override Action Start() {

         int err = PFS.Connect();
         
         if (err == 0) {
            // Grab the first sample just to make sure everything is linked 
            // up properly and to initialize the values
            err = GetSamples();
            if (err == 0 || err == PFS.FREESPACE_ERROR_TIMEOUT) {
               return RunSensorPoll;  // start the poll thread
            }
            else
               throw new Exception("Failed to initialize the FreeSpace tracker with error " + err);
         }
         else {
            throw new Exception("Failed to connect to FreeSpace tracker with error " + err);
         }
      }

      //-----------------------------------------------------------------------
      public override void Stop() {
 
         Thread thread = PollThread;
         if (thread != null && !Stopped) {
            Stopped = true;
            // Block until the polling thread is dead.
            
            //thread.Join();

            // Unfortunately I can't join the thread since FreePIE keeps it
            // hostage for a long time, so I have to lock on a different semaphore
            lock (PollThreadLock) {
               // Now I can close the tracker without worrying about synchronization
               PFS.Close();
            }
         }
      }

      //-----------------------------------------------------------------------
      public override string FriendlyName {
         get { return "FreeSpace"; }
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
      int GetSamples() {

         float yaw, pitch, roll;

         int err = PFS.GetOrientation(out yaw, out pitch, out roll);
         if (err == 0) {

            // Float assignments are atomic in C# so I don't have to worry about
            // thread access protection to these samples
            float previous_yaw = YawSample;
            YawSample = yaw;
            PitchSample = pitch;
            RollSample = roll;

            float delta = YawSample - previous_yaw;
            float HALF_CIRCLE = (float)Math.PI;
            if (Math.Abs(delta) > HALF_CIRCLE) {
               // We turned across the discontinuity at 180 degrees so modify the delta to 
               // reflect the probable angular motion
               if (delta > 0)
                  delta -= (2 * HALF_CIRCLE);
               else
                  delta += (2 * HALF_CIRCLE);
            }
            ContinuousYaw += delta;
         }

         return err;         
      }

      //-----------------------------------------------------------------------
      private void RunSensorPoll() {
      
         OnStarted(this, new EventArgs());

         lock (PollThreadLock) {
            PollThread = Thread.CurrentThread;  // store the thread ID for later
         
            Stopped = false;
            int err = 0;
            while (!Stopped && (err == 0 || err == PFS.FREESPACE_ERROR_TIMEOUT)) {
               err = GetSamples();
            }

            PollThread = null;
            if (!Stopped) {
               throw new Exception("Freespace tracker errored with " + err);
            }
         }
      }

      //-----------------------------------------------------------------------
      public double Yaw {
         get {
            double yaw;
            if (ContinousYawMode)
               yaw = ContinuousYaw;
            else
               yaw = YawSample;
               
            return yaw;
         }
      }

      //-----------------------------------------------------------------------
      public double Roll {
         get {
            double roll = RollSample;
            return roll;
         }
      }

      //-----------------------------------------------------------------------
      public double Pitch {
         get {
            double pitch = PitchSample;
            return pitch;
         }
      }
   }

   //==========================================================================
   //                          FreeSpacePluginGlobal
   //==========================================================================
   [LuaGlobal(Name = "freespace")]
   public class FreeSpacePluginGlobal {
      private readonly FreeSpacePlugin Device;

      //-----------------------------------------------------------------------
      public FreeSpacePluginGlobal(FreeSpacePlugin plugin) {
         Device = plugin;
      }

      //-----------------------------------------------------------------------
      public bool ContinuousYawMode {
         get { return Device.ContinousYawMode; }
         set { Device.ContinousYawMode = value; }
      }

      //-----------------------------------------------------------------------
      public double Yaw {
         get { return Device.Yaw; }
      }

      //-----------------------------------------------------------------------
      public double Pitch {
         get { return Device.Pitch; }
      }

      //-----------------------------------------------------------------------
      public double Roll {
         get { return Device.Roll; }
      }
   }
}
