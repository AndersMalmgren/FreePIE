package com.freepie.android.imu.datasources;

import java.nio.ByteBuffer;

import com.freepie.android.imu.DataProducer;
import com.freepie.android.imu.TargetSettings;

import android.hardware.Sensor;
import android.hardware.SensorEvent;
import android.hardware.SensorEventListener;
import android.hardware.SensorManager;

public class MagGyroAccOrientationProducer extends DataProducer implements SensorEventListener {
	@SuppressWarnings("unused")
	private static final String TAG = "MagGyroAccOrientationProducer";
	
	private float[] acc;
	private float[] mag;
	private float[] gyr;
	private float[] imu;
	private float[] rotationVector;
	private final float[] rotationMatrix = new float[16];
	private TargetSettings mTarget;

	public MagGyroAccOrientationProducer() {
		super();
	}

	@Override
	public String toString() {
		return "Classic sensor";
	}
	
	public void start(TargetSettings target) {
		mTarget = target;
		if (mTarget.getSendRaw()) {
			mTarget.getSensorManager().registerListener(this,
					mTarget.getSensorManager().getDefaultSensor(Sensor.TYPE_ACCELEROMETER), mTarget.getSampleRate());

			mTarget.getSensorManager().registerListener(this,
					mTarget.getSensorManager().getDefaultSensor(Sensor.TYPE_GYROSCOPE), mTarget.getSampleRate());

			mTarget.getSensorManager().registerListener(this,
					mTarget.getSensorManager().getDefaultSensor(Sensor.TYPE_MAGNETIC_FIELD), mTarget.getSampleRate());
		}
		if (mTarget.getSendOrientation()) {
			mTarget.getSensorManager().registerListener(this,
					mTarget.getSensorManager().getDefaultSensor(Sensor.TYPE_ROTATION_VECTOR), mTarget.getSampleRate());
		}
	}

	@Override
	public void onAccuracyChanged(Sensor sensor, int accuracy) {
	}

	@Override
	public void onSensorChanged(SensorEvent sensorEvent) {
		switch (sensorEvent.sensor.getType()) {
		case Sensor.TYPE_ACCELEROMETER:
			acc = sensorEvent.values.clone();
			break;
		case Sensor.TYPE_MAGNETIC_FIELD:
			mag = sensorEvent.values.clone();
			break;

		case Sensor.TYPE_GYROSCOPE:
			gyr = sensorEvent.values.clone();
			break;
		case Sensor.TYPE_ROTATION_VECTOR:
			rotationVector = sensorEvent.values.clone();
			break;
		}

		if (mTarget.getSendOrientation() && rotationVector != null) {
			SensorManager.getRotationMatrixFromVector(rotationMatrix, rotationVector);
			SensorManager.getOrientation(rotationMatrix, rotationVector);
			imu = rotationVector;
			rotationVector = null;
		}

		if (mTarget.getDebug() && acc != null && gyr != null && mag != null)
			mTarget.getDebugListener().debugRaw(acc, gyr, mag);

		if (mTarget.getDebug() && imu != null)
			mTarget.getDebugListener().debugImu(imu);

		boolean raw = mTarget.getSendRaw() && acc != null && gyr != null && mag != null;
		boolean orientation = mTarget.getSendOrientation() && imu != null;
		if (raw || orientation) {
			notifySenderTask();
		}
	}

	@Override
	public void stop() {
		mTarget.getSensorManager().unregisterListener(this);
		notifySenderTask();
	}

	@Override
	public void fillBuffer(ByteBuffer buffer) {
		boolean raw = mTarget.getSendRaw() && acc != null && gyr != null && mag != null;
		boolean orientation = mTarget.getSendOrientation() && imu != null;
		
		buffer.clear();			
		buffer.put(mTarget.getDeviceIndex());
		buffer.put(getFlagByte(mTarget.getSendRaw(), mTarget.getSendOrientation()));
		buffer.put(getFlagByte(raw, orientation));

		if (raw) {
			//Acc
			buffer.putFloat(acc[0]);
			buffer.putFloat(acc[1]);
			buffer.putFloat(acc[2]);
			
			//Gyro
			buffer.putFloat(gyr[0]);
			buffer.putFloat(gyr[1]);
			buffer.putFloat(gyr[2]);	
			
			//Mag
			buffer.putFloat(mag[0]);
			buffer.putFloat(mag[1]);
			buffer.putFloat(mag[2]);
			
			acc = null;
			mag = null;
			gyr = null;
		}
		
		if (orientation) {			
			buffer.putFloat(imu[0]);
			buffer.putFloat(imu[1]);
			buffer.putFloat(imu[2]);
			imu = null;
		}
	}
}
