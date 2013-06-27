#include "../include/freepie_io.h"
#include <stdio.h>
#include <string.h>
#include <Windows.h>

#define ARRAY_SIZE 4

void read(int slots, freepie_io_6dof_data *data)
{
  int i;
  int read_result = freepie_io_6dof_read(0, slots, data);

  if(read_result < 0)
  {
    printf("\nError occured while reading");
    return;
  }

  i = 0;
  while(read_result > 0)
  {
    if(read_result & 1)
      printf("\nResult in requested index: %i", i);

    read_result >>= 1;
    i++;
  }
}

void write(int slots, freepie_io_6dof_data *data)
{
  if(freepie_io_6dof_write(0, slots, data))
    printf("\nError occured while writing");
}

void increment(int count, freepie_io_6dof_data *data)
{
  int i;
  for(i = 0; i < count; i++)
  {
    data[i].x += 1e-3;
    data[i].y += 1e-3;
    data[i].z += 1e-3;
    data[i].yaw += 1e-3;
    data[i].pitch += 1e-3;
    data[i].roll += 1e-3;
  }
}

int main()
{
  int do_read = 0;
  int i = 0;
  int count = 0;
  uint32_t slots;
  freepie_io_6dof_data data[ARRAY_SIZE] = { 0 };

  slots = freepie_io_6dof_slots();

  if(slots > ARRAY_SIZE)
  {
    printf("Allocated data too small to accomodate %i slots.", slots);
    return 0;
  }

  printf("\nAttempting to read...");
  
  while(1)
  {
    if(do_read)
      read(slots, data);
    else
    {
      increment(ARRAY_SIZE, data);
      write(slots, data);
    }

    Sleep(1);
  }

  printf("\nAll done.");
}