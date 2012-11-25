using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreePIE.Core.Plugins.SensorFusion
{
    public static class Vector
    {
        public static float DotProduct(float[] vector1, float[] vector2)
        {
            float op = 0;

            for (int c = 0; c < 3; c++)
            {
                op += vector1[c]*vector2[c];
            }

            return op;
        }

        // Multiply the vector by a scalar. 
        public static void Scale(float[] vectorOut, float[] vectorIn, float scale2)
        {
            for (int c = 0; c < 3; c++)
            {
                vectorOut[c] = vectorIn[c]*scale2;
            }
        }

        public static void Add(float[] vectorOut, float[] vectorIn1, float[] vectorIn2)
        {
            for (int c = 0; c < 3; c++)
            {
                vectorOut[c] = vectorIn1[c] + vectorIn2[c];
            }
        }

        //Multiply two 3x3 matrixs. This function developed by Jordi can be easily adapted to multiple n*n matrix's. (Pero me da flojera!). 
        public static void MatrixMultiply(float[][] a, float[][] b, float[][] mat)
        {
            var op = new float[3];
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    for (int w = 0; w < 3; w++)
                    {
                        op[w] = a[x][w]*b[w][y];
                    }
                    mat[x][y] = 0;
                    mat[x][y] = op[0] + op[1] + op[2];

                    float test = mat[x][y];
                }
            }
        }

        // Computes the cross product of two vectors
        public static void CrossProduct(float[] vectorOut, float[] v1, float[] v2)
        {
            vectorOut[0] = (v1[1]*v2[2]) - (v1[2]*v2[1]);
            vectorOut[1] = (v1[2]*v2[0]) - (v1[0]*v2[2]);
            vectorOut[2] = (v1[0]*v2[1]) - (v1[1]*v2[0]);
        }

        public static float Constrain(float x, float a, float b)
        {
            if (x < a)
            {
                return a;
            }
            else if (b < x)
            {
                return b;
            }
            else
                return x;
        }
    }

    public class RazorAhrs
    {
        private float G_Dt; // Integration time for DCM algorithm

        private float[] accel = new float[3];
        private float[] magnetom = new float[3];
        private float[] gyro = new float[3];

        // Euler angles
        private float yaw;
        private float pitch;
        private float roll;

        // DCM variables
        private float MAG_Heading;
        private float[] Accel_Vector = new float[] {0, 0, 0}; // Store the acceleration in a vector
        private float[] Gyro_Vector = new float[] {0, 0, 0}; // Store the gyros turn rate in a vector
        private float[] Omega_Vector = new float[] {0, 0, 0}; // Corrected Gyro_Vector data
        private float[] Omega_P = new float[] {0, 0, 0}; // Omega Proportional correction
        private float[] Omega_I = new float[] {0, 0, 0}; // Omega Integrator
        private float[] Omega = new float[] {0, 0, 0};
        private float[] errorRollPitch = new float[] {0, 0, 0};
        private float[] errorYaw = new float[] {0, 0, 0};
        private float[][] DCM_Matrix = new[] { new float[] { 1, 0, 0 }, new float[] { 0, 1, 0 }, new float[] { 0, 0, 1 } };
        private float[][] Update_Matrix = new[] { new float[] { 0, 1, 2 }, new float[] { 3, 4, 5 }, new float[] { 6, 7, 8 } };
        private float[][] Temporary_Matrix = new[] { new float[] { 0, 0, 0 }, new float[] { 0, 0, 0 }, new float[] { 0, 0, 0 } };

        // DCM parameters
        private const float Kp_ROLLPITCH = 0.02f;
        private const float Ki_ROLLPITCH = 0.00002f;
        private const float Kp_YAW = 1.2f;
        private const float Ki_YAW = 0.00002f;

        private bool calibrated;
        private const float GRAVITY = 256.0f; // "1G reference" used for DCM filter and accelerometer calibration

        // Gain for gyroscope (ITG-3200)
        private const float GYRO_GAIN = 0.06957f; // Same gain on all axes

        // SENSOR CALIBRATION
        /*****************************************************************/
        // How to calibrate? Read the tutorial at http://dev.qu.tu-berlin.de/projects/sf-razor-9dof-ahrs
        // Put MIN/MAX and OFFSET readings for your board here!
        // Accelerometer
        // "accel x,y,z (min/max) = X_MIN/X_MAX  Y_MIN/Y_MAX  Z_MIN/Z_MAX"

        private float ACCEL_X_MIN = -278f;
        private float ACCEL_X_MAX = 258f;
        private float ACCEL_Y_MIN = -260f;
        private float ACCEL_Y_MAX = 292f;
        private float ACCEL_Z_MIN = -282f;
        private float ACCEL_Z_MAX = 250f;

        // Magnetometer
        // "magn x,y,z (min/max) = X_MIN/X_MAX  Y_MIN/Y_MAX  Z_MIN/Z_MAX"
        private float MAGN_X_MIN = -600f;
        private float MAGN_X_MAX = 600f;
        private float MAGN_Y_MIN = 600f;
        private float MAGN_Y_MAX = 600f;
        private float MAGN_Z_MIN = -600f;
        private float MAGN_Z_MAX = 600f;

        // Gyroscope
        // "gyro x,y,z (current/average) = .../OFFSET_X  .../OFFSET_Y  .../OFFSET_Z
        private float GYRO_AVERAGE_OFFSET_X = -22.13f;
        private float GYRO_AVERAGE_OFFSET_Y = 22.31f;
        private float GYRO_AVERAGE_OFFSET_Z = 6.88f;


        // Sensor calibration scale and offset values
        private float ACCEL_X_OFFSET;
        private float ACCEL_Y_OFFSET;
        private float ACCEL_Z_OFFSET;
        private float ACCEL_X_SCALE;
        private float ACCEL_Y_SCALE;
        private float ACCEL_Z_SCALE;

        private float MAGN_X_OFFSET;
        private float MAGN_Y_OFFSET;
        private float MAGN_Z_OFFSET;
        private float MAGN_X_SCALE;
        private float MAGN_Y_SCALE;
        private float MAGN_Z_SCALE;

        private DateTime timestamp = DateTime.Now;

        public RazorAhrs(float gDt)
        {
            G_Dt = gDt;
        }

        public void Update(float gx, float gy, float gz, float ax, float ay, float az, float mx, float my, float mz)
        {
            gyro[0] = gx;
            gyro[1] = gy;
            gyro[2] = gz;

            accel[0] = ax;
            accel[1] = ay;
            accel[2] = az;

            magnetom[0] = mx;
            magnetom[1] = my;
            magnetom[2] = mz;

            //G_Dt = (float)(DateTime.Now - timestamp).TotalSeconds; // Real time of loop run. We use this on the DCM algorithm (gyro integration time)

            //timestamp = DateTime.Now;


            // Apply sensor calibration
            CompensateSensorErrors();

            // Run DCM algorithm
            CompassHeading(); // Calculate magnetic heading
            MatrixUpdate();
            Normalize();
            Drift_correction();
            Euler_angles();
        }

        public float Yaw
        {
            get { return yaw; }
        }

        // Apply calibration to raw sensor readings
        private void CompensateSensorErrors()
        {
            if (!calibrated)
            {
                // Sensor calibration scale and offset values
                ACCEL_X_OFFSET = ((ACCEL_X_MIN + ACCEL_X_MAX)/2.0f);
                ACCEL_Y_OFFSET = ((ACCEL_Y_MIN + ACCEL_Y_MAX)/2.0f);
                ACCEL_Z_OFFSET = ((ACCEL_Z_MIN + ACCEL_Z_MAX)/2.0f);
                ACCEL_X_SCALE = (GRAVITY/(ACCEL_X_MAX - ACCEL_X_OFFSET));
                ACCEL_Y_SCALE = (GRAVITY/(ACCEL_Y_MAX - ACCEL_Y_OFFSET));
                ACCEL_Z_SCALE = (GRAVITY/(ACCEL_Z_MAX - ACCEL_Z_OFFSET));

                MAGN_X_OFFSET = ((MAGN_X_MIN + MAGN_X_MAX)/2.0f);
                MAGN_Y_OFFSET = ((MAGN_Y_MIN + MAGN_Y_MAX)/2.0f);
                MAGN_Z_OFFSET = ((MAGN_Z_MIN + MAGN_Z_MAX)/2.0f);
                MAGN_X_SCALE = (100.0f/(MAGN_X_MAX - MAGN_X_OFFSET));
                MAGN_Y_SCALE = (100.0f/(MAGN_Y_MAX - MAGN_Y_OFFSET));
                MAGN_Z_SCALE = (100.0f/(MAGN_Z_MAX - MAGN_Z_OFFSET));
                calibrated = true;
            }

            // Compensate accelerometer error
            accel[0] = (accel[0] - ACCEL_X_OFFSET)*ACCEL_X_SCALE;
            accel[1] = (accel[1] - ACCEL_Y_OFFSET)*ACCEL_Y_SCALE;
            accel[2] = (accel[2] - ACCEL_Z_OFFSET)*ACCEL_Z_SCALE;

            // Compensate magnetometer error
            magnetom[0] = (magnetom[0] - MAGN_X_OFFSET)*MAGN_X_SCALE;
            magnetom[1] = (magnetom[1] - MAGN_Y_OFFSET)*MAGN_Y_SCALE;
            magnetom[2] = (magnetom[2] - MAGN_Z_OFFSET)*MAGN_Z_SCALE;

            // Compensate gyroscope error
            gyro[0] -= GYRO_AVERAGE_OFFSET_X;
            gyro[1] -= GYRO_AVERAGE_OFFSET_Y;
            gyro[2] -= GYRO_AVERAGE_OFFSET_Z;
        }

        private void CompassHeading()
        {
            float mag_x;
            float mag_y;
            float cos_roll;
            float sin_roll;
            float cos_pitch;
            float sin_pitch;

            cos_roll = (float) Math.Cos(roll);
            sin_roll = (float) Math.Sin(roll);
            cos_pitch = (float) Math.Cos(pitch);
            sin_pitch = (float) Math.Sin(pitch);

            // Tilt compensated magnetic field X
            mag_x = magnetom[0]*cos_pitch + magnetom[1]*sin_roll*sin_pitch + magnetom[2]*cos_roll*sin_pitch;
            // Tilt compensated magnetic field Y
            mag_y = magnetom[1]*cos_roll - magnetom[2]*sin_roll;
            // Magnetic Heading
            MAG_Heading = (float) Math.Atan2(-mag_y, mag_x);
        }

        private float GetGyroScaledRad(float x)
        {
            return x*((float) Math.PI/180f)*GYRO_GAIN; // Calculate the scaled gyro readings in radians per second
        }

        private void MatrixUpdate()
        {

            Gyro_Vector[0] = GetGyroScaledRad(gyro[0]); //gyro x roll
            Gyro_Vector[1] = GetGyroScaledRad(gyro[1]); //gyro y pitch
            Gyro_Vector[2] = GetGyroScaledRad(gyro[2]); //gyro z yaw

            Accel_Vector[0] = accel[0];
            Accel_Vector[1] = accel[1];
            Accel_Vector[2] = accel[2];

            Vector.Add(Omega, Gyro_Vector, Omega_I); //adding proportional term
            Vector.Add(Omega_Vector, Omega, Omega_P); //adding Integrator term

            Update_Matrix[0][0] = 0;
            Update_Matrix[0][1] = -G_Dt*Omega_Vector[2]; //-z
            Update_Matrix[0][2] = G_Dt*Omega_Vector[1]; //y
            Update_Matrix[1][0] = G_Dt*Omega_Vector[2]; //z
            Update_Matrix[1][1] = 0;
            Update_Matrix[1][2] = -G_Dt*Omega_Vector[0]; //-x
            Update_Matrix[2][0] = -G_Dt*Omega_Vector[1]; //-y
            Update_Matrix[2][1] = G_Dt*Omega_Vector[0]; //x
            Update_Matrix[2][2] = 0;

            Vector.MatrixMultiply(DCM_Matrix, Update_Matrix, Temporary_Matrix); //a*b=c

            for (int x = 0; x < 3; x++) //Matrix Addition (update)
            {
                for (int y = 0; y < 3; y++)
                {
                    DCM_Matrix[x][y] += Temporary_Matrix[x][y];
                }
            }
        }

        private void Normalize()
        {
            float error = 0;
            var temporary = new[] {new float[] {0, 0, 0}, new float[] {0, 0, 0}, new float[] {0, 0, 0}};
            float renorm = 0;

            error = -Vector.DotProduct(DCM_Matrix[0], DCM_Matrix[1])*.5f; //eq.19

            Vector.Scale(temporary[0], DCM_Matrix[1], error); //eq.19
            Vector.Scale(temporary[1], DCM_Matrix[0], error); //eq.19

            Vector.Scale(temporary[0], temporary[0], DCM_Matrix[0][0]); //eq.19
            Vector.Scale(temporary[1], temporary[1], DCM_Matrix[1][0]); //eq.19

            Vector.CrossProduct(temporary[2], temporary[0], temporary[1]); // c= a x b //eq.20

            renorm = .5f*(3 - Vector.DotProduct(temporary[0], temporary[0])); //eq.21
            Vector.Scale(DCM_Matrix[0], temporary[0], renorm);

            renorm = .5f*(3 - Vector.DotProduct(temporary[1], temporary[1])); //eq.21
            Vector.Scale(DCM_Matrix[1], temporary[1], renorm);

            renorm = .5f*(3 - Vector.DotProduct(temporary[2], temporary[2])); //eq.21
            Vector.Scale(DCM_Matrix[2], temporary[2], renorm);
        }

        private void Drift_correction()
        {
            float mag_heading_x;
            float mag_heading_y;
            float errorCourse;
            //Compensation the Roll, Pitch and Yaw drift. 
            var Scaled_Omega_P = new float[3];
            var Scaled_Omega_I = new float[3];
            float Accel_magnitude;
            float Accel_weight;


            //*****Roll and Pitch***************

            // Calculate the magnitude of the accelerometer vector
            Accel_magnitude =
                (float)
                Math.Sqrt(Accel_Vector[0]*Accel_Vector[0] + Accel_Vector[1]*Accel_Vector[1] +
                          Accel_Vector[2]*Accel_Vector[2]);
            Accel_magnitude = Accel_magnitude/GRAVITY; // Scale to gravity.
            // Dynamic weighting of accelerometer info (reliability filter)
            // Weight for accelerometer info (<0.5G = 0.0, 1G = 1.0 , >1.5G = 0.0)
            Accel_weight = Vector.Constrain(1 - 2*Math.Abs(1 - Accel_magnitude), 0, 1); //  

            Vector.CrossProduct(errorRollPitch, Accel_Vector, DCM_Matrix[2]); //adjust the ground of reference
            Vector.Scale(Omega_P, errorRollPitch, Kp_ROLLPITCH*Accel_weight);

            Vector.Scale(Scaled_Omega_I, errorRollPitch, Ki_ROLLPITCH*Accel_weight);
            Vector.Add(Omega_I, Omega_I, Scaled_Omega_I);

            //*****YAW***************
            // We make the gyro YAW drift correction based on compass magnetic heading

            mag_heading_x = (float) Math.Cos(MAG_Heading);
            mag_heading_y = (float) Math.Sin(MAG_Heading);
            errorCourse = (DCM_Matrix[0][0]*mag_heading_y) - (DCM_Matrix[1][0]*mag_heading_x); //Calculating YAW error
            Vector.Scale(errorYaw, DCM_Matrix[2], errorCourse);
                //Applys the yaw correction to the XYZ rotation of the aircraft, depeding the position.

            Vector.Scale(Scaled_Omega_P, errorYaw, Kp_YAW); //.01proportional of YAW.
            Vector.Add(Omega_P, Omega_P, Scaled_Omega_P); //Adding  Proportional.

            Vector.Scale(Scaled_Omega_I, errorYaw, Ki_YAW); //.00001Integrator
            Vector.Add(Omega_I, Omega_I, Scaled_Omega_I); //adding integrator to the Omega_I
        }

        private void Euler_angles()
        {
            pitch = (float) -Math.Asin(DCM_Matrix[2][0]);
            roll = (float) Math.Atan2(DCM_Matrix[2][1], DCM_Matrix[2][2]);
            yaw = (float) Math.Atan2(DCM_Matrix[1][0], DCM_Matrix[0][0]);
        }
    }
}
