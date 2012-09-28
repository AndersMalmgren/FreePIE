package com.example.freepie.android.imu;

import java.util.Arrays;
import java.util.List;

import android.app.Activity;
import android.content.SharedPreferences;
import android.hardware.SensorManager;
import android.os.Bundle;
import android.view.Menu;
import android.view.View;
import android.view.WindowManager;
import android.widget.ArrayAdapter;
import android.widget.CheckBox;
import android.widget.EditText;
import android.widget.Spinner;
import android.widget.ToggleButton;

public class MainActivity extends Activity {

	private UdpSenderTask udpSender;
	private static final String IP = "ip";
	private static final String PORT = "port";
	private static final String SEND_ORIENTATION = "send_orientation";
	private static final String SEND_RAW = "send_raw";
	private static final String SAMPLE_RATE = "sample_rate";
	
	private ToggleButton start;
	private EditText txtIp;
	private EditText txtPort;
	private CheckBox chkSendOrientation;
	private CheckBox chkSendRaw;
	private Spinner spnSampleRate;
    
		
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        
        final SharedPreferences preferences = getPreferences(MODE_PRIVATE);
        
        start = (ToggleButton) findViewById(R.id.start);
        txtIp = (EditText) findViewById(R.id.ip);
        txtPort = (EditText) findViewById(R.id.port);
        chkSendOrientation = (CheckBox) findViewById(R.id.sendOrientation);
        chkSendRaw = (CheckBox) findViewById(R.id.sendRaw);       
        
        txtIp.setText(preferences.getString(IP,  "192.168.1.1"));
        txtPort.setText(preferences.getString(PORT,  "5555"));
        chkSendOrientation.setChecked(preferences.getBoolean(SEND_ORIENTATION, true));
        chkSendRaw.setChecked(preferences.getBoolean(SEND_RAW, true));
        spnSampleRate = (Spinner)this.findViewById(R.id.sampleRate);
        populateSampleRates(preferences.getInt(SAMPLE_RATE, 0));
        
        final SensorManager sensorManager = (SensorManager)getSystemService(SENSOR_SERVICE);
            
        getWindow().addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);
        
        start.setOnClickListener(new View.OnClickListener() {
            public void onClick(View view) {
            	boolean on = ((ToggleButton) view).isChecked();
            	
            	if(on) {
                String ip = txtIp.getText().toString();
                int port = Integer.parseInt(txtPort.getText().toString());
                boolean sendOrientation = chkSendOrientation.isChecked();
                boolean sendRaw = chkSendRaw.isChecked();
            	
	        	udpSender = new UdpSenderTask();
	        	udpSender.start(new TargetSettings(ip, port, sensorManager, sendOrientation, sendRaw, getSelectedSampleRateId()));
            	} else {
            		udpSender.stop();
            	}
            }
        });
    }
    
    private void populateSampleRates(int defaultSampleRate) {
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
    	
    	ArrayAdapter<SampleRate> sampleRatesAdapter = new ArrayAdapter<SampleRate>(this,
			android.R.layout.simple_spinner_item, sampleRates);
    	spnSampleRate.setAdapter(sampleRatesAdapter);
    	spnSampleRate.setSelection(sampleRates.indexOf(selectedSampleRate), false);
    }
    
    private int getSelectedSampleRateId() {
    	return ((SampleRate)spnSampleRate.getSelectedItem()).getId();
    }

    @Override
    protected void onStop(){
    	super.onStop();    	
    	final SharedPreferences preferences = getPreferences(MODE_PRIVATE);
    	
    	preferences.edit()
			.putString(IP, txtIp.getText().toString())
			.putString(PORT, txtPort.getText().toString())
			.putBoolean(SEND_ORIENTATION, chkSendOrientation.isChecked())
			.putBoolean(SEND_RAW, chkSendRaw.isChecked())
			.putInt(SAMPLE_RATE,  getSelectedSampleRateId())
			.commit();
    }    
    
    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        getMenuInflater().inflate(R.menu.activity_main, menu);
        return true;
    }

    
}
