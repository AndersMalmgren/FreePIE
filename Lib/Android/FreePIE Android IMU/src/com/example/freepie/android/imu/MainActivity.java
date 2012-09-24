package com.example.freepie.android.imu;

import android.os.Bundle;
import android.app.Activity;
import android.content.Intent;
import android.content.SharedPreferences;
import android.hardware.SensorManager;
import android.util.Log;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.view.WindowManager;
import android.widget.Button;
import android.widget.CheckBox;
import android.widget.EditText;
import android.widget.ToggleButton;
import android.support.v4.app.NavUtils;

public class MainActivity extends Activity {

	private UdpSenderTask udpSender;
	private static final String IP = "ip";
	private static final String PORT = "port";
	private static final String SEND_ORIENTATION = "send_orientation";
	private static final String SEND_RAW = "send_raw";
	
	private ToggleButton start;
	private EditText txtIp;
	private EditText txtPort;
	private CheckBox chkSendOrientation;
	private CheckBox chkSendRaw;
    
		
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
	        	udpSender.start(new TargetSettings(ip, port, sensorManager, sendOrientation, sendRaw));
            	} else {
            		udpSender.stop();
            	}
            }
        });
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
			.commit();
    }    
    
    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        getMenuInflater().inflate(R.menu.activity_main, menu);
        return true;
    }

    
}
