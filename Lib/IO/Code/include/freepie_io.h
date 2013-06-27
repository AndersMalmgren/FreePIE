#ifndef FREEPIE_IO_H
#define FREEPIE_IO_H

#include <stdint.h>

typedef struct freepie_io_6dof_data
{
    float yaw;
    float pitch;
    float roll;

    float x;
    float y;
    float z;
} freepie_io_6dof_data;

/*
  The number of currently available 'generic 6-degrees-of-freedom' slots.
*/
uint32_t freepie_io_6dof_slots();

/*
  Used to make 6-degrees-of-freedom data available to any
  programs currently listening for data on the 
  'generic 6-degrees-of-freedom interface' defined in conjuction with FreePIE.

  The return value is an error code if (code < 0).

  Example 1: If (code >= 0) for the call (index = 1, length = 3, data_to_write = 'array with at least 3 elements')
             then 3 elements have been written to index 1, 2, 3.

  Example 2: Return value of -2 corresponds to FREEPIE_IO_ERROR_OUT_OF_BOUNDS.
*/
int32_t freepie_io_6dof_write(uint32_t index, uint32_t length, freepie_io_6dof_data *data_to_write);

/*
  Used to read 6-degrees-of-freedom data from any 6dof provider compatible with 
  the 'generic 6-degrees-of-freedom interface' defined in conjuction with FreePIE.

  The return value is an error code if (code < 0), otherwise it is a bit sequence
  signifying whether an update has occured in the corresponding index or not.

  Example 1: bit sequence ...00000110 returned by the call with bounding arguments (index = 1, length = 3)
             signifies that since the previous call freepie_io_6dof index 1 has not changed, but index 2 and 3 have.
             The value in index 0 is unknown, since it was not requested.

             The output variable now contains an array of data starting at the requested index with a length
             corresponding to the requested length.

  Example 2: Return value of -2 corresponds to FREEPIE_IO_ERROR_OUT_OF_BOUNDS.
*/
int32_t freepie_io_6dof_read(uint32_t index, uint32_t length, freepie_io_6dof_data *output);

/*
  An attempt to access the shared store of data failed.
*/
static const int32_t FREEPIE_IO_ERROR_SHARED_DATA = -1;

/*
  An attempt to access out of bounds data was made.
*/
static const int32_t FREEPIE_IO_ERROR_OUT_OF_BOUNDS = -2;

#endif