#ifndef OVR_FREEPIE_6DOF_H
#define OVR_FREEPIE_6DOF_H
#include <OVR_CAPI.h>

typedef struct ovr_freepie_6dof
{
  float yaw, pitch, roll;
  float yawDegrees, pitchDegrees, rollDegrees;
  float x, y, z;
  
} ovr_freepie_6dof;

typedef struct ovr_freepie_data {
	
	ovr_freepie_6dof head;
	ovr_freepie_6dof leftHand;
	ovr_freepie_6dof rightHand;
	
	unsigned int touches;
	unsigned int buttons;
	
	float LTrigger;
	float RTrigger;

	float LGrip;
	float RGrip;

	ovrVector2f         Lstick;
	ovrVector2f         Rstick;

	ovrControllerType   ControllerType;

	unsigned int statusHead;
	unsigned int statusLeftHand;
	unsigned int statusRightHand;
	unsigned int HmdMounted;

}ovr_freepie_data;

int ovr_freepie_init();
int ovr_freepie_read(ovr_freepie_data *output);
int ovr_freepie_destroy();
int ovr_freepie_reset_orientation();

#endif