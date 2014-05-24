package com.freepie.android.imu.datasources;

import java.nio.ByteBuffer;

import com.freepie.android.imu.Biquad;
import com.freepie.android.imu.DataProducer;
import com.freepie.android.imu.Filter;
import com.freepie.android.imu.FilterArray;
import com.freepie.android.imu.TargetSettings;
import com.freepie.android.imu.FilterArray.Builder;

import android.hardware.Sensor;
import android.hardware.SensorEvent;
import android.hardware.SensorEventListener;
import android.hardware.SensorManager;
import android.util.Log;

public class MagAccFilteringOrientationProducer extends DataProducer implements SensorEventListener {
	@SuppressWarnings("unused")
	private static final String TAG = "MagAccFilteringOrientationProducer";
	
	private float[] mAcc = new float[3];
	private float[] mMag = new float[3];
	
	private float[] mFilteredAcc = new float[3];
	private float[] mFilteredMag = new float[3];
	
	private float[] mRotationVector;
	private final float[] rotationMatrix = new float[16];
	private TargetSettings mTarget;
	private boolean mFilterRunning;

	private final static int mSampleRate = 50;
	private final static float mCutoff = 0.5f;
	private final static float mQ = 0.7f;
	
	private FilterArray mMagFilter;
	private FilterArray mAccFilter;
	
	public MagAccFilteringOrientationProducer() {
		super();
	}

	@Override
	public String toString() {
		return "No gyro filtering sensor";
	}
	
	private void createFilters() {
		FilterArray.Builder lowpass = new FilterArray.Builder() {
			public Filter create() {
				return new Biquad(Biquad.LOWPASS, mSampleRate, mCutoff, mQ);
			}
		}; 
		
		mMagFilter = new FilterArray(3, lowpass);
		mAccFilter = new FilterArray(3, lowpass);
	}

	public void start(TargetSettings target) {
		mTarget = target;
		createFilters();
		mTarget.getSensorManager().registerListener(this,
				mTarget.getSensorManager().getDefaultSensor(Sensor.TYPE_ACCELEROMETER), mTarget.getSampleRate());

		mTarget.getSensorManager().registerListener(this,
				mTarget.getSensorManager().getDefaultSensor(Sensor.TYPE_MAGNETIC_FIELD), mTarget.getSampleRate());
		
		mFilterRunning = true;
		Thread filter = new Thread(new Runnable() {
			@Override
			public void run() {
				float[] mag = new float[3];
				float[] acc = new float[3];
				
				while(mFilterRunning) {
					try {
						Thread.sleep(1000/mSampleRate); // 50 Hz -> 20ms
					} catch (InterruptedException e) {}
					
					synchronized(this) {
						System.arraycopy(mMag, 0, mag, 0, 3);
						System.arraycopy(mAcc, 0, acc, 0, 3);
					}
					
					mMagFilter.filter(mag, mFilteredMag);
					mAccFilter.filter(acc, mFilteredAcc);
					mRotationVector = getOrientation();
					if (mRotationVector != null) {
						notifySenderTask();
					}
				}
			}
		});
		filter.start();
	}

	private float[] norm(float[] arr) {
		double l = Math.sqrt(arr[0]*arr[0] + arr[1]*arr[1] + arr[2]*arr[2]);
		arr[0] = (float) (arr[0] / l);
		arr[1] = (float) (arr[1] / l);
		arr[2] = (float) (arr[2] / l);
		return arr;
	}
	
	private String repr(float[] arr) {
		if (arr == null) return "(null)";
		
		StringBuilder bob = new StringBuilder("[");
		for (float f: arr) {
			bob.append(String.format("%3.2f,", f));
		}
		return bob.delete(bob.length()-1, bob.length()-1).append("]").toString();
	}
	
	@Override
	public void onAccuracyChanged(Sensor sensor, int accuracy) {
	}

	@Override
	public void onSensorChanged(SensorEvent sensorEvent) {
		switch (sensorEvent.sensor.getType()) {
		case Sensor.TYPE_ACCELEROMETER:
			synchronized(this) {
				System.arraycopy(sensorEvent.values, 0, mAcc, 0, 3);
			}
			break;
		case Sensor.TYPE_MAGNETIC_FIELD:
			synchronized(this) {
				System.arraycopy(sensorEvent.values, 0, mMag, 0, 3);
			}
			break;
		}

		if (mTarget.getSendOrientation() && mRotationVector != null) {
			SensorManager.getRotationMatrixFromVector(rotationMatrix, mRotationVector);
			SensorManager.getOrientation(rotationMatrix, mRotationVector);
		}

		if (mTarget.getDebug() && mAcc != null && mMag != null)
			mTarget.getDebugListener().debugRaw(mAcc, null, mMag);

		if (mTarget.getDebug() && mRotationVector != null)
			mTarget.getDebugListener().debugImu(mRotationVector);
	}

	@Override
	public void stop() {
		mTarget.getSensorManager().unregisterListener(this);
		mFilterRunning = false;
		notifySenderTask();
	}

	@Override
	public void fillBuffer(ByteBuffer buffer) {
		boolean orientation = mTarget.getSendOrientation() && mRotationVector != null;
		
		buffer.clear();			
		buffer.put(mTarget.getDeviceIndex());
		buffer.put(getFlagByte(mTarget.getSendRaw(), mTarget.getSendOrientation()));
		buffer.put(getFlagByte(false, orientation));

		if (orientation) {			
			buffer.putFloat(mRotationVector[0]);
			buffer.putFloat(mRotationVector[1]);
			buffer.putFloat(mRotationVector[2]);
		}
	}
	
	private float[] getOrientation() {
		float[] orientation = null;

		float R[] = new float[9];
		float I[] = new float[9];
		boolean success = SensorManager.getRotationMatrix(R, I, mFilteredAcc, mFilteredMag);
		if (success) {
			orientation = new float[3];
			SensorManager.getOrientation(R, orientation);
		}
		
		return orientation;
	}

}
