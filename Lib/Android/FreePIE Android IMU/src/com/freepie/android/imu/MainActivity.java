package com.freepie.android.imu;

import java.util.ArrayList;
import java.util.List;
import android.app.Activity;
import android.app.AlertDialog;
import android.content.SharedPreferences;
import android.hardware.SensorManager;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.Menu;
import android.view.View;
import android.view.ViewGroup;
import android.view.WindowManager;
import android.widget.AdapterView;
import android.widget.CheckBox;
import android.widget.CompoundButton;
import android.widget.CompoundButton.OnCheckedChangeListener;
import android.widget.EditText;
import android.widget.LinearLayout;
import android.widget.Spinner;
import android.widget.TextView;
import android.widget.ToggleButton;

public class MainActivity extends Activity implements IDebugListener, IErrorHandler {
	public static final String TAG = "FreePIE";
	
	private UdpSenderTask udpSender;
	private static final String IP = "ip";
	private static final String PORT = "port";
	private static final String INDEX = "index";
	private static final String DEBUG = "debug";
	private static final String DATAPRODUCER = "producer";
	
	private static final String[] producers = new String[] {
		"com.freepie.android.imu.datasources.MagGyroAccOrientationProducer",
		"com.freepie.android.imu.datasources.MagAccOrientationProducer",
		"com.freepie.android.imu.datasources.MagAccFilteringOrientationProducer",
	};
	
	private static final String DEBUG_FORMAT = "%.2f;%.2f;%.2f";
	
	private ToggleButton start;
	private EditText txtIp;
	private EditText txtPort;
	private Spinner spnIndex;
	private CheckBox chkDebug; 
	private LinearLayout debugView;
	private TextView acc;    
	private TextView gyr;
	private TextView mag;
	private TextView imu;
	private Spinner spnDataProducers;
	private DataProducer lastDataProducer;
		
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        setContentView(R.layout.activity_main);

        final SharedPreferences preferences = getPreferences(MODE_PRIVATE);        
        
        txtIp = (EditText) findViewById(R.id.ip);
        txtPort = (EditText) findViewById(R.id.port);
        spnIndex = (Spinner) findViewById(R.id.index);        
        spnDataProducers = (Spinner)this.findViewById(R.id.sensors);
        start = (ToggleButton) findViewById(R.id.start);
        debugView = (LinearLayout) findViewById(R.id.debugView);
        chkDebug = (CheckBox) findViewById(R.id.debug);
        acc = (TextView) findViewById(R.id.acc);
        gyr = (TextView) findViewById(R.id.gyr);
        mag = (TextView) findViewById(R.id.mag);
        imu = (TextView) findViewById(R.id.imu);
        acc.setText(R.string.debug_no_data);
        gyr.setText(R.string.debug_no_data);
        mag.setText(R.string.debug_no_data);
        imu.setText(R.string.debug_no_data);
        
        txtIp.setText(preferences.getString(IP,  "192.168.1.1"));
        txtPort.setText(preferences.getString(PORT,  "5555"));

        populateDataProducers(preferences.getString(DATAPRODUCER, producers[0]));
        
        final ViewGroup optionsViewGroup = (ViewGroup)findViewById(R.id.producerOptions);
        View child = LayoutInflater.from(this).inflate(getSelectedDataProducer().getOptionsLayoutId(), null);
        optionsViewGroup.addView(child);
        
        getSelectedDataProducer().onCreateOptions(this, preferences);

        spnDataProducers.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
			@Override
			public void onItemSelected(AdapterView<?> arg0, View arg1, int arg2, long arg3) {
				stop();
				lastDataProducer.savePreferences(preferences.edit()).commit();
				optionsViewGroup.removeAllViews();
		        View child = LayoutInflater.from(MainActivity.this).inflate(getSelectedDataProducer().getOptionsLayoutId(), null);
		        optionsViewGroup.addView(child);
		        getSelectedDataProducer().onCreateOptions(MainActivity.this, preferences);
			}

