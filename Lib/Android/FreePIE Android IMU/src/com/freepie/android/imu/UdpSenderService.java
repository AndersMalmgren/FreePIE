package com.freepie.android.imu;

import android.app.Notification;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.app.Service;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.hardware.Sensor;
import android.hardware.SensorEvent;
import android.hardware.SensorEventListener;
import android.hardware.SensorManager;
import android.net.wifi.WifiManager;
import android.os.Binder;
import android.os.IBinder;
import android.os.PowerManager;
import android.support.v4.app.NotificationCompat;
import android.util.Log;

import java.io.IOException;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;

public class UdpSenderService extends Service implements SensorEventListener {
    private final IBinder mBinder = new MyBinder();
    NotificationManager mNotificationManager;
    private PowerManager mPowerManager;
    private WifiManager mWifiManager;

    private static final byte SEND_RAW = 0x01;
    private static final byte SEND_ORIENTATION = 0x02;
    private static final byte SEND_NONE = 0x00;

    float[] acc = new float[]{0, 0, 0};
    float[] mag = new float[]{0, 0, 0};
    float[] gyr = new float[]{0, 0, 0};
    float[] imu = new float[]{0, 0, 0};

    float[] rotationVector = new float[3];
    final float[] rotationMatrix = new float[16];

    float[] R_ = new float[] {0,0,0, 0,0,0, 0,0,0};
    float[] I = new float[] {0,0,0, 0,0,0, 0,0,0};

    DatagramSocket socket;
    byte deviceIndex;
    boolean sendOrientation;
    boolean sendRaw;
    private int sampleRate;
    private SensorManager sensorManager;

    ByteBuffer buffer;

    Thread worker;
    volatile boolean running;
    private boolean hasGyro;
    private WifiManager.WifiLock wifiLock;
    private PowerManager.WakeLock wakeLock;
    private DatagramPacket p = new DatagramPacket(new byte[] {}, 0);

    private String lastError;

