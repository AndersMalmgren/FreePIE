using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreePIE.Core.Contracts;
using System.Net;
using System.Net.Sockets;

namespace FreePIE.Core.Plugins {

   //==========================================================================
   //                          ApplePlugin
   //==========================================================================
   public abstract class ApplePlugin : Plugin {

      bool Stopped = false;
      int UdpPort = 10552;
      UdpClient UdpSock = null;

      bool Degrees = false;
      double DataUnitScale = 1.0;
      bool ContinousYawMode = false;

      int PitchIndex = 4;
      int RollIndex = 5;
      int YawIndex = 6;
      
      double YawSample;
      int YawPeriod = 0;      // the number of full yaw rotations
      double PitchSample;
      double RollSample;

      DateTime PollTime = DateTime.Now;

      //-----------------------------------------------------------------------
      public ApplePlugin() {
         EnableDegrees(true);
      }

      //-----------------------------------------------------------------------
      public override Action Start() {
         
         return RunSensorPoll;
      }

      //-----------------------------------------------------------------------
      public void EnableDegrees(bool val) {
         Degrees = val;

         if (Degrees)
            DataUnitScale = 180.0 / Math.PI;
         else
            DataUnitScale = 1.0;
      }

      //-----------------------------------------------------------------------
      public override void Stop() {
         Stopped = true;

         // This will cause the blocking read to kick out an exception and complete
         if (UdpSock != null)
            UdpSock.Close();
      }

      //-----------------------------------------------------------------------
      public override bool GetProperty(int index, IPluginProperty property) {
         
         if (index == 0) {
            property.Name = "UDPPort";
            property.Caption = "UDP Port";
            property.DefaultValue = 10552;
            property.HelpText = "UDP Port number that Sensor Data App transmits on (default 10552)";
            return true;
         }
         else
            return false;
      }

      //-----------------------------------------------------------------------
      public override bool SetProperties(Dictionary<string, object> properties) {

         UdpPort = (int)properties["UDPPort"];
         return true;
      }

      //-----------------------------------------------------------------------
      public override void DoBeforeNextExecute() {
         //This method will be executed each iteration of the script
         
      }

      //-----------------------------------------------------------------------
      private void RunSensorPoll() {
       
         try {
            IPEndPoint peer = new IPEndPoint(IPAddress.Any, 0);
            UdpSock = new UdpClient(UdpPort);

            char[] delims = new char[] {','};
         
            Stopped = false;
            OnStarted(this, new EventArgs());
            DateTime start = DateTime.Now;
            while (!Stopped) {
               
               Byte[] bytes = UdpSock.Receive(ref peer);
               int len = bytes.Length;
               if (bytes[len-2] == '\r')  // remove the \r\n
                  len -= 2;

               string data = Encoding.ASCII.GetString(bytes, 0, len);
               String[] fields = data.Split(delims);
               if (fields[0] == "Timestamp") {
                  // The first message is a field definition list
                  // Parse it to determine which data is present and where it is going to be located
                  for (int i=1; i<fields.Length; i++) {
                     // It looksl like there are bugs below but there are not.
                     // The iPhone app seems to orient phone with the long axis running east/west
                     // while I assume the phone should have the long axis running north/south
                     // This difference in perspective, reverses the definition of pitch and roll
                     if (fields[i] == "Roll")
                        PitchIndex = i;
                     else if (fields[i] == "Pitch")
                        RollIndex = i;
                     else if (fields[i] == "Yaw")
                        YawIndex = i;
                     YawPeriod = 0;
                  }
               }
               else {
                  
                  RollSample = Double.Parse(fields[RollIndex]);
                  PitchSample = Double.Parse(fields[PitchIndex]);
                  double previous_yaw = YawSample;
                  YawSample = Double.Parse(fields[YawIndex]);

                  //DateTime now = DateTime.Now;
                  //TimeSpan span = now - start;
                  //System.Console.WriteLine(span.TotalMilliseconds + " sampled yaw = " + YawSample);
                  //start = now;

                  double delta = YawSample - previous_yaw;
                  double HALF_CIRCLE = Math.PI;
                  if (Math.Abs(delta) > HALF_CIRCLE) {
                     // We turned across the discontinuity at 180 degrees so modify the delta to 
                     // reflect the probable angular motion
                     if (delta > 0)
                        YawPeriod--;
                     else
                        YawPeriod++;
                  }
               }
            }
         }
         catch (Exception err) {
            if (err.Message == "A blocking operation was interrupted by a call to WSACancelBlockingCall")
               ;  // this is a graceful shutdown initated by Stop()
            else
               System.Console.WriteLine(err);
         }
      }

