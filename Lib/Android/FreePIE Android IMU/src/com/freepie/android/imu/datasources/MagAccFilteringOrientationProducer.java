package com.freepie.android.imu.datasources;

import java.nio.ByteBuffer;

import com.freepie.android.imu.DataProducer;
import com.freepie.android.imu.R;
import com.freepie.android.imu.TargetSettings;
import com.freepie.android.imu.dsp.Biquad;
import com.freepie.android.imu.dsp.Filter;
import com.freepie.android.imu.dsp.FilterArray;

import android.app.Activity;
import android.content.SharedPreferences;
import android.content.SharedPreferences.Editor;
import android.hardware.Sensor;
import android.hardware.SensorEvent;
import android.hardware.SensorEventListener;
import android.hardware.SensorManager;
import android.text.Editable;
import android.text.TextWatcher;
import android.widget.RadioGroup;
import android.widget.RadioGroup.OnCheckedChangeListener;
import android.widget.SeekBar;
import android.widget.SeekBar.OnSeekBarChangeListener;
import android.widget.TextView;

public class MagAccFilteringOrientationProducer extends DataProducer implements SensorEventListener {
	@SuppressWarnings("unused")
	private static final String TAG = "MagAccFilteringOrientationProducer";
	
	private final static String FILTER_CUTOFF = "filter_cutoff";
	private final static String SAMPLE_RATE = "filtering_samplefreq";
	
	private final static float FILTERQ = 0.7f;

	
	private float[] mAcc = new float[3];
	private float[] mMag = new float[3];
	
	private float[] mFilteredAcc = new float[3];
	private float[] mFilteredMag = new float[3];
	
	private float[] mRotationVector;
	private final float[] rotationMatrix = new float[16];
	private TargetSettings mTarget;
	private boolean mFilterRunning;

	private int mSampleRate = 50;
	private float mCutoff = 0.5f;
	
	private FilterArray mMagFilter;
	private FilterArray mAccFilter;
	
	public MagAccFilteringOrientationProducer() {
		super();
	}

	@Override
	public String toString() {
		return "Acc+Mag Filtering Sensor";
	}
	
	private void createFilters() {
		FilterArray.Builder lowpass = new FilterArray.Builder() {
			public Filter create() {
				return new Biquad(Biquad.LOWPASS, mSampleRate, mCutoff, FILTERQ);
			}
		}; 
		
		synchronized(mFilterLock) {
			mMagFilter = new FilterArray(3, lowpass);
			mAccFilter = new FilterArray(3, lowpass);
		}
	}

