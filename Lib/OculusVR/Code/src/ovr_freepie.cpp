extern "C"
{
  #include "../include/ovr_freepie.h"
}
#include <OVR.h>

using namespace OVR;

ovrHmd	HMD;

int ovr_freepie_init()
{
	ovr_Initialize();
	HMD = ovrHmd_Create(0);
	if (!HMD) {
		return 1;
	}

	ovrHmd_SetEnabledCaps(HMD, ovrHmdCap_DynamicPrediction);

	ovrHmd_ConfigureTracking(HMD, ovrTrackingCap_Orientation |
		ovrTrackingCap_MagYawCorrection |
		ovrTrackingCap_Position, 0);

	return 0;
}

int ovr_freepie_reset_orientation()
{
	ovrHmd_RecenterPose(HMD);
	return 0;
}

int ovr_freepie_read(ovr_freepie_6dof *output)
{
	ovrTrackingState ts = ovrHmd_GetTrackingState(HMD, ovr_GetTimeInSeconds());
	Posef pose = ts.HeadPose.ThePose;
	
	pose.Rotation.GetEulerAngles<Axis_Y, Axis_X, Axis_Z>(&output->yaw, &output->pitch, &output->roll);
	output->x = pose.Translation.x;
	output->y = pose.Translation.y;
	output->z = pose.Translation.z;

	return 0;
}

int ovr_freepie_destroy()
{
	ovrHmd_Destroy(HMD); 
	ovr_Shutdown();

	return 0;
}