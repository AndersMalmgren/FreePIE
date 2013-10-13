extern "C"
{
  #include "../include/ovr_freepie.h"
}
#include <OVR.h>

using namespace OVR;

Ptr<DeviceManager> pManager = 0;
Ptr<HMDDevice>     pHMD = 0;
Ptr<SensorDevice>  pSensor = 0;
SensorFusion*       pFusionResult;
bool enableSensorPrediction = false;
float sensorPrediction = 0;

int ovr_freepie_init(float dt)
{
	OVR::System::Init();
	     
	pFusionResult = new SensorFusion();

	pManager = *DeviceManager::Create();
	pHMD     = *pManager->EnumerateDevices<HMDDevice>().CreateDevice();
	
	if (!pHMD)
		return 1;
	
	sensorPrediction = dt;
	enableSensorPrediction = sensorPrediction > 0;
	

	pSensor  = *pHMD->GetSensor();

	HMDInfo hmdInfo;
	pHMD->GetDeviceInfo(&hmdInfo);
    
	if (pSensor)
		pFusionResult->AttachToSensor(pSensor);
	if(enableSensorPrediction)
	pFusionResult->SetPrediction(sensorPrediction);

	return 0;
}

int ovr_freepie_reset_orientation()
{
	pFusionResult->Reset();
	return 0;
}

int ovr_freepie_read(ovr_freepie_3dof *output)
{
	Quatf q = enableSensorPrediction ? pFusionResult->GetPredictedOrientation() : pFusionResult->GetOrientation();

	Matrix4f bodyFrameMatrix(q); 

	q.GetEulerAngles<Axis_Y, Axis_X, Axis_Z>(&output->yaw, &output->pitch, &output->roll);

	return 0;
}

int ovr_freepie_destroy()
{
	pSensor.Clear();
	pHMD.Clear();
	pManager.Clear();
  
	OVR::System::Destroy();
	return 0;
}