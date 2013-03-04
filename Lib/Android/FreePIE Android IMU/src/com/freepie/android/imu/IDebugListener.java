package com.freepie.android.imu;

public interface IDebugListener {
	void debugRaw(float[] acc, float[] gyr, float[] mag);
	void debugImu(float[] imu);
	void debugGPS(float[] distanceBetween);
}
