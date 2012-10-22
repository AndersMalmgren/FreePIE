/*

Copyright (c) 2012 Max Malmgren

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
documentation files (the "Software"), to deal in the Software without restriction, including without limitation
the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,
and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

CREDIT TO linuxtrack-wine FOR THE GROUNDWORK IN EMULATING TRACKIR - atleast that is where I found the relevant information.

*/


#include <cstdint>
#include <iostream>
#include <string>

struct sig_data {
        char dllsig[200];
        char appsig[200];
};

struct head_pose_data
{
	float yaw;
	float pitch;
	float roll;
	float tx;
	float ty;
	float tz;
};

struct head_pose_data_with_frame
{
	head_pose_data head_pose;
	uint32_t frame_number;
};

struct freepie_data
{
	head_pose_data_with_frame data;
	char log_path[200];
};

struct tir_data {
        short status;
        short frame;
        unsigned int checksum;
        float roll, pitch, yaw;
        float tx, ty, tz;
        float padding[9];
};

extern "C" int __declspec(dllexport) __stdcall NP_GetSignature(struct sig_data *sig);
extern "C" int __declspec(dllexport) __stdcall NP_QueryVersion(short *ver);
extern "C" int __declspec(dllexport) __stdcall NP_ReCenter();
extern "C" int __declspec(dllexport) __stdcall NP_RegisterWindowHandle(void *handle);
extern "C" int __declspec(dllexport) __stdcall NP_UnregisterWindowHandle();
extern "C" int __declspec(dllexport) __stdcall NP_RegisterProgramProfileID(short id);
extern "C" int __declspec(dllexport) __stdcall NP_RequestData(short data);
extern "C" int __declspec(dllexport) __stdcall NP_GetData(void *data);
extern "C" int __declspec(dllexport) __stdcall NP_StopCursor();
extern "C" int __declspec(dllexport) __stdcall NP_StartCursor();
extern "C" int __declspec(dllexport) __stdcall NP_StartDataTransmission();
extern "C" int __declspec(dllexport) __stdcall NP_StopDataTransmission();

extern "C" void __declspec(dllexport) __stdcall Freepie_Setup(const char* log_path);
extern "C" void __declspec(dllexport) __stdcall Freepie_SetPosition();
extern "C" head_pose_data __declspec(dllexport) __stdcall Freepie_DecodeTrackirData(head_pose_data data);