#include "Lib/sixense.h"
#include <stdint.h>
#include <xtgmath.h>
#include "Lib\sixense_math.hpp"

#include <iostream>
#include <fstream>
#include <vector>
#include <array>
#include "shared_memory.h"
#include <algorithm>

std::ofstream outfile;
std::array<std::vector<sixenseControllerData>, 2> controller_data;

struct emulated_data
{
  float yaw, pitch, roll;
  float x, y, z;
  float joystick_x;
  float joystick_y;
  float trigger;
  unsigned int buttons;
  int enabled;
  int controller_index;
  unsigned char is_docked;
  unsigned char which_hand;
};

freepie_io::shared_memory<std::array<emulated_data, 2>> shared_memory;

SIXENSE_EXPORT int sixenseInit( void )
{
  outfile.open("sixense.log", std::ios_base::app);
  outfile << "Init\r\n";

  shared_memory = freepie_io::shared_memory<std::array<emulated_data, 2>>("SixenseEmulatedData");

  return SIXENSE_SUCCESS;
}

SIXENSE_EXPORT int sixenseExit( void )
{
	outfile << "Exit\r\n"; 

	outfile.close();

  return SIXENSE_SUCCESS;
}

SIXENSE_EXPORT int sixenseGetMaxBases()
{
		outfile << "sixenseGetMaxBases\r\n"; 

  return 4;
}
SIXENSE_EXPORT int sixenseSetActiveBase( int i )
{
	outfile << "sixenseSetActiveBase\r\n"; 

  return SIXENSE_SUCCESS;
}
SIXENSE_EXPORT int sixenseIsBaseConnected( int i )
{
	outfile << "sixenseIsBaseConnected:" << i << "\r\n"; 

  if(i == 0)
    return 1;
  return 0;
}

SIXENSE_EXPORT int sixenseGetMaxControllers( void )
{
	outfile << "sixenseGetMaxControllers\r\n"; 
  return 4;
}

SIXENSE_EXPORT int sixenseIsControllerEnabled( int which )
{
	outfile << "sixenseIsBaseConnected:" << which << "\r\n"; 

  if(which <= 1)
    return 1;
  return 0;
}
SIXENSE_EXPORT int sixenseGetNumActiveControllers()
{
	outfile << "sixenseGetNumActiveControllers\r\n"; 
  return 2;
}

SIXENSE_EXPORT int sixenseGetHistorySize()
{
outfile << "sixenseGetHistorySize\r\n"; 
  return 0;
}

 void convert_euler(float yaw, float pitch, float roll, sixenseControllerData *output) {
  auto quat = sixenseMath::Quat::rotation(yaw, pitch, roll);
  auto mat = sixenseMath::Matrix3::rotation(quat);
  
  quat.fill((float*)&output->rot_quat);
  mat.fill((float(*)[3])&output->rot_mat);  
 }

std::array<uint8_t, 2> sequence_numbers = { 0 };

SIXENSE_EXPORT int sixenseGetData(int which, int index_back, sixenseControllerData *output)
{
  if(index_back != 0 || which > 2)
    return SIXENSE_FAILURE;

  auto view = shared_memory.open_view();

  if(!view)
    outfile << "Cannot open view" << std::endl;  
  
  auto data = view.map()[which];

  output->sequence_number = sequence_numbers[which]++;

  output->buttons = data.buttons;
  output->trigger = data.trigger;
  output->controller_index = which;
  output->which_hand = data.which_hand;
  output->enabled = data.enabled;
  output->firmware_revision = 174;
  output->hardware_revision = 0;
  output->is_docked = data.is_docked;
  output->joystick_x = data.joystick_x;
  output->joystick_y = data.joystick_y;
  output->pos[0] = data.x;
  output->pos[1] = data.y;
  output->pos[2] = data.z;
  output->hemi_tracking_enabled = 1;
  output->magnetic_frequency = 0;
  output->packet_type = 1;

  convert_euler(data.yaw, data.pitch, data.roll, output);

  return SIXENSE_SUCCESS;
}

SIXENSE_EXPORT int sixenseGetAllData( int index_back, sixenseAllControllerData *output)
{
  auto success = sixenseGetData(0, index_back, &output->controllers[0]);
  success |= sixenseGetData(1, index_back, &output->controllers[1]);
  return success;
}
SIXENSE_EXPORT int sixenseGetNewestData( int which, sixenseControllerData *output)
{
  return sixenseGetData(which, 0, output);
}

SIXENSE_EXPORT int sixenseGetAllNewestData( sixenseAllControllerData *output)
{
  return sixenseGetAllData(0, output);
}

SIXENSE_EXPORT int sixenseSetHemisphereTrackingMode( int which_controller, int state )
{
	outfile << "sixenseSetHemisphereTrackingMode\r\n"; 

  return SIXENSE_SUCCESS;
}
SIXENSE_EXPORT int sixenseGetHemisphereTrackingMode( int which_controller, int *state )
{
	outfile << "sixenseGetHemisphereTrackingMode\r\n"; 
  return SIXENSE_SUCCESS;
}

SIXENSE_EXPORT int sixenseAutoEnableHemisphereTracking( int which_controller )
{
	outfile << "sixenseAutoEnableHemisphereTracking\r\n"; 
  return SIXENSE_SUCCESS;
}

SIXENSE_EXPORT int sixenseSetHighPriorityBindingEnabled( int on_or_off )
{ 
	outfile << "sixenseSetHighPriorityBindingEnabled\r\n"; 

  return SIXENSE_SUCCESS;
}
SIXENSE_EXPORT int sixenseGetHighPriorityBindingEnabled( int *on_or_off )
{

	outfile << "sixenseGetHighPriorityBindingEnabled\r\n"; 
  return SIXENSE_SUCCESS;
}

SIXENSE_EXPORT int sixenseTriggerVibration( int controller_id, int duration_100ms, int pattern_id )
{
	outfile << "sixenseTriggerVibration\r\n"; 
  return SIXENSE_SUCCESS;
}

SIXENSE_EXPORT int sixenseSetFilterEnabled( int on_or_off )
{
	outfile << "sixenseSetFilterEnabled\r\n"; 
  return SIXENSE_SUCCESS;
}
SIXENSE_EXPORT int sixenseGetFilterEnabled( int *on_or_off )
{
	outfile << "sixenseGetFilterEnabled\r\n"; 
  return SIXENSE_SUCCESS;
}

SIXENSE_EXPORT int sixenseSetFilterParams( float near_range, float near_val, float far_range, float far_val )
{
	outfile << "sixenseSetFilterParams\r\n"; 
  return SIXENSE_SUCCESS;
}

SIXENSE_EXPORT int sixenseGetFilterParams( float *near_range, float *near_val, float *far_range, float *far_val )
{
	outfile << "sixenseGetFilterParams\r\n"; 
  return SIXENSE_SUCCESS;
}

SIXENSE_EXPORT int sixenseSetBaseColor( unsigned char red, unsigned char green, unsigned char blue )
{
	outfile << "sixenseGetFilterParams\r\n"; 
  return SIXENSE_SUCCESS;
}
SIXENSE_EXPORT int sixenseGetBaseColor( unsigned char *red, unsigned char *green, unsigned char *blue )
{
	outfile << "sixenseGetBaseColor\r\n"; 
  return SIXENSE_SUCCESS;
}