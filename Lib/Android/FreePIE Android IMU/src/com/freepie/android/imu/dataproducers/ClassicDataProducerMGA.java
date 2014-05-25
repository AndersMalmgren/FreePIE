package com.freepie.android.imu.dataproducers;

import java.nio.ByteBuffer;

import com.freepie.android.imu.TargetSettings;

import android.hardware.Sensor;
import android.hardware.SensorEvent;
import android.hardware.SensorEventListener;
import android.hardware.SensorManager;

public class ClassicDataProducerMGA extends ClassicDataProducer implements SensorEventListener {
	@SuppressWarnings("unused")
	private static final String TAG = "ClassicDataProducerMGA";

	private float[] acc;
	private float[] mag;
	private float[] gyr;
	private float[] imu;
	private float[] rotationVector;
	private final float[] rotationMatrix = new float[16];
	private TargetSettings mTarget;

	public ClassicDataProducerMGA() {
		super();
	}

	@Override
	public String toString() {
		return "Acc+Gyro+Mag Sensor";
	}
	
	public void start(TargetSettings target) {
		mTarget = target;
		if (getSendRaw()) {
			mTarget.getSensorManager().registerListener(this,
					mTarget.getSensorManager().getDefaultSensor(Sensor.TYPE_ACCELEROMETER), getSampleRate());

			mTarget.getSensorManager().registerListener(this,
					mTarget.getSensorManager().getDefaultSensor(Sensor.TYPE_GYROSCOPE), getSampleRate());

			mTarget.getSensorManager().registerListener(this,
					mTarget.getSensorManager().getDefaultSensor(Sensor.TYPE_MAGNETIC_FIELD), getSampleRate());
		}
		if (getSendOrientation()) {
			mTarget.getSensorManager().registerListener(this,
					mTarget.getSensorManager().getDefaultSensor(Sensor.TYPE_ROTATION_VECTOR), getSampleRate());
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

		if (getSendOrientation() && rotationVector != null) {
			SensorManager.getRotationMatrixFromVector(rotationMatrix, rotationVector);
			SensorManager.getOrientation(rotationMatrix, rotationVector);
			imu = rotationVector;
			rotationVector = null;
		}

		if (mTarget.getDebug() && acc != null && gyr != null && mag != null)
			mTarget.getDebugListener().debugRaw(acc, gyr, mag);

		if (mTarget.getDebug() && imu != null)
			mTarget.getDebugListener().debugImu(imu);

		boolean raw = getSendRaw() && acc != null && gyr != null && mag != null;
		boolean orientation = getSendOrientation() && imu != null;
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
		boolean raw = getSendRaw() && acc != null && gyr != null && mag != null;
		boolean orientation = getSendOrientation() && imu != null;
		
		buffer.clear();			
		buffer.put(mTarget.getDeviceIndex());
		buffer.put(getFlagByte(getSendRaw(), getSendOrientation()));
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