    private BroadcastReceiver screen_off_receiver = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
            if (intent.getAction().equals(Intent.ACTION_SCREEN_OFF)) {
                register_sensors();
            }
        }
    };

    public void register_sensors() {
        sensorManager.unregisterListener(this);
        sensorManager.registerListener(this,
                sensorManager.getDefaultSensor(Sensor.TYPE_ACCELEROMETER),
                sampleRate);
        if (hasGyro)
            sensorManager.registerListener(this,
                    sensorManager.getDefaultSensor(Sensor.TYPE_GYROSCOPE),
                    sampleRate);
        sensorManager.registerListener(this,
                sensorManager.getDefaultSensor(Sensor.TYPE_MAGNETIC_FIELD),
                sampleRate);
    }

    public String getLastError() {
        synchronized (this) {
            return lastError;
        }
    }

    private void setLastError(String e) {
        synchronized (this) {
            lastError = e;
        }
    }

    public String debug(float[] acc_, float[] mag_, float[] gyr_, float[] imu_)
    {
        synchronized (this)
        {
            System.arraycopy(acc, 0, acc_, 0, 3);
            System.arraycopy(mag, 0, mag_, 0, 3);
            System.arraycopy(gyr, 0, gyr_, 0, 3);
            System.arraycopy(imu, 0, imu_, 0, 3);

            String err = lastError;

            lastError = null;

            return err;
        }
    }

    @Override
    public IBinder onBind(Intent intent) {
        return mBinder;
    }

    public class MyBinder extends Binder {
        UdpSenderService getService() {
            return UdpSenderService.this;
        }
    }

    @Override
    public void onCreate() {
        super.onCreate();
        mNotificationManager = (NotificationManager) getSystemService(Context.NOTIFICATION_SERVICE);
        mPowerManager = (PowerManager) getSystemService(Context.POWER_SERVICE);
        mWifiManager = (WifiManager) getSystemService(Context.WIFI_SERVICE);
        sensorManager = (SensorManager) getSystemService(Context.SENSOR_SERVICE);
    }

    @Override
    public void onDestroy() {
        stop();
        super.onDestroy();
    }

    public void stop() {
        try {
            unregisterReceiver(screen_off_receiver);
        } catch (Exception ignored) {}
        if (wakeLock != null) {
            wakeLock.release();
            wakeLock = null;
        }
        if (wifiLock != null) {
            wifiLock.release();
            wifiLock = null;
        }
        mNotificationManager.cancelAll();
        if (sensorManager != null)
            sensorManager.unregisterListener(this);
        running = false;
        synchronized (this) {
            notifyAll();
        }
        if (worker != null) {
            try {
                worker.join();
            } catch (InterruptedException e) {
            }
        }
    }

    @Override
    public void onAccuracyChanged(Sensor sensor, int accuracy) {
    }

    private void enableNotification() {
        NotificationCompat.Builder mBuilder = new NotificationCompat.Builder(this);
        mBuilder.setContentTitle("FreePIE IMU sender");
        mBuilder.setContentInfo("Sending orientation data");
        mBuilder.setSmallIcon(R.drawable.ic_launcher);
        Intent intent = new Intent(this, MainActivity.class);
        Notification n = mBuilder.build();
        n.contentIntent = PendingIntent.getActivity(this, 0, intent, 0);
        n.flags |= Notification.FLAG_NO_CLEAR|Notification.FLAG_ONGOING_EVENT;
        int notify_id = 42;
        mNotificationManager.notify(notify_id, n);
    }

    public void start(TargetSettings target)
    {
        stop();
        enableNotification();
        PowerManager.WakeLock wl = mPowerManager.newWakeLock(PowerManager.PARTIAL_WAKE_LOCK, "freepie send lock");
        WifiManager.WifiLock nl = mWifiManager.createWifiLock(WifiManager.WIFI_MODE_FULL, "freepie network lock");
        registerReceiver(screen_off_receiver, new IntentFilter(Intent.ACTION_SCREEN_OFF));

        deviceIndex = target.getDeviceIndex();
        final int port = target.getPort();
        final String ip = target.getToIp();

        sendRaw = target.getSendRaw();
        sendOrientation = target.getSendOrientation();
        sampleRate = target.getSampleRate();

        buffer = ByteBuffer.allocate(50);
        buffer.order(ByteOrder.LITTLE_ENDIAN);

        final UdpSenderService this_ = this;

        worker = new Thread(new Runnable() {
            public void run(){
                try {
                    socket = new DatagramSocket();
                    p.setAddress(InetAddress.getByName(ip));
                    p.setPort(port);
                }
                catch(Exception e) {
                    setLastError("Can't create endpoint " + e.getMessage());
                    return;
                }

                running = true;

                while(running) {
                    try {
                        synchronized (this_) {
                            this_.wait();
                            Send();
                        }
                    } catch (InterruptedException e) {
                    }
                }
                try  {
                    socket.disconnect();
                }
                catch(Exception e)  {}
            }
        });
        worker.start();

        hasGyro = sensorManager.getDefaultSensor(Sensor.TYPE_GYROSCOPE) != null;

        register_sensors();

        wakeLock = wl;
        wifiLock = nl;

        wifiLock.acquire();
        wakeLock.acquire();
    }

    private byte getFlagByte(boolean raw, boolean orientation) {
        return (byte)((raw ? SEND_RAW : SEND_NONE) |
                (orientation ? SEND_ORIENTATION : SEND_NONE));
    }

    private void Send() {
        buffer.clear();

        buffer.put(deviceIndex);
        buffer.put(getFlagByte(sendRaw, sendOrientation));

        if (sendRaw) {
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
        }

        if (sendOrientation) {
            buffer.putFloat(imu[0]);
            buffer.putFloat(imu[1]);
            buffer.putFloat(imu[2]);
        }

        byte[] arr = buffer.array();

        p.setData(arr, 0, buffer.position());

        try {
            socket.send(p);
        }
        catch(IOException w) {
        }
    }

    public boolean isRunning() {
        return running;
    }

    public void onSensorChanged(SensorEvent sensorEvent) {
        synchronized (this) {
            switch (sensorEvent.sensor.getType()) {
                case Sensor.TYPE_ACCELEROMETER:
                    System.arraycopy(sensorEvent.values, 0, acc, 0, 3);
                    break;
                case Sensor.TYPE_MAGNETIC_FIELD:
                    System.arraycopy(sensorEvent.values, 0, mag, 0, 3);
                    break;
                case Sensor.TYPE_GYROSCOPE:
                    System.arraycopy(sensorEvent.values, 0, gyr, 0, 3);
                    break;
                case Sensor.TYPE_ROTATION_VECTOR:
                    System.arraycopy(sensorEvent.values, 0, rotationVector, 0, 3);
                    break;
            }

            if (sendOrientation) {
                if (!hasGyro) {
                    boolean ignored = SensorManager.getRotationMatrix(R_, I, acc, mag);
                    SensorManager.getOrientation(R_, imu);
                } else {
                    SensorManager.getRotationMatrixFromVector(rotationMatrix, rotationVector);
                    SensorManager.getOrientation(rotationMatrix, imu);
                }
            }

            notifyAll();
        }
    }
}
