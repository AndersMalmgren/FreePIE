/*
Author: Brant Lewis (brantlew@yahoo.com) - July 2012 

piefreespace (PFS) is a proxy dll that is used to translate complex freespacelib 
calls into a simple API for easy linking by the FreePIE FreeSpacePlugin.  It sets up
the device connection, handles I/O reads, and converts quaternion vectors into
Euler angles.  This simplifies the code in FreePIE instead of implementing and calling
a full C# libfreespace interface

It was developed against libfreespace-0.6 

*/

#include "piefreespace.h"
#include <freespace/freespace.h>
#include <string.h>
#include <math.h>
#include <time.h>
#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
// Windows Header Files:
#include <windows.h>
#include <stdio.h>

FreespaceDeviceId DeviceID = -1;
FreespaceDeviceInfo Device;

//----------------------------------------------------------------------------
BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved)
{
	switch (ul_reason_for_call) {
	   case DLL_PROCESS_ATTACH:
	   case DLL_THREAD_ATTACH:
	   case DLL_THREAD_DETACH:
	   case DLL_PROCESS_DETACH:
		   break;
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
int Connect() {

   // Initialize the freespace library
   int err = freespace_init();
   if (err)
      return err;

   // Look for a single tracker
   int num_devices;
   err = freespace_getDeviceList(&DeviceID, 1, &num_devices);
   if (err)
      return err;

   if (num_devices == 0)
      return FREESPACE_ERROR_NO_DEVICE;

   // Attach to the tracker
   err = freespace_openDevice(DeviceID);
   if (err) {
      DeviceID = -1;
      return err;
   }

   err = freespace_getDeviceInfo(DeviceID, &Device);

   // Flush the message stream
   err = freespace_flush(DeviceID);
   if (err)
      return err;

   // Turn on the orientation message stream
   freespace_message msg;
   memset(&msg, 0, sizeof(msg));

   if (Device.hVer >= 2) {
      // version 2 protocol
      msg.messageType = FREESPACE_MESSAGE_DATAMODECONTROLV2REQUEST;
      msg.dataModeControlV2Request.packetSelect = 3;    // User Frame (orientation)
      msg.dataModeControlV2Request.modeAndStatus = 0;   // operating mode == full motion
   }
   else {
      // version 1 protocol
      msg.messageType = FREESPACE_MESSAGE_DATAMODEREQUEST;
      msg.dataModeRequest.enableUserPosition = 1;
      msg.dataModeRequest.inhibitPowerManager = 1;
   }

   err = freespace_sendMessage(DeviceID, &msg);
   if (err)
      return err;

   // Now the tracker is ready to read
   return 0;
}

//-----------------------------------------------------------------------------
void Close() {

   if (DeviceID >= 0) {
      
      // Shut off the data stream
      freespace_message msg;
      memset(&msg, 0, sizeof(msg));

      if (Device.hVer >= 2) {
         msg.messageType = FREESPACE_MESSAGE_DATAMODECONTROLV2REQUEST;
         msg.dataModeControlV2Request.packetSelect = 0;        // No output
         msg.dataModeControlV2Request.modeAndStatus = 1 << 1;  // operating mode == sleep  
      }
      else {
         msg.messageType = FREESPACE_MESSAGE_DATAMODEREQUEST;
         msg.dataModeRequest.enableUserPosition = 0;
         msg.dataModeRequest.inhibitPowerManager = 0;
      }

      int err = freespace_sendMessage(DeviceID, &msg);
      
      // Detach from the tracker
      freespace_closeDevice(DeviceID);
      DeviceID = -1;
   }

   freespace_exit();
}

//-----------------------------------------------------------------------------
int GetOrientation(float* yaw, float* pitch, float* roll) {

   
   int duration = 0;
   int timeout = 250;       // 1/4 second max time in this function

   long start = clock();

   freespace_message msg;
   while (duration < timeout) {
   
      int err = freespace_readMessage(DeviceID, &msg, timeout - duration);
      if (err == 0) {
         // Check if this is a user frame message.
         if (msg.messageType == FREESPACE_MESSAGE_USERFRAME) {
            // Convert from quaternion to Euler angles

            // Get the quaternion vector
            float w = msg.userFrame.angularPosA;
            float x = msg.userFrame.angularPosB;
            float y = msg.userFrame.angularPosC;
            float z = msg.userFrame.angularPosD;

            // normalize the vector
            float len = sqrtf((w*w) + (x*x) + (y*y) + (z*z));
            w /= len;
            x /= len;
            y /= len;
            z /= len;

            // The Freespace quaternion gives the rotation in terms of
            // rotating the world around the object. We take the conjugate to
            // get the rotation in the object's reference frame.
            w = w;
            x = -x;
            y = -y;
            z = -z;
      
            // Convert to angles in radians
            float m11 = (2.0f * w * w) + (2.0f * x * x) - 1.0f;
            float m12 = (2.0f * x * y) + (2.0f * w * z);
            float m13 = (2.0f * x * z) - (2.0f * w * y);
            float m23 = (2.0f * y * z) + (2.0f * w * x);
            float m33 = (2.0f * w * w) + (2.0f * z * z) - 1.0f;

            *roll = atan2f(m23, m33);
            *pitch = asinf(-m13);
            *yaw = atan2f(m12, m11);
            return 0;   
         }

         // any other message types will just fall through and keep looping until the timeout is reached
      }
      else
         return err;  // return on timeouts or serious errors

      duration += clock() - start;
   }

   return FREESPACE_ERROR_TIMEOUT;  // The function returns gracefully without values
}

//-----------------------------------------------------------------------------
int PFS_Connect() {

   int err = Connect();
   if (err)
      Close();   // Shutdown on error

   return err;
}

//-----------------------------------------------------------------------------
void PFS_Close() {
   Close();
}

//-----------------------------------------------------------------------------
int PFS_GetOrientation(float* yaw, float* pitch, float* roll) {
   
   int err = GetOrientation(yaw, pitch, roll);
   if (err && (err != FREESPACE_ERROR_TIMEOUT))
      Close();  // Shutdown on true error

   return err;
}

//-----------------------------------------------------------------------------
#ifndef _USRDLL

// This section is strictly for development and testing as an EXE and is not 
// compiled as part of the dll
#include <conio.h>

#define PI    3.141592654
#define RADIANS_TO_DEGREES(rad) ((float) rad * (float) (180.0 / PI))

int main(int argc, char* argv[]) {

   int err = PFS_Connect();

   if (err)
      printf("Connect errored with %d", err);
   else {

      float yaw, pitch, roll;
      while (!err && !_kbhit()) {
         err = PFS_GetOrientation(&yaw, &pitch, &roll);
         if (err) {
            if (err == FREESPACE_ERROR_TIMEOUT) {
               printf("wait timeout\n");
               err = 0;  // not a real error so keep looping
            }
         }
         else {
            yaw = RADIANS_TO_DEGREES(yaw);
            pitch = RADIANS_TO_DEGREES(pitch);
            roll = RADIANS_TO_DEGREES(roll);

            printf("yaw=%g, pitch=%g, roll=%g\n", yaw, pitch, roll);
            //Sleep(0);
         }
      }

      if (err)
         printf("Read errored with %d", err);
   
      PFS_Close();
   }
}

#endif

