package com.freepie.android.imu;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.Timer;
import java.util.TimerTask;
import java.util.logging.Handler;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.ComponentName;
import android.content.Context;
import android.content.Intent;
import android.content.ServiceConnection;
import android.content.SharedPreferences;
import android.hardware.SensorManager;
import android.os.Bundle;
import android.os.IBinder;
import android.view.Menu;
import android.view.View;
import android.view.WindowManager;
import android.widget.ArrayAdapter;
import android.widget.CheckBox;
import android.widget.CompoundButton;
import android.widget.CompoundButton.OnCheckedChangeListener;
import android.widget.EditText;
import android.widget.LinearLayout;
import android.widget.Spinner;
import android.widget.TextView;
import android.widget.ToggleButton;

public class MainActivity extends Activity {
    private UdpSenderService udpSenderService;

    private ServiceConnection conn = new ServiceConnection() {
        @Override
        public void onServiceConnected(ComponentName componentName, IBinder iBinder) {
            udpSenderService = ((UdpSenderService.MyBinder) iBinder).getService();
            start.setChecked(udpSenderService.isRunning());
        }

        @Override
        public void onServiceDisconnected(ComponentName componentName) {
            udpSenderService = null;
        }
    };

    private void bindService() {
        startService(new Intent(this, UdpSenderService.class));
        bindService(new Intent(this, UdpSenderService.class), conn, Context.BIND_AUTO_CREATE);
    }

    private void unbindService() {
        if (udpSenderService != null) {
            unbindService(conn);
        }
    }

    private static final String IP = "ip";
    private static final String PORT = "port";
    private static final String INDEX = "index";
    private static final String SEND_ORIENTATION = "send_orientation";
    private static final String SEND_RAW = "send_raw";
    private static final String SAMPLE_RATE = "sample_rate";
    private static final String DEBUG = "debug";

    private static final String DEBUG_FORMAT = "%.2f;%.2f;%.2f";

    private ToggleButton start;
    private EditText txtIp;
    private EditText txtPort;
    private Spinner spnIndex;
    private CheckBox chkSendOrientation;
    private CheckBox chkSendRaw;
    private Spinner spnSampleRate;
    private CheckBox chkDebug;
    private LinearLayout debugView;
    private TextView acc;
    private TextView gyr;
    private TextView mag;
    private TextView imu;

    private TimerTask debugHandler = new TimerTask() {
        @Override
        public void run() {
            runOnUiThread(new Runnable() {
                @Override
                public void run() {
                    if (udpSenderService != null && udpSenderService.isRunning()) {
                        float[] imu = new float[3], acc = new float[3], mag = new float[3], gyr = new float[3];
                        String lastError = udpSenderService.debug(acc, mag, gyr, imu);
                        if (lastError != null)
                            error(lastError);
                        debugImu(imu);
                        debugRaw(acc, gyr, mag);
                    }
                }
            });
        }
    };

    private Timer t = new Timer();

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        final SharedPreferences preferences = getPreferences(MODE_PRIVATE);

        txtIp = (EditText) findViewById(R.id.ip);
        txtPort = (EditText) findViewById(R.id.port);
        spnIndex = (Spinner) findViewById(R.id.index);
        chkSendOrientation = (CheckBox) findViewById(R.id.sendOrientation);
        chkSendRaw = (CheckBox) findViewById(R.id.sendRaw);
        spnSampleRate = (Spinner) this.findViewById(R.id.sampleRate);
        start = (ToggleButton) findViewById(R.id.start);
        debugView = (LinearLayout) findViewById(R.id.debugView);
        chkDebug = (CheckBox) findViewById(R.id.debug);
        acc = (TextView) findViewById(R.id.acc);
        gyr = (TextView) findViewById(R.id.gyr);
        mag = (TextView) findViewById(R.id.mag);
        imu = (TextView) findViewById(R.id.imu);

        txtIp.setText(preferences.getString(IP, "192.168.1.1"));
        txtPort.setText(preferences.getString(PORT, "5555"));
        chkSendOrientation.setChecked(preferences.getBoolean(SEND_ORIENTATION, true));
        chkSendRaw.setChecked(preferences.getBoolean(SEND_RAW, true));
        chkDebug.setChecked(preferences.getBoolean(DEBUG, false));
        populateSampleRates(preferences.getInt(SAMPLE_RATE, 0));
        populateIndex(preferences.getInt(INDEX, 0));

