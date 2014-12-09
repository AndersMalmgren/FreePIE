package com.freepie.android.imu;

import android.app.Notification;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.app.Service;
import android.content.Context;
import android.content.Intent;
import android.net.wifi.WifiManager;
import android.os.Binder;
import android.os.IBinder;
import android.os.PowerManager;
import android.support.v4.app.NotificationCompat;

public class UdpSenderService extends Service {
    private final IBinder mBinder = new MyBinder();
    NotificationManager mNotificationManager;
    UdpSenderTask t = null;
    private PowerManager mPowerManager;
    private WifiManager mWifiManager;

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
    }

    @Override
    public void onDestroy() {
        stop();
        super.onDestroy();
    }

    public void stop() {
        if (t != null)
            t.stop();
        t = null;
        mNotificationManager.cancelAll();
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

    public void start(TargetSettings settings)
    {
        stop();
        enableNotification();
        t = new UdpSenderTask();
        PowerManager.WakeLock wl = mPowerManager.newWakeLock(PowerManager.PARTIAL_WAKE_LOCK, "freepie send lock");
        WifiManager.WifiLock nl = mWifiManager.createWifiLock(WifiManager.WIFI_MODE_FULL, "freepie network lock");
        t.start(settings, wl, nl);
    }

    public void setDebug(boolean flag) {
        if (t != null)
            t.setDebug(flag);
    }

    public boolean isRunning() {
        return t != null;
    }
}
