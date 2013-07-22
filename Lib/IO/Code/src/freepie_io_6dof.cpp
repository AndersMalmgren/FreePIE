// freepie_io_6dof.cpp
// (c) Max Malmgren - https://github.com/AndersMalmgren/FreePIE
// License: MIT (http://www.opensource.org/licenses/mit-license.php)

extern "C"
{
  #include "../include/freepie_io.h"
}

#include "shared_memory.h"
#include <map>

namespace freepie_io {
  struct shared_data
  {
    int32_t data_id;
    freepie_io_6dof_data data;
  };

  const size_t MaxGenericSlots = 4;
  typedef shared_data shared_data_array[MaxGenericSlots];
  shared_memory<shared_data_array> memory;
  view<shared_data_array> mapping;

  std::map<uint32_t, int32_t> data_ids;

  void init_shared_memory()
  {
    if(!memory)
      memory = shared_memory<shared_data_array>("FPGeneric");

    if(memory && !mapping)
      mapping = memory.open_view();
  }

  int32_t check_conditions_and_init_memory(uint32_t start_index, uint32_t length)
  {
    if(start_index + length > MaxGenericSlots)
      return FREEPIE_IO_ERROR_OUT_OF_BOUNDS;

    init_shared_memory();

    if(!memory || !mapping)
      return FREEPIE_IO_ERROR_SHARED_DATA;

    return 0;
  }

  int32_t write(uint32_t start_index, uint32_t length, freepie_io_6dof_data *data_to_write)
  {
    auto error = check_conditions_and_init_memory(start_index, length);

    if(error < 0)
      return error;

    auto mapped_data = mapping.map();

    for(auto i = start_index; i < length + start_index; i++)
    {
      mapped_data[i].data_id++;
      mapped_data[i].data = data_to_write[i - start_index]; 
    }

    return 0;
  }

  template<typename T>
  bool make_equal(T &first, T &second)
  {
    if(first == second)
      return true;

    first = second;
    return false;
  }

  int32_t read(uint32_t start_index, uint32_t length, freepie_io_6dof_data *output)
  {    
    auto error = check_conditions_and_init_memory(start_index, length);

    if(error < 0)
      return error;

    auto mapped_data = mapping.map();

    int32_t new_values = 0;

    for(auto i = start_index; i < length + start_index; i++)
    {
      if(make_equal(data_ids[i], mapped_data[i].data_id))
        continue;

      output[i - start_index] = mapped_data[i].data;
      new_values |= (1 << (i - start_index));
    }

    return new_values;
  }
}

int32_t freepie_io_6dof_write(uint32_t index, uint32_t length, freepie_io_6dof_data *data_to_write)
{
  return freepie_io::write(index, length, data_to_write);
}

uint32_t freepie_io_6dof_slots()
{
  return freepie_io::MaxGenericSlots;
}

int32_t freepie_io_6dof_read(uint32_t index, uint32_t length, freepie_io_6dof_data *output)
{
  return freepie_io::read(index, length, output);
}