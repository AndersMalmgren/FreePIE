package com.freepie.android.imu;

import java.io.IOException;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.util.Vector;
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

	private static final byte SEND_RAW = 0x01;
	private static final byte SEND_ORIENTATION = 0x02;
	private static final byte SEND_GPS = 0x04;
	private static final byte SEND_NONE = 0x00;
	
	float[] acc;
	float[] mag;
	float[] gyr;
	float[] imu;
	Location lastLocation; 
	float[] distanceBetween = new float[2];
	boolean newGpsDataToSend = false;
	
	float[]  rotationVector;
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
		
		sendFlag = getFlagByte(sendRaw, sendOrientation, sendGPS);
		
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
	
	private byte getFlagByte(boolean raw, boolean orientation, boolean gps) {
		return (byte)((raw ? SEND_RAW : SEND_NONE) | 
				(orientation ? SEND_ORIENTATION : SEND_NONE) |
				(gps ? SEND_GPS : SEND_NONE)); 
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
	        	rotationVector = sensorEvent.values.clone();
	        	break;
	    }	
	    
       if(sendOrientation && rotationVector != null) {
            SensorManager.getRotationMatrixFromVector(rotationMatrix , rotationVector);
            SensorManager.getOrientation(rotationMatrix, rotationVector);
            imu = rotationVector;
       }
	    	    
	    if(debug && acc != null && gyr != null && mag != null)
	    	debugListener.debugRaw(acc, gyr, mag);
	    
	    if(debug && imu != null)
	    	debugListener.debugImu(imu);	
	    
	    releaseSendThread();
	}
	
	private void releaseSendThread() {
		if(sync.getNumberWaiting() > 0)
			sync.reset();	
	}
	
	@Override
	public void onAccuracyChanged(Sensor sensor, int accuracy) {	
	}
	
	@Override
	public void onLocationChanged(Location location) {
		if(lastLocation != null) {			
			Location.distanceBetween(lastLocation.getLatitude(), lastLocation.getLongitude(), location.getLatitude(), location.getLongitude(), distanceBetween);
			newGpsDataToSend = true;
			
			if(debug)
		    	debugListener.debugGPS(distanceBetween);
			
			releaseSendThread();
		}
		
		lastLocation = location;		
	}
	
	public void stop() {
		running = false;
		sensorManager.unregisterListener(this);		
		locationManager.removeUpdates(this);
	}

	private void Send() {
		float[] localAcc = acc;
		float[] localGyr = gyr;
		float[] localMag = mag;
		
		float[] localImu = imu;
		float[] localDistanceBetween = distanceBetween;
		
		buffer.clear();			
		
		buffer.put(sendFlag);
		
		boolean raw = sendRaw && localAcc != null && localGyr != null && localMag != null;
		boolean orientation = sendOrientation && localImu != null;
		boolean gps = newGpsDataToSend;
		
		buffer.put(getFlagByte(raw, orientation, gps));
		
		if(raw) {
			//Acc
			buffer.putFloat(localAcc[0]);
			buffer.putFloat(localAcc[1]);
			buffer.putFloat(localAcc[2]);
			
			//Gyro
			buffer.putFloat(localGyr[0]);
			buffer.putFloat(localGyr[1]);
			buffer.putFloat(localGyr[2]);	
			
			//Mag
			buffer.putFloat(localMag[0]);
			buffer.putFloat(localMag[1]);
			buffer.putFloat(localMag[2]);
			
			acc = null;
			mag = null;
			gyr = null;
		}
		
		if(orientation) {			
			buffer.putFloat(localImu[0]);
			buffer.putFloat(localImu[1]);
			buffer.putFloat(localImu[2]);
			imu = null;
		}
		
		if(gps) {		
			buffer.putFloat(localDistanceBetween[0]);
			buffer.putFloat(localDistanceBetween[1]);
			newGpsDataToSend = false;
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