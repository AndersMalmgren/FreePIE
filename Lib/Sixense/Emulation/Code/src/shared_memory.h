/*
	Copyright Max Malmgren 2012 - just give me cred.
*/
#pragma once

#include <Windows.h>
#include <string>

namespace freepie_io {
	template <typename Target> 
	class view
	{
	public:

    view() : view_ptr(NULL)
    { }

    view(HANDLE map_handle) : view_ptr(MapViewOfFile(map_handle, FILE_MAP_ALL_ACCESS, 0, 0, 50))
		{	}

    view(view&& other) : view_ptr(other.view_ptr)
		{
			other.view_ptr = NULL;
		}

    view& operator=(view&& rhs)
		{
      view_ptr = rhs.view_ptr;
			rhs.view_ptr = NULL;
      return *this;
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

		Target& map() const
		{
			return *(Target*)view_ptr;
		}

		void write(const Target& out)
		{
			*((Target*)view_ptr) = out;
		}

	private:
		LPVOID view_ptr;

		view(view& other);
		view& operator=(const view& rhs);		
	};

	template <typename Target>
	class shared_memory
	{
	public:
    shared_memory() : name(), map_handle(NULL)
		{	}

		shared_memory(std::string shared_memory_name) : name(shared_memory_name), map_handle(open_file_map(name))
		{	}

    shared_memory(shared_memory &&other) : map_handle(other.map_handle)
		{
      other.map_handle = NULL;
    }

    shared_memory& operator=(shared_memory &&rhs)
    {
      map_handle = rhs.map_handle;
      rhs.map_handle = NULL;
      return *this;
    }

		~shared_memory()
		{
			if(map_handle != NULL && map_handle != INVALID_HANDLE_VALUE)
				CloseHandle(map_handle);
		}

		operator bool() const
		{
			return map_handle != NULL && map_handle != INVALID_HANDLE_VALUE;
		}

		view<Target> open_view()
		{
			return view<Target>(map_handle);
		}

	private:
		std::string name;
		HANDLE map_handle;

    shared_memory(shared_memory& other);
		shared_memory& operator=(const shared_memory& rhs);		

		HANDLE create_file_mapping(std::string name)
		{
      auto size = sizeof(Target);
      return CreateFileMapping(INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 0, sizeof(Target), name.c_str());
		}

		HANDLE open_file_map(std::string name)
		{
      HANDLE handle = OpenFileMapping(FILE_MAP_ALL_ACCESS, FALSE, name.c_str());
			return create_file_mapping(name);
		}
	};
}