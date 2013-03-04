package com.freepie.android.imu;

import java.io.IOException;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.util.concurrent.CyclicBarrier;
import android.hardware.Sensor;
import android.hardware.SensorEvent;
import android.hardware.SensorEventListener;
import android.hardware.SensorManager;
import android.location.Location;
import android.location.LocationListener;
import android.location.LocationManager;
import android.os.Bundle;

public class UdpSenderTask implements SensorEventListener, LocationListener {

	float[] acc;
	float[] mag;
	float[] gyr;
	float[] imu;
	Location lastLocation; 
	float[] distanceBetween = new float[2];
	
	float[]  orientation;
	final float[] rotationMatrix = new float[16];
	
	DatagramSocket socket;
	InetAddress endPoint;
	int port;
	boolean sendOrientation;
	boolean sendRaw;
	boolean sendGPS;
	boolean debug;
	private int sampleRate;
	private IDebugListener debugListener;
	private SensorManager sensorManager;
	private LocationManager locationManager;
	
	byte sendFlag;
	
	ByteBuffer buffer;
	CyclicBarrier sync;
	
	Thread worker;
	boolean running;

	public void start(TargetSettings target) {
		sensorManager = target.getSensorManager();		
		locationManager = target.getLocationManager();
		sendRaw = target.getSendRaw();
		sendGPS = target.getSendGPS();
		sendOrientation = target.getSendOrientation();
		
		sampleRate = target.getSampleRate();		
		debug = target.getDebug();
		debugListener = target.getDebugListener();
		
		sendFlag = (byte)((sendRaw ? 0x01 : 0x00) | (sendOrientation ? 0x02 : 0x00)); 
		
		sync = new CyclicBarrier(2);
			
		buffer = ByteBuffer.allocate(49);
		buffer.order(ByteOrder.LITTLE_ENDIAN);
		
		try {	
			endPoint = InetAddress.getByName(target.getToIp());
			port = target.getPort();
			socket = new DatagramSocket();
		}
		catch(Exception e) {			
		}
					
		running = true;
		worker = new Thread(new Runnable() { 
            public void run(){
        		while(running) {
        			try {
        			sync.await();
        			} catch(Exception e) {}
        			
        			Send();
        		}
        		try  {
        			socket.disconnect();
        		}
        		catch(Exception e)  {}
        	}
		});	
		worker.start();
		
		if(sendRaw) {
			sensorManager.registerListener(this,
					sensorManager.getDefaultSensor(Sensor.TYPE_ACCELEROMETER),
					sampleRate);
			
			sensorManager.registerListener(this,
					sensorManager.getDefaultSensor(Sensor.TYPE_GYROSCOPE),
					sampleRate);		
			
			sensorManager.registerListener(this,
					sensorManager.getDefaultSensor(Sensor.TYPE_MAGNETIC_FIELD),
					sampleRate);
		}
		if(sendOrientation)
			sensorManager.registerListener(this,
					sensorManager.getDefaultSensor(Sensor.TYPE_ROTATION_VECTOR),
					sampleRate);
		
		if(sendGPS)
			locationManager.requestLocationUpdates(LocationManager.GPS_PROVIDER, 
                       0, 0, this);   
		
	}
	
	public void onSensorChanged(SensorEvent sensorEvent) {
	    switch (sensorEvent.sensor.getType()) {  
	        case Sensor.TYPE_ACCELEROMETER:
	            acc = sensorEvent.values.clone();
	            break;
	        case Sensor.TYPE_MAGNETIC_FIELD:
	            mag = sensorEvent.values.clone();
	            break;
	            
	        case Sensor.TYPE_GYROSCOPE:
	            gyr = sensorEvent.values.clone();
	            break;
	        case Sensor.TYPE_ROTATION_VECTOR:
	        	orientation = sensorEvent.values.clone();
	        	break;
	    }	
	    
       if(sendOrientation && orientation != null) {
            SensorManager.getRotationMatrixFromVector(rotationMatrix , orientation);
            SensorManager.getOrientation(rotationMatrix, orientation);
            imu = orientation;
       }
	    	    
	    if(debug && acc != null && gyr != null && mag != null)
	    	debugListener.debugRaw(acc, gyr, mag);
	    
	    if(debug && imu != null)
	    	debugListener.debugImu(imu);	
	    
		if(sync.getNumberWaiting() > 0)
			sync.reset();			
	}
	
	@Override
	public void onAccuracyChanged(Sensor sensor, int accuracy) {	
	}
	
	public void stop() {
		running = false;
		sensorManager.unregisterListener(this);		
		locationManager.removeUpdates(this);
	}

	private void Send() {
		buffer.clear();			
		
		buffer.put(sendFlag);
		
		if(sendRaw && acc != null && mag != null && gyr != null) {
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
		
		if(sendOrientation && imu != null) {		
			//Orientation
			buffer.putFloat(imu[0]);
			buffer.putFloat(imu[1]);
			buffer.putFloat(imu[2]);
			imu = null;
		}		
      				
		byte[] arr = buffer.array();
	    DatagramPacket p = new DatagramPacket(arr, arr.length, endPoint, port);	    
	    try {
	    	socket.send(p);
	    }
	    catch(IOException w) {
	    	
	    }
	}

	public void setDebug(boolean debug) {
		this.debug = debug;		
	}

	@Override
	public void onLocationChanged(Location location) {
		if(lastLocation != null) {			
			Location.distanceBetween(lastLocation.getLatitude(), lastLocation.getLongitude(), location.getLatitude(), location.getLongitude(), distanceBetween);
		    
			if(debug)
		    	debugListener.debugGPS(distanceBetween);
		}
		
		lastLocation = location;		
	}

	@Override
	public void onProviderDisabled(String provider) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void onProviderEnabled(String provider) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void onStatusChanged(String provider, int status, Bundle extras) {
		// TODO Auto-generated method stub
		
	}
}