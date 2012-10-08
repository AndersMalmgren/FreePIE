/*
	Copyright Max Malmgren 2012 - just give me cred.
*/
#pragma once

#include <Windows.h>
#include <string>

namespace freepie {

	enum access_type { ReadWrite, Read };

	DWORD file_map_access_type(access_type type)
		{
			switch(type)
			{
			case ReadWrite:				
				return FILE_MAP_ALL_ACCESS;				
			case Read:
				return FILE_MAP_READ;
			default:
				return FILE_MAP_READ;
			}
		}

	DWORD page_access_type(access_type type)
	{
		switch(type)
		{
		case ReadWrite:				
			return PAGE_READWRITE;				
		case Read:
			return PAGE_READONLY;
		default:
			return PAGE_READONLY;
		}
	}

	template <typename Target> 
	class view
	{
	public:
		view(HANDLE map_handle, access_type type) : type(type), view_ptr(MapViewOfFile(map_handle, file_map_access_type(type), 0, 0, 0))
		{
		}

		~view()
		{
			if(view_ptr != NULL)
				UnmapViewOfFile(view_ptr);
		}

		operator bool() const
		{
			return view_ptr != NULL;
		}

		view(view&& other) : view_ptr(other.view_ptr)
		{
			other.view_ptr = NULL;
		}

		const Target& read() const
		{
			return *(Target*)view_ptr;
		}

		void write(const Target& out)
		{
			*((Target*)view_ptr) = out;
		}

	private:
		access_type type;
		LPVOID view_ptr;

		view(view& other);
		view& operator=(const view& rhs);		
	};

	template <typename Target>
	class shared_memory
	{
	public:		
		shared_memory(std::string shared_memory_name, access_type access_type = ReadWrite) : name(shared_memory_name), type(access_type), map_handle(open_file_map(name, access_type))
		{
		}

		~shared_memory()
		{
			if(map_handle != NULL)
				CloseHandle(map_handle);
		}

		operator bool() const
		{
			return map_handle != NULL;
		}

		view<Target> open_view()
		{
			if(map_handle == NULL)
				throw std::runtime_error("Cannot open view - handle not opened");

			return view<Target>(map_handle, type);
		}

	private:
		std::string name;
		access_type type;
		HANDLE map_handle;

		HANDLE CreateFileMapping(std::string name, access_type access_type)
		{
			return ::CreateFileMapping(INVALID_HANDLE_VALUE, NULL, page_access_type(access_type), 0, sizeof(Target), name.c_str());
		}

		HANDLE open_file_map(std::string name, access_type access_type)
		{
			HANDLE handle = OpenFileMapping(file_map_access_type(access_type), true, name.c_str());
			return handle != NULL ? handle : CreateFileMapping(name, access_type);
		}
	};
}