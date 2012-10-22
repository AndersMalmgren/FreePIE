#pragma once

#include <Windows.h>

class lock
{
public:
	lock(HANDLE mutex_handle, DWORD milliseconds) : mutex(mutex_handle)
	{ 
		wait_result = WaitForSingleObject(mutex_handle, milliseconds);
	}

	~lock()
	{
		if(wait_result == WAIT_OBJECT_0)
			ReleaseMutex(mutex);
	}

	operator bool() const
	{
		return wait_result == WAIT_OBJECT_0;
	}

private:
	DWORD wait_result;
	HANDLE mutex;
};