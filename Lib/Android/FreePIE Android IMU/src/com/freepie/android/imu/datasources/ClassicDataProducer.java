package com.freepie.android.imu.datasources;

import java.util.Arrays;
import java.util.List;

import android.app.Activity;
import android.content.SharedPreferences;
import android.content.SharedPreferences.Editor;
import android.hardware.SensorManager;
import android.widget.CheckBox;
import android.widget.Spinner;

import com.freepie.android.imu.DataProducer;
import com.freepie.android.imu.R;
import com.freepie.android.imu.SampleRate;
import com.freepie.android.imu.Util;

public abstract class ClassicDataProducer extends DataProducer {
	private static final String SEND_ORIENTATION = "send_orientation";
	private static final String SEND_RAW = "send_raw";
	private static final String SAMPLE_RATE = "sample_rate";
	private CheckBox mChkSendOrientation;
	private CheckBox mChkSendRaw;
	private Spinner mSpnSampleRate;

	public ClassicDataProducer() {
		super();
	}

	protected boolean getSendRaw() {
		return mChkSendRaw.isChecked();
	}

	protected boolean getSendOrientation() {
		return mChkSendOrientation.isChecked();
	}

	@Override
	public int getOptionsLayoutId() {
		return R.layout.options_classic;
	}

	@Override
	public void onCreateOptions(Activity context, SharedPreferences preferences) {
	    mChkSendOrientation = (CheckBox) context.findViewById(R.id.sendOrientation);
	    mChkSendRaw = (CheckBox) context.findViewById(R.id.sendRaw);
	    mSpnSampleRate = (Spinner)context.findViewById(R.id.sampleRate);
	
		mChkSendOrientation.setChecked(preferences.getBoolean(SEND_ORIENTATION, true));
		mChkSendRaw.setChecked(preferences.getBoolean(SEND_RAW, true));
	    populateSampleRates(context, preferences.getInt(SAMPLE_RATE, 0));
	}

	@Override
	public Editor savePreferences(Editor editor) {
		return editor.putBoolean(SEND_ORIENTATION, getSendOrientation())
			.putBoolean(SEND_RAW, getSendRaw())
			.putInt(SAMPLE_RATE,  getSampleRate());		
	}

	private void populateSampleRates(Activity context, int defaultSampleRate) {
		List<SampleRate> sampleRates = Arrays.asList(new SampleRate[] {   
			new SampleRate(SensorManager.SENSOR_DELAY_UI, "UI"), 
			new SampleRate(SensorManager.SENSOR_DELAY_NORMAL, "Normal"), 
			new SampleRate(SensorManager.SENSOR_DELAY_GAME, "Game"), 
			new SampleRate(SensorManager.SENSOR_DELAY_FASTEST, "Fastest") 
		});
		
		SampleRate selectedSampleRate = null;
		for (SampleRate sampleRate : sampleRates) {
		  if (sampleRate.getId() == defaultSampleRate) {
		    selectedSampleRate = sampleRate;
		    break;
		  }
		}
		
		Util.populateSpinner(context, mSpnSampleRate, sampleRates, selectedSampleRate);    	
	}

	protected int getSampleRate() {
		return ((SampleRate)mSpnSampleRate.getSelectedItem()).getId();
	}
}