#include <algorithm>

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

int ovr_freepie_setPose(Posef pose, ovr_freepie_6dof *dof)
{
	pose.Rotation.GetEulerAngles<Axis_Y, Axis_X, Axis_Z>(
		&dof->yaw,
		&dof->pitch,
		&dof->roll);
	
	
	dof->yawDegrees = RadToDegree(dof->yaw);
	dof->pitchDegrees = RadToDegree(dof->pitch);
	dof->rollDegrees = RadToDegree(dof->roll);

	dof->x = pose.Translation.x;
	dof->y = pose.Translation.y;
	dof->z = pose.Translation.z;
	return  0;
}

int ovr_freepie_read(ovr_freepie_data *output)
{
	HmdFrameTiming = ovr_GetPredictedDisplayTime(HMD, 0);
	ovrTrackingState ts = ovr_GetTrackingState(HMD, ovr_GetTimeInSeconds(), HmdFrameTiming);
	ovrSessionStatus sessionStatus;

	ovrResult result = ovr_GetSessionStatus(HMD,&sessionStatus);
	output->HmdMounted = sessionStatus.HmdMounted;
	Posef headpose = ts.HeadPose.ThePose;
	Posef lhandPose = ts.HandPoses[ovrHand_Left].ThePose;
	Posef rhandPose = ts.HandPoses[ovrHand_Right].ThePose;
	ovr_freepie_setPose(headpose, &output->head);	
	ovr_freepie_setPose(lhandPose, &output->leftHand);	
	ovr_freepie_setPose(rhandPose, &output->rightHand);
	
	output->statusHead = ts.StatusFlags;
	output->statusLeftHand = ts.HandStatusFlags[ovrHand_Left];
	output->statusRightHand = ts.HandStatusFlags[ovrHand_Right];

	ovrInputState inputState;

	if (OVR_SUCCESS(ovr_GetInputState(HMD, ovrControllerType_Touch, &inputState)))
	{
		output->buttons = inputState.Buttons;
		output->touches = inputState.Touches;

		output->ControllerType = inputState.ControllerType;

		output->LTrigger = inputState.IndexTrigger[ovrHand_Left];
		output->LGrip = inputState.HandTrigger[ovrHand_Left];
		output->Lstick = inputState.Thumbstick[ovrHand_Left];

		output->RTrigger = inputState.IndexTrigger[ovrHand_Right];
		output->RGrip = inputState.HandTrigger[ovrHand_Right];
		output->Rstick = inputState.Thumbstick[ovrHand_Right];
			
	}
	
	return 0;
}

int ovr_freepie_destroy()
{
	ovr_Destroy(HMD); 
	ovr_Shutdown();

	return 0;
}