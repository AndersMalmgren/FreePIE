#include "NPClient.h"
#include "shared_memory.h"
#include "lock.h"

#include <string>
#include <fstream>
#include <time.h>
#include <iostream>
#include <sstream>

template <typename K> 
std::string operator+ (const std::string& first, const K& second) 
{
	std::stringstream stream;
	stream << first;
	stream << second;
	return stream.str();
}

template <typename K> 
std::string operator+ (const std::string& first, const K&& second) 
{
	std::stringstream stream;
	stream << first;
	stream << second;
	return stream.str();
}

template <typename K> 
std::string operator+ (const char* first, const K&& second) 
{
	std::stringstream stream;
	stream << first;
	stream << second;
	return stream.str();
}

HANDLE mutex_handle = CreateMutex(NULL, FALSE, "Freepie.TrackIRMutex");
freepie::shared_memory<freepie_data> _freepie_data("Freepie.TrackIRFaker");

std::string get_log_path()
{
	lock l(mutex_handle, 10);

	if(l)
	{
		auto view = _freepie_data.open_view();

		if(view)
			return std::string(view.read().log_path);
		
		return std::string();
	}
}

std::string get_time()
{
	time_t t = time(0);   // get time now
	struct tm now;
	localtime_s(&now, &t);

	char buf[80];

	size_t num_chars = strftime(buf, sizeof(buf), "%Y-%m-%d %X", &now);

	return std::string(buf, num_chars);
}

void log_message(std::string path, std::string message)
{
	if(path.empty())
		return;

	std::fstream out(path, std::ios::app);

	if(out)
		out << message << std::endl;
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

sig_data get_signature_from_offset(char* address, std::string &log_path)
{
	static const size_t search_limit = 1500;
	static const char* default_sig = "freepie input emulator";

	sig_data retval;
	address -= search_limit;

	auto signature_address = search_for_word(address, "precise", search_limit);

	if(signature_address !=  NULL)
	{
		log_message(log_path, "Found signature: " + std::string(signature_address, 106));
		memcpy(&retval, signature_address, sizeof(retval));
	} 
	else 
	{
		log_message(log_path, "Sending default signature.");
		memcpy(&retval, default_sig, sizeof(default_sig));
	}

	return retval;
}

void log_function(std::string &log_path, std::string functionName, std::string addition = "")
{
	log_message(log_path, get_time() + " -:- in function: " + functionName + " " + addition);
}

int __stdcall NP_GetSignature(struct sig_data *sig)
{
	static_assert(sizeof sig_data == 400, "sig_data needs to be 400 chars");

	auto path = get_log_path();

	log_function(path, __FUNCTION__);

	*sig = get_signature_from_offset((char*)sig, path);

	return 0;
}

int __stdcall NP_QueryVersion(short *ver)
{
	*ver = 0x0400;

	log_function(get_log_path(), __FUNCTION__);
	return 0;
}

int __stdcall NP_ReCenter(void)
{
	log_function(get_log_path(), __FUNCTION__);
	return 0;
}

int __stdcall NP_RegisterWindowHandle(void *handle)
{
	log_function(get_log_path(), __FUNCTION__);
	return 0;
}

int __stdcall NP_UnregisterWindowHandle(void)
{
	log_function(get_log_path(), __FUNCTION__);
	return 0;
}

int __stdcall NP_RegisterProgramProfileID(short id)
{
	log_function(get_log_path(), __FUNCTION__);
	return 0;
}

int __stdcall NP_RequestData(short data)
{
	log_function(get_log_path(), __FUNCTION__);
	return 0;
}

void log_shared_memory_error(std::string &log_path ,std::string causant, DWORD error_code)
{
	log_message(log_path, "error with " + causant + ", error: " + error_code);
}

bool read_freepie_data(float &yaw, float &pitch, float &roll, float &tx, float &ty, float &tz)
{
	static uint16_t frame_number;

	auto l = lock(mutex_handle, 10);

	if(l)
	{
		if(!_freepie_data)
		{
			log_shared_memory_error(get_log_path(), "memory mapping", GetLastError());
			return false;
		}
		
		auto view = _freepie_data.open_view();

		if(!view)
		{
			log_shared_memory_error(get_log_path(), "view mapping", GetLastError());
			return false;
		}

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
	log_function(get_log_path(), __FUNCTION__);

	memset(data, 0, sizeof tir_data);

	float yaw, pitch, roll, tx, ty, tz;

    if(read_freepie_data(yaw, pitch, roll, tx, ty, tz))
		set_trackir_data(data, yaw, pitch, roll, tx, ty, tz);

	return 0;
}

int __stdcall NP_StopCursor(void)
{
	log_function(get_log_path(), __FUNCTION__);
	return 0;
}

int __stdcall NP_StartCursor(void)
{
	log_function(get_log_path(), __FUNCTION__);
	return 0;
}
int __stdcall NP_StartDataTransmission(void)
{
	log_function(get_log_path(), __FUNCTION__);
	return 0;
}
int __stdcall NP_StopDataTransmission(void)
{
	log_function(get_log_path(), __FUNCTION__);
	return 0;
}

void __stdcall Freepie_Setup(const char* log)
{
	lock l(mutex_handle, 10);

	auto freepie = _freepie_data.open_view();

	auto data = freepie.read();

	strncpy(data.log_path, log, 200);

	freepie.write(data);
}

void __stdcall Freepie_SetPosition(float yaw, float pitch, float roll, float tx, float ty, float tz)
{
	//log_function(__FUNCTION__, std::string("yaw: ") + yaw + std::string(" pitch: ") + pitch + std::string( " roll: ") + roll);
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