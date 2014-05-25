package com.freepie.android.imu.dataproducers;

import java.nio.ByteBuffer;

import com.freepie.android.imu.TargetSettings;
import android.hardware.Sensor;
import android.hardware.SensorEvent;
import android.hardware.SensorEventListener;
import android.hardware.SensorManager;

public class ClassicDataProducerMA extends ClassicDataProducer implements SensorEventListener {
	@SuppressWarnings("unused")
	private static final String TAG = "ClassicDataProducerMA";
	
	private float[] mAcc;
	private float[] mMag;
	private float[] mRotationVector;
	private final float[] rotationMatrix = new float[16];
	private TargetSettings mTarget;

	public ClassicDataProducerMA() {
		super();
	}

	@Override
	public String toString() {
		return "Acc+Mag Sensor";
	}

	public void start(TargetSettings target) {
		mTarget = target;
		if (getSendRaw()) {
			mTarget.getSensorManager().registerListener(this,
					mTarget.getSensorManager().getDefaultSensor(Sensor.TYPE_ACCELEROMETER), getSampleRate());
			mTarget.getSensorManager().registerListener(this,
					mTarget.getSensorManager().getDefaultSensor(Sensor.TYPE_MAGNETIC_FIELD), getSampleRate());
		}
	}

	@Override
	public void onAccuracyChanged(Sensor sensor, int accuracy) {
	}

	@Override
	public void onSensorChanged(SensorEvent sensorEvent) {
		switch (sensorEvent.sensor.getType()) {
		case Sensor.TYPE_ACCELEROMETER:
			mAcc = sensorEvent.values.clone();
			break;
		case Sensor.TYPE_MAGNETIC_FIELD:
			mMag = sensorEvent.values.clone();
			break;
		}

		if (getSendOrientation() && mRotationVector != null) {
			SensorManager.getRotationMatrixFromVector(rotationMatrix, mRotationVector);
			SensorManager.getOrientation(rotationMatrix, mRotationVector);
		}

		if (mTarget.getDebug() && mAcc != null && mMag != null)
			mTarget.getDebugListener().debugRaw(mAcc, null, mMag);

		if (mTarget.getDebug() && mRotationVector != null)
			mTarget.getDebugListener().debugImu(mRotationVector);

		boolean raw = getSendRaw() && mAcc != null && mMag != null;
		boolean orientation = getSendOrientation() && mRotationVector != null;
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
		boolean raw = getSendRaw() && mAcc != null && mMag != null;
		boolean orientation = getSendOrientation() && mRotationVector != null;
		
		buffer.clear();			
		buffer.put(mTarget.getDeviceIndex());
		buffer.put(getFlagByte(getSendRaw(), getSendOrientation()));
		buffer.put(getFlagByte(raw, orientation));

		if (raw) {
			//Acc
			buffer.putFloat(mAcc[0]);
			buffer.putFloat(mAcc[1]);
			buffer.putFloat(mAcc[2]);
			
			//Gyro
			buffer.putFloat(0);
			buffer.putFloat(0);
			buffer.putFloat(0);	
			
			//Mag
			buffer.putFloat(mMag[0]);
			buffer.putFloat(mMag[1]);
			buffer.putFloat(mMag[2]);
			
			mAcc = null;
			mMag = null;
		}
		
		if (orientation) {			
			buffer.putFloat(mRotationVector[0]);
			buffer.putFloat(mRotationVector[1]);
			buffer.putFloat(mRotationVector[2]);
			mRotationVector = null;
		}
	}
}