      //-----------------------------------------------------------------------
      public void SetContinousYawMode(bool continous) {
         ContinousYawMode = continous;
      }

      //-----------------------------------------------------------------------
      public double Yaw {
         get {

            double yaw;
            if (ContinousYawMode)
               yaw = ((YawPeriod * 2 * Math.PI) + YawSample) * DataUnitScale;
            else
               yaw = YawSample * DataUnitScale;

               
            return yaw;
         }
      }

      //-----------------------------------------------------------------------
      public double Roll {
         get {
            double roll = RollSample * DataUnitScale;
            return roll;
         }
      }

      //-----------------------------------------------------------------------
      public double Pitch {
         get {
            double pitch = PitchSample * DataUnitScale;
            return pitch;
         }
      }
   }

   //==========================================================================
   //                          AppleGlobal
   //==========================================================================
   public class ApplePluginGlobal {
      private readonly ApplePlugin Device;

      //-----------------------------------------------------------------------
      public ApplePluginGlobal(ApplePlugin plugin) {
         Device = plugin;
      }

      //-----------------------------------------------------------------------
      public void setDegrees(bool val) {
         Device.EnableDegrees(val);
      }

      //-----------------------------------------------------------------------
      public void setContinuousYawMode(bool continuous) {
         Device.SetContinousYawMode(continuous);
      }

      //-----------------------------------------------------------------------
      public double getYaw() {
         return Device.Yaw;
      }

      //-----------------------------------------------------------------------
      public double getPitch() {
         return Device.Pitch;
      }

      //-----------------------------------------------------------------------
      public double getRoll() {
         return Device.Roll;
      }
   }

   //==========================================================================
   //                          iPhonePlugin
   //==========================================================================
   [LuaGlobalType(Type = typeof(iPhonePluginGlobal))]
   public class iPhonePlugin : ApplePlugin {

      //----------------------------------------------------------------------- 
      public override object CreateGlobal() {
          return new iPhonePluginGlobal(this);
      }

      //-----------------------------------------------------------------------
      public override string FriendlyName {
         get { return "iPhone"; }
      }
   }
   
   //==========================================================================
   //                          iPhonePluginGlobal
   //==========================================================================
   [LuaGlobal(Name = "iPhone")]
   public class iPhonePluginGlobal : ApplePluginGlobal {

      //-----------------------------------------------------------------------
      public iPhonePluginGlobal(iPhonePlugin plugin) : base(plugin) {
         
      }
   }

   //==========================================================================
   //                          iPodPlugin
   //==========================================================================
   [LuaGlobalType(Type = typeof(iPodPluginGlobal))]
   public class iPodPlugin : ApplePlugin {

      //----------------------------------------------------------------------- 
      public override object CreateGlobal() {
          return new iPodPluginGlobal(this);
      }

      //-----------------------------------------------------------------------
      public override string FriendlyName {
         get { return "iPod"; }
      }
   }
   
   //==========================================================================
   //                          iPodPluginGlobal
   //==========================================================================
   [LuaGlobal(Name = "iPod")]
   public class iPodPluginGlobal : ApplePluginGlobal {

      //-----------------------------------------------------------------------
      public iPodPluginGlobal(iPodPlugin plugin) : base(plugin) {
         
      }
   }

   //==========================================================================
   //                          iPadPlugin
   //==========================================================================
   [LuaGlobalType(Type = typeof(iPadPluginGlobal))]
   public class iPadPlugin : ApplePlugin {

      //----------------------------------------------------------------------- 
      public override object CreateGlobal() {
          return new iPadPluginGlobal(this);
      }

      //-----------------------------------------------------------------------
      public override string FriendlyName {
         get { return "iPad"; }
      }
   }
   
   //==========================================================================
   //                          iPadPluginGlobal
   //==========================================================================
   [LuaGlobal(Name = "iPad")]
   public class iPadPluginGlobal : ApplePluginGlobal {

      //-----------------------------------------------------------------------
      public iPadPluginGlobal(iPadPlugin plugin) : base(plugin) {
         
      }
   }
}