        chkDebug.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                setDebugVisibility(chkDebug.isChecked());
            }
        });

        setDebugVisibility(chkDebug.isChecked());

        final SensorManager sensorManager = (SensorManager) getSystemService(SENSOR_SERVICE);

        //getWindow().addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);
        getWindow().setSoftInputMode(WindowManager.LayoutParams.SOFT_INPUT_STATE_ALWAYS_HIDDEN);

        start.setOnClickListener(new View.OnClickListener() {
            public void onClick(View view) {
                if (udpSenderService != null) {
                    save();
                    boolean flag = !udpSenderService.isRunning();
                    if (flag) {
                        String ip = txtIp.getText().toString();
                        int port = Integer.parseInt(txtPort.getText().toString());
                        boolean sendOrientation = chkSendOrientation.isChecked();
                        boolean sendRaw = chkSendRaw.isChecked();
                        udpSenderService.start(new TargetSettings(ip, port, getSelectedDeviceIndex(), sensorManager, sendOrientation, sendRaw, getSelectedSampleRateId()));
                    } else {
                        udpSenderService.stop();
                    }
                    start.setChecked(udpSenderService.isRunning());
                }
            }
        });

        t.schedule(debugHandler, 1000L, 1000L);

        bindService();
    }

    private void setDebugVisibility(boolean show) {
        debugView.setVisibility(show ? LinearLayout.VISIBLE : LinearLayout.INVISIBLE);
    }

    private void populateIndex(int defaultIndex) {
        List<DeviceIndex> deviceIndexes = new ArrayList<DeviceIndex>();
        for (byte index = 0; index < 16; index++) {
            deviceIndexes.add(new DeviceIndex(index));
        }

        DeviceIndex selectedDeviceIndex = null;
        for (DeviceIndex deviceIndex : deviceIndexes) {
            if (deviceIndex.getIndex() == defaultIndex) {
                selectedDeviceIndex = deviceIndex;
                break;
            }
        }

        populateSpinner(spnIndex, deviceIndexes, selectedDeviceIndex);
    }

    private void populateSampleRates(int defaultSampleRate) {
        List<SampleRate> sampleRates = Arrays.asList(new SampleRate[]{
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

        populateSpinner(spnSampleRate, sampleRates, selectedSampleRate);
    }

    private <T> void populateSpinner(Spinner spinner, List<T> items, T selectedItem) {
        ArrayAdapter<T> adapter = new ArrayAdapter<T>(this,
                android.R.layout.simple_spinner_item, items);
        spinner.setAdapter(adapter);
        spinner.setSelection(items.indexOf(selectedItem), false);
    }

    private int getSelectedSampleRateId() {
        return ((SampleRate) spnSampleRate.getSelectedItem()).getId();
    }

    private byte getSelectedDeviceIndex() {
        return ((DeviceIndex) spnIndex.getSelectedItem()).getIndex();
    }

    private void debugRaw(float[] acc, float[] gyr, float[] mag) {
        this.acc.setText(String.format(DEBUG_FORMAT, acc[0], acc[1], acc[2]));
        this.gyr.setText(String.format(DEBUG_FORMAT, gyr[0], gyr[1], gyr[2]));
        this.mag.setText(String.format(DEBUG_FORMAT, mag[0], mag[1], mag[2]));
    }

    private void debugImu(float[] imu) {
        this.imu.setText(String.format(DEBUG_FORMAT, imu[0], imu[1], imu[2]));
    }

    private void save() {
        final SharedPreferences preferences = getPreferences(MODE_PRIVATE);

        preferences.edit()
                .putString(IP, txtIp.getText().toString())
                .putString(PORT, txtPort.getText().toString())
                .putInt(INDEX, getSelectedDeviceIndex())
                .putBoolean(SEND_ORIENTATION, chkSendOrientation.isChecked())
                .putBoolean(SEND_RAW, chkSendRaw.isChecked())
                .putInt(SAMPLE_RATE, getSelectedSampleRateId())
                .putBoolean(DEBUG, chkDebug.isChecked())
                .commit();
    }

    @Override
    protected void onStop() {
        super.onStop();
        unbindService(conn);
        if (udpSenderService != null && !udpSenderService.isRunning())
            stopService(new Intent(this, UdpSenderService.class));
        save();
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        getMenuInflater().inflate(R.menu.activity_main, menu);
        return true;
    }

    public void error(final String text) {
        final Activity activity = this;

        new AlertDialog.Builder(activity).setTitle("Error").setMessage(text).setNeutralButton("OK", null).show();
        if (udpSenderService != null)
            udpSenderService.stop();
    }
}
