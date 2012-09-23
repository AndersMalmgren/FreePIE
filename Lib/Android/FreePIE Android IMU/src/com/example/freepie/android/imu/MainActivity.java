package com.example.freepie.android.imu;

import android.os.Bundle;
import android.app.Activity;
import android.content.Intent;
import android.hardware.SensorManager;
import android.util.Log;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.widget.Button;
import android.widget.CheckBox;
import android.widget.EditText;
import android.widget.ToggleButton;
import android.support.v4.app.NavUtils;

public class MainActivity extends Activity {

	private UdpSenderTask udpSender;
	
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        
        final ToggleButton start = (ToggleButton) findViewById(R.id.start);
        final EditText txtIp = (EditText) findViewById(R.id.ip);
        final EditText txtPort = (EditText) findViewById(R.id.port);
        final CheckBox chkSendOrientation = (CheckBox) findViewById(R.id.sendOrientation);
        final CheckBox chkSendRaw = (CheckBox) findViewById(R.id.sendRaw);       
        
        txtIp.setText("192.168.1.99");
        txtPort.setText("5555");
        chkSendOrientation.setChecked(true);
        chkSendRaw.setChecked(true);
        
        final SensorManager sensorManager = (SensorManager)getSystemService(SENSOR_SERVICE);
                
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
    public boolean onCreateOptionsMenu(Menu menu) {
        getMenuInflater().inflate(R.menu.activity_main, menu);
        return true;
    }

    
}
