extern "C"
{
  #include "../include/ovr_freepie.h"
}
#include <OVR_CAPI.h>
#include <OVR_Math.h>

using namespace OVR;

ovrSession HMD;
double	HmdFrameTiming;

int ovr_freepie_init()
{
	ovrResult result = ovr_Initialize(nullptr);
	if (!OVR_SUCCESS(result)) return 1;

	ovrGraphicsLuid luid;
	result = ovr_Create(&HMD, &luid);

	if (!OVR_SUCCESS(result)) return 1;

	if (!HMD) {
		return 1;
	}

	return 0;
}

int ovr_freepie_reset_orientation()
{
	ovr_RecenterTrackingOrigin(HMD);
	return 0;
}

int ovr_freepie_read(ovr_freepie_6dof *output)
{
	HmdFrameTiming = ovr_GetPredictedDisplayTime(HMD, 0);
	ovrTrackingState ts = ovr_GetTrackingState(HMD, ovr_GetTimeInSeconds(), HmdFrameTiming);
	Posef pose = ts.HeadPose.ThePose;
	
	pose.Rotation.GetEulerAngles<Axis_Y, Axis_X, Axis_Z>(&output->yaw, &output->pitch, &output->roll);
	output->x = pose.Translation.x;
	output->y = pose.Translation.y;
	output->z = pose.Translation.z;

	return 0;
}

int ovr_freepie_destroy()
{
	ovr_Destroy(HMD); 
	ovr_Shutdown();

	return 0;
}