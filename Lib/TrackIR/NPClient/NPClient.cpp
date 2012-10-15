#include "NPClient.h"
#include "shared_memory.h"
#include "lock.h"

#include <string>
#include <fstream>
#include <time.h>
#include <iostream>
#include <sstream>

HANDLE mutex_handle = CreateMutex(NULL, FALSE, "Freepie.TrackIRMutex");
freepie::shared_memory<freepie_data> _freepie_data("FreePIEDisconnectedData");

const std::string word = "precise";

std::string get_log_path()
{
	auto l = lock(mutex_handle, 10);

	if(l)
	{
		if(!_freepie_data)
			return false;
		
		auto view = _freepie_data.open_view();

		if(!view)
			return false;

		auto freepie = view.read();

		return freepie.log_path;
	}

	return std::string();
}

std::fstream get_log_stream()
{
	return std::fstream(get_log_path(), std::ios::app);
}

template <typename T>
void log_message(const T& t)
{
	auto stream = get_log_stream();
	stream << t << std::endl;
}

template <typename T, typename K>
void log_message(const T& t, const K& k)
{
	auto stream = get_log_stream();
	stream << t << k << std::endl;
}

const char* search_for_word(const char* address, const std::string &word, size_t limit)
{
	size_t matching_chars = 0;

	for(size_t i = 0; i < limit; i++)
	{
		if(address[i] == word[matching_chars])
			matching_chars++;
		else matching_chars = 0;

		if(matching_chars == word.length())
			return address + i - matching_chars + 1;
	}

	return NULL;
}

void get_signature_from_offset(char* address, sig_data *destination)
{
	const size_t search_limit = 1500;
	const char* default_sig = "freepie input emulator";
	const size_t default_sig_size = sizeof("freepie input emulator");

	address -= search_limit;

	auto signature_address = search_for_word(address, word, search_limit);

	if(signature_address !=  NULL)
	{
		log_message("found signature in game - using it");
		memcpy(destination, signature_address, sizeof(sig_data));
	}
	else 
	{
		log_message("did not find signature in game - using freepie signature");
		memset(destination, 0, sizeof(sig_data));
		memcpy(destination, default_sig, default_sig_size);		
	}
}

int __stdcall NP_GetSignature(struct sig_data *sig)
{
	static_assert(sizeof sig_data == 400, "sig_data needs to be 400 chars");

	log_message("NP_GetSignature");

	get_signature_from_offset((char*)sig, sig);

	return 0;
}

int __stdcall NP_QueryVersion(short *ver)
{
	*ver = 0x0400;

	log_message("NP_QueryVersion");

	return 0;
}

int __stdcall NP_ReCenter(void)
{
	return 0;
}

int __stdcall NP_RegisterWindowHandle(void *handle)
{
	auto log_path = get_log_path();
	log_message("NP_RegisterWindowHandle, handle: ", handle);
	
	return 0;
}

int __stdcall NP_UnregisterWindowHandle(void)
{
	return 0;
}

int __stdcall NP_RegisterProgramProfileID(short id)
{
	log_message("NP_RegisterProgramProfileId, id: ", id);

	return 0;
}

int __stdcall NP_RequestData(short data)
{
	log_message("NP_RequestData: data", data);

	return 0;
}

bool read_freepie_data(float &yaw, float &pitch, float &roll, float &tx, float &ty, float &tz)
{
	static uint16_t frame_number;

	auto l = lock(mutex_handle, 10);

	if(l)
	{
		if(!_freepie_data)
			return false;
		
		auto view = _freepie_data.open_view();

		if(!view)
			return false;

		auto freepie = view.read();

		if(freepie.data.frame_number == frame_number)
			return false;

		frame_number = freepie.data.frame_number;
		yaw = freepie.data.head_pose.yaw;
		pitch = freepie.data.head_pose.pitch;
		roll = freepie.data.head_pose.roll;
		tx = freepie.data.head_pose.tx;
		ty = freepie.data.head_pose.ty;
		tz = freepie.data.head_pose.tz;

		return true;
	}

	return false;
}

void set_freepie_data(float &yaw, float &pitch, float &roll, float &tx, float &ty, float &tz)
{
	auto l = lock(mutex_handle, 10);

	if(l)
	{
		auto view = _freepie_data.open_view();

		freepie_data freepie = view.read();

		freepie.data.frame_number++;
		freepie.data.head_pose.yaw = yaw;
		freepie.data.head_pose.pitch = pitch;
		freepie.data.head_pose.roll = roll;
		freepie.data.head_pose.tx = tx;
		freepie.data.head_pose.ty = ty;
		freepie.data.head_pose.tz = tz;

		view.write(freepie);
	}
}

void set_trackir_data(void *data, float yaw, float pitch, float roll, float tx, float ty, float tz)
{
	static unsigned short frame;

	tir_data *tir = (tir_data*)data;

	tir->frame = frame++;
	tir->yaw = -(yaw / 180.0f) * 16384.0f;
	tir->pitch = -(pitch / 180.0f) * 16384.0f;
	tir->roll = -(roll / 180.0f) * 16384.0f;

	tir->tx = -tx * 64.0f;
	tir->ty = ty * 64.0f;
    tir->tz = tz * 64.0f;
}

int __stdcall NP_GetData(void *data)
{
	memset(data, 0, sizeof tir_data);

	float yaw, pitch, roll, tx, ty, tz;

    if(read_freepie_data(yaw, pitch, roll, tx, ty, tz))
		set_trackir_data(data, yaw, pitch, roll, tx, ty, tz);

	return 0;
}

int __stdcall NP_StopCursor(void)
{
	return 0;
}

int __stdcall NP_StartCursor(void)
{
	return 0;
}
int __stdcall NP_StartDataTransmission(void)
{
	log_message("NP_StartDataTransmission: ");

	return 0;
}
int __stdcall NP_StopDataTransmission(void)
{
	return 0;
}

void __stdcall Freepie_SetPosition(float yaw, float pitch, float roll, float tx, float ty, float tz)
{
	set_freepie_data(yaw, pitch, roll, tx, ty, tz);
}

head_pose_data __stdcall Freepie_DecodeTrackirData(head_pose_data input)
{
	head_pose_data data;

	data.yaw = -(input.yaw * 180.0f) / 16384.0f;
	data.pitch = -(input.pitch * 180.0f) / 16384.0f;
	data.roll = -(input.roll * 180.0f) / 16384.0f;

	data.tx = -input.tx / 64.0f;
    data.ty = input.ty / 64.0f;
    data.tz = input.tz / 64.0f;

	return data;
}