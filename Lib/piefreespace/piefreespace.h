/*
Author: Brant Lewis (brantlew@yahoo.com) - July 2012 

piefreespace (PFS) is a proxy dll that is used to translate complex freespacelib 
calls into a simple API for easy linking by the FreePIE FreeSpacePlugin.  It sets up
the device connection, handles I/O reads, and converts quaternion vectors into
Euler angles.  This simplifies the code in FreePIE instead of implementing and calling
a full C# libfreespace interface

It was developed against libfreespace-0.6 

*/

#ifndef _PIEFREESPACE_H
#define _PIEFREESPACE_H

#ifdef _USRDLL
	#define PFS_DLL_DECL __declspec(dllexport)
#else
	#define PFS_DLL_DECL
#endif

#ifdef _USRDLL
extern "C" {
#endif

PFS_DLL_DECL int PFS_Connect();
PFS_DLL_DECL void PFS_Close();
PFS_DLL_DECL int PFS_GetOrientation(float* yaw, float* pitch, float* roll);


#ifdef _USRDLL
}
#endif

#endif
