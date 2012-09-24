package com.example.freepie.android.imu;

import java.io.IOException;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.net.SocketAddress;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.util.concurrent.CyclicBarrier;
import java.util.concurrent.locks.Lock;

import android.R.bool;
import android.hardware.Sensor;
import android.hardware.SensorEvent;
import android.hardware.SensorEventListener;
import android.hardware.SensorManager;
import android.os.AsyncTask;
import android.util.Log;

public class UdpSenderTask {

	float[] acc = new float[3];
	float[] mag = new float[3];;
	float[] gyr = new float[3];;
	float[] imu = new float[3];;
	
	DatagramSocket socket;
	InetAddress endPoint;
	int port;
	boolean sendOrientation;
	boolean sendRaw;
	byte sendFlag;
	
	ByteBuffer buffer;
	CyclicBarrier sync;
	
	Thread worker;
	boolean running;

	public void start(TargetSettings target) {
		final SensorManager sensorManager = target.getSensorManager();
		sendRaw = target.getSendRaw();
		sendOrientation = target.getSendOrientation();
		
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
			
		SensorEventListener accListener = new SensorEventListener() {
			public void onSensorChanged(SensorEvent event) {				 			
				setArray(acc, event.values);		
			}
			@Override
			public void onAccuracyChanged(Sensor sensor, int accuracy) {				
			}
		};
		
		SensorEventListener gyrListener = new SensorEventListener() {
			public void onSensorChanged(SensorEvent event) {				 			
				setArray(gyr, event.values);		
			}
			@Override
			public void onAccuracyChanged(Sensor sensor, int accuracy) {				
			}
		};
		
		SensorEventListener magListener = new SensorEventListener() {
			public void onSensorChanged(SensorEvent event) {				 			
				setArray(mag, event.values);		
			}
			@Override
			public void onAccuracyChanged(Sensor sensor, int accuracy) {				
			}
		};
		
		/*SensorEventListener imuListener = new SensorEventListener() {
			public void onSensorChanged(SensorEvent event) {				
				setArray(imu, event.values);
			}
			@Override
			public void onAccuracyChanged(Sensor sensor, int accuracy) {				
			}
		};*/
		
		SensorEventListener allListener = new SensorEventListener() {
			public void onSensorChanged(SensorEvent event) {
				if(sendOrientation && event.accuracy != SensorManager.SENSOR_STATUS_UNRELIABLE) {
				 float R[] = new float[9];
				 float I[] = new float[9];
				 float orientation[] = new float[3];
				 boolean success = sensorManager.getRotationMatrix(R, I, acc, mag);
				 if (success) {					    
				   SensorManager.getOrientation(R, orientation);
				   setArray(imu, orientation);
				 }
				}
				
				if(sync.getNumberWaiting() > 0)
					sync.reset();			
			}
			@Override
			public void onAccuracyChanged(Sensor sensor, int accuracy) {				
			}
		};	

		
		sensorManager.registerListener(accListener,
				sensorManager.getDefaultSensor(Sensor.TYPE_ACCELEROMETER),
				SensorManager.SENSOR_DELAY_FASTEST);
		
		sensorManager.registerListener(gyrListener,
				sensorManager.getDefaultSensor(Sensor.TYPE_GYROSCOPE),
				SensorManager.SENSOR_DELAY_FASTEST);		
		
		sensorManager.registerListener(magListener,
				sensorManager.getDefaultSensor(Sensor.TYPE_MAGNETIC_FIELD),
				SensorManager.SENSOR_DELAY_FASTEST);	
		
		/*sensorManager.registerListener(imuListener,
				sensorManager.getDefaultSensor(Sensor.TYPE_ORIENTATION),
				SensorManager.SENSOR_DELAY_FASTEST);*/	
		
		sensorManager.registerListener(allListener,
				sensorManager.getDefaultSensor(Sensor.TYPE_ALL),
				SensorManager.SENSOR_DELAY_FASTEST);	
		
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
	}
	
	public void stop() {
		running = false;
	}
	
	private void setArray(float[] arr, float[] sourceArr) {
		arr[0] = sourceArr[0];
		arr[1] = sourceArr[1];
		arr[2] = sourceArr[2];
	}

	private void Send() {
		buffer.clear();			
		
		buffer.put(sendFlag);
		
		if(sendRaw) {
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
		
		if(sendOrientation) {		
			//Orientation
			buffer.putFloat(imu[0]);
			buffer.putFloat(imu[1]);
			buffer.putFloat(imu[2]);
		}		
      				
		byte[] arr = buffer.array();
	    DatagramPacket p = new DatagramPacket(arr, arr.length, endPoint, port);	    
	    try {
	    	socket.send(p);
	    }
	    catch(IOException w) {
	    	
	    }
	}
}