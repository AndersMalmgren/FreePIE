#ifndef OVR_FREEPIE_6DOF_H
#define OVR_FREEPIE_6DOF_H

typedef struct ovr_freepie_6dof
{
  float yaw, pitch, roll;
  float x, y, z;
} ovr_freepie_6dof;

int ovr_freepie_init();
int ovr_freepie_read(ovr_freepie_6dof *output);
int ovr_freepie_destroy();
int ovr_freepie_reset_orientation();

#endif