extern "C"
{
  #include "../include/ovr_freepie.h"
}
#include <OVR.h>

using namespace OVR;

Ptr<DeviceManager> pManager = 0;
Ptr<HMDDevice>     pHMD = 0;
Ptr<SensorDevice>  pSensor = 0;
SensorFusion       FusionResult;

int ovr_freepie_init()
{
   OVR::System::Init();
     
  pManager = *DeviceManager::Create();
  pHMD     = *pManager->EnumerateDevices<HMDDevice>().CreateDevice();

  if (!pHMD)
      return 1;

  pSensor  = *pHMD->GetSensor();

  HMDInfo hmdInfo;
  pHMD->GetDeviceInfo(&hmdInfo);
    
  if (pSensor)
      FusionResult.AttachToSensor(pSensor);

  return 0;
}

int ovr_freepie_reset_orientation()
{
  FusionResult.Reset();
  return 0;
}

int ovr_freepie_read(ovr_freepie_3dof *output)
{
  Quatf q = FusionResult.GetOrientation();

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