	public void start(TargetSettings target) {
		mTarget = target;
		createFilters();
		mTarget.getSensorManager().registerListener(this,
				mTarget.getSensorManager().getDefaultSensor(Sensor.TYPE_ACCELEROMETER), SensorManager.SENSOR_DELAY_FASTEST);

		mTarget.getSensorManager().registerListener(this,
				mTarget.getSensorManager().getDefaultSensor(Sensor.TYPE_MAGNETIC_FIELD), SensorManager.SENSOR_DELAY_FASTEST);
		
		mFilterRunning = true;
		Thread filter = new Thread(new Runnable() {
			@Override
			public void run() {
				float[] mag = new float[3];
				float[] acc = new float[3];
				
				while(mFilterRunning) {
					try {
						Thread.sleep(1000 / mSampleRate); // 50 Hz -> 20ms
					} catch (InterruptedException e) {}
					
					synchronized(this) {
						System.arraycopy(mMag, 0, mag, 0, 3);
						System.arraycopy(mAcc, 0, acc, 0, 3);
					}					
					synchronized(mFilterLock) {
						mMagFilter.filter(mag, mFilteredMag);
						mAccFilter.filter(acc, mFilteredAcc);
					}
					mRotationVector = getOrientation();
					if (mRotationVector != null) {
						notifySenderTask();
					}
				}
			}
		});
		filter.start();
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

		if (mRotationVector != null) {
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
		boolean orientation = mRotationVector != null;
		
		buffer.clear();			
		buffer.put(mTarget.getDeviceIndex());
		buffer.put(getFlagByte(false, true));
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

	private SeekBar mCutoffSeekBar;
	private TextView mCutoffText;
	private RadioGroup mSampleRateRadioGroup;
	private boolean mIgnoreTextChange;
	private Object mTag = new Object();

	private Object mFilterLock = new Object();
	
	@Override
	public void onCreateOptions(Activity context, SharedPreferences preferences) {
		mCutoffSeekBar = (SeekBar) context.findViewById(R.id.cutoffSeekBar);
		mCutoffText = (TextView) context.findViewById(R.id.cutoffEditText);
		mSampleRateRadioGroup = (RadioGroup) context.findViewById(R.id.fsRadioGroup);
		
		mSampleRate = preferences.getInt(SAMPLE_RATE, 50);
		mCutoff = preferences.getFloat(FILTER_CUTOFF, 0.5f);

		updateSampleRateRadioGroup();
		mSampleRateRadioGroup.setOnCheckedChangeListener(new OnCheckedChangeListener() {
			@Override
			public void onCheckedChanged(RadioGroup group, int checkedId) {
				switch(checkedId) {
				case R.id.fs10:	mSampleRate = 10; break;
				case R.id.fs25: mSampleRate = 25; break;
				case R.id.fs50: mSampleRate = 50; break;
				}
				limitCutoff();
				updateCutoffText();
				mCutoffSeekBar.setMax(freqToProgress(maxCutoffFreq()));
				mCutoffSeekBar.setProgress(freqToProgress(mCutoff));
				updateFilters();
			}
		});
		
		mCutoffSeekBar.setMax(freqToProgress(maxCutoffFreq()));
		mCutoffSeekBar.setProgress(freqToProgress(mCutoff));
		mCutoffSeekBar.setOnSeekBarChangeListener(new OnSeekBarChangeListener() {
			@Override
			public void onProgressChanged(SeekBar seekBar, int progress, boolean fromUser) {
				if (fromUser) {
					mCutoff = progressToFreq(progress);
					updateCutoffText();
				}
			}
			@Override
			public void onStartTrackingTouch(SeekBar seekBar) {
			}
			@Override
			public void onStopTrackingTouch(SeekBar seekBar) {
			}
		});
		
		updateCutoffText();

		mCutoffText.addTextChangedListener(new TextWatcher() {
			@Override
			public void afterTextChanged(Editable s) {
				if (mIgnoreTextChange || s.getSpanFlags(mTag) != 0)
					return;
				try {
					mCutoff = progressToFreq(freqToProgress(Float.parseFloat(s.toString())));
				} catch (NumberFormatException e) {}
				limitCutoff();
				updateCutoffSeekBar();
				
				try {
					s.setSpan(mTag, 0, s.length(), 1);
					s.replace(0, s.length(), String.format("%2.2f", mCutoff));
				} 
				finally {
					s.removeSpan(mTag);
				}
			}

			@Override
			public void beforeTextChanged(CharSequence s, int start, int count, int after) {
			}

			@Override
			public void onTextChanged(CharSequence s, int start, int before, int count) {
			}
		});
	}

	private void limitCutoff() {
		if (mCutoff > maxCutoffFreq()) {
			mCutoff = maxCutoffFreq();
		}
		if (mCutoff < 0) {
			mCutoff = 0;
		}
	}
	
	private void updateSampleRateRadioGroup() {
		int id;
		switch (mSampleRate) {
		case 10:
			id = R.id.fs10;
			break;
		case 25:
			id = R.id.fs25;
			break;
		case 50:
			id = R.id.fs50;
			break;
		default:
			id = -1;
			break;
		}
		mSampleRateRadioGroup.check(id);
	}

	private float maxCutoffFreq() {
		return mSampleRate / 5;
	}

	private int freqToProgress(float f) {
		return Math.round(f*4);
	}
	
	private float progressToFreq(int p) {
		return p * 0.25f;
	}
	
	private void updateCutoffText() {
		try {
			mIgnoreTextChange = true;
			mCutoffText.setText(String.format("%2.2f", mCutoff));
		} 
		finally {
			mIgnoreTextChange = false;
		}
		updateFilters();
	}

	private void updateCutoffSeekBar() {
		mCutoffSeekBar.setProgress(freqToProgress(mCutoff));
		updateFilters();
	}

	private void updateFilters() {
		if (this.mFilterRunning) {
			createFilters();
		}
	}

	@Override
	public int getOptionsLayoutId() {
		return R.layout.options_filtering;
	}

	@Override
	public Editor savePreferences(Editor editor) {
		return editor
				.putFloat(FILTER_CUTOFF, mCutoff)
				.putInt(SAMPLE_RATE, mSampleRate);
	}

}
