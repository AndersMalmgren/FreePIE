#ifndef OVR_FREEPIE_3DOF_H
#define OVR_FREEPIE_3DOF_H

typedef struct ovr_freepie_3dof
{
  float yaw, pitch, roll;
} ovr_freepie_3dof;

int ovr_freepie_init(float sensorPrediction);
int ovr_freepie_read(ovr_freepie_3dof *output);
int ovr_freepie_destroy();
int ovr_freepie_reset_orientation();

#endif