			@Override
			public void onNothingSelected(AdapterView<?> arg0) {
			}
        });
        
        chkDebug.setChecked(preferences.getBoolean(DEBUG, false));        
        populateIndex(preferences.getInt(INDEX, 0));
        setDebugVisibility(chkDebug.isChecked());
        
        final SensorManager sensorManager = (SensorManager)getSystemService(SENSOR_SERVICE);
            
        getWindow().addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);
        getWindow().setSoftInputMode(WindowManager.LayoutParams.SOFT_INPUT_STATE_ALWAYS_HIDDEN);
        
        chkDebug.setOnCheckedChangeListener(new OnCheckedChangeListener()
        {
            public void onCheckedChanged(CompoundButton buttonView, boolean isChecked)
            {
            	setDebugVisibility(isChecked);
            	
            	if(udpSender != null)
            		udpSender.setDebug(isChecked);
            }
        });
        
        final IDebugListener debugListener = this;  
        final IErrorHandler error = this;
        start.setOnClickListener(new View.OnClickListener() {
            public void onClick(View view) {
            	boolean on = ((ToggleButton) view).isChecked();
            	
            	if(on) {
	                String ip = txtIp.getText().toString();
	                int port = Integer.parseInt(txtPort.getText().toString());
	                boolean debug = chkDebug.isChecked();
	            	
	                udpSender = new UdpSenderTask(getSelectedDataProducer());
		        	udpSender.start(new TargetSettings(ip, port, getSelectedDeviceIndex(), sensorManager, debug, debugListener, error));
            	} else {
            		stop();
            	}
            }
        });
    }
    
    private void stop() {
    	start.setChecked(false);
    	if (udpSender != null) {
			udpSender.stop();
			udpSender = null;
    	}
    }
    
    private void setDebugVisibility(boolean show) {
    	debugView.setVisibility(show ? LinearLayout.VISIBLE : LinearLayout.INVISIBLE);
    }
    
    private void populateIndex(int defaultIndex) {
    	List<DeviceIndex> deviceIndexes = new ArrayList<DeviceIndex>();
    	for(byte index = 0; index < 16; index++) {
    		deviceIndexes.add(new DeviceIndex(index));
    	}
    	
    	DeviceIndex selectedDeviceIndex = null;
      	for (DeviceIndex deviceIndex : deviceIndexes) {
  		  if (deviceIndex.getIndex() == defaultIndex) {
  			selectedDeviceIndex = deviceIndex;
  		    break;
  		  }
  		}
      	
      	Util.populateSpinner(this, spnIndex, deviceIndexes, selectedDeviceIndex);
    }
    
    private void populateDataProducers(String defaultDataProducer) {
    	DataProducer selected = null;
    	List<DataProducer> dataProducers = new ArrayList<DataProducer>();
    	for (String pclass: producers) {
    		try {
    			DataProducer p = (DataProducer)Class.forName(pclass).newInstance();
				dataProducers.add(p);
				if (pclass.equals(defaultDataProducer)) {
					selected = p;
				}
			} catch (Exception e) {
			}
    	}
    	Util.populateSpinner(this, spnDataProducers, dataProducers, selected);
    	lastDataProducer = selected;
    }
    
    private byte getSelectedDeviceIndex() {
    	return ((DeviceIndex)spnIndex.getSelectedItem()).getIndex();
    }
    
    private DataProducer getSelectedDataProducer() {
    	return (DataProducer)spnDataProducers.getSelectedItem();
    }
    
	@Override
	public void debugRaw(float[] acc, float[] gyr, float[] mag) {
		if (acc != null)
			this.acc.setText(String.format(DEBUG_FORMAT, acc[0], acc[1], acc[2]));
		if (gyr != null) 
			this.gyr.setText(String.format(DEBUG_FORMAT, gyr[0], gyr[1], gyr[2]));
		if (mag != null)
			this.mag.setText(String.format(DEBUG_FORMAT, mag[0], mag[1], mag[2]));
	}
	
	@Override
	public void debugImu(float[] imu) {
		this.imu.setText(imu != null ? String.format(DEBUG_FORMAT, imu[0], imu[1], imu[2]) : "(no data)");		
	}

    @Override
    protected void onStop(){
    	super.onStop();    	
    	final SharedPreferences preferences = getPreferences(MODE_PRIVATE);

    	SharedPreferences.Editor editor = preferences.edit().putString(IP, txtIp.getText().toString())
			.putString(PORT, txtPort.getText().toString())
			.putInt(INDEX,  getSelectedDeviceIndex())
			.putBoolean(DEBUG, chkDebug.isChecked())
			.putString(DATAPRODUCER, getSelectedDataProducer().getClass().getName()); 	
    	editor = getSelectedDataProducer().savePreferences(editor);
    	editor.commit();
    }    
    
    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        getMenuInflater().inflate(R.menu.activity_main, menu);
        return true;
    }

	@Override
	public void error(final String title, final String text) {
		final Activity activity = this;
		
		this.runOnUiThread(new Runnable() { 
            public void run(){
            	new AlertDialog.Builder(activity).setTitle(title).setMessage(text).setNeutralButton("OK", null).show();         
        		stop();
            }
		});
	} 
}
