package com.freepie.android.imu;

import android.hardware.SensorManager;
import android.location.LocationManager;

public class TargetSettings {
	private String toIp;
	private int port;
	private SensorManager sensorManager;
	private LocationManager locationManager;
	private boolean sendOrientation;
	private boolean sendRaw;
	private boolean sendGPS;
	private int sampleRate;
	private boolean debug;
	private IDebugListener debugListener;
	
	public TargetSettings(String toIp, int port, SensorManager sensorManager, LocationManager locationManager, boolean sendOrientation, boolean sendRaw, boolean sendGPS, int sampleRate, boolean debug, IDebugListener debugListener) {
		this.toIp = toIp;
		this.port = port;
		this.sensorManager = sensorManager;
		this.locationManager = locationManager;
		this.sendOrientation = sendOrientation;
		this.sendRaw = sendRaw;
		this.sendGPS = sendGPS;
		this.sampleRate = sampleRate;
		this.debug = debug;
		this.debugListener = debugListener;
	}
	
	public String getToIp()
	{
		return toIp;
	}
	
	public int getPort()
	{
		return port;
	} 
	
	public SensorManager getSensorManager()
	{
		return sensorManager;
	} 
	
	public LocationManager getLocationManager() 
	{		
		return locationManager;
	}
	
	public boolean getSendOrientation()
	{
		return sendOrientation;
	} 
	
	public boolean getSendRaw()
	{
		return sendRaw;
	} 
	
	public boolean getSendGPS()
	{
		return sendGPS;
	} 
	
	public int getSampleRate() {
		return sampleRate;
	}
	
	public boolean getDebug() {
		return debug;
	}
	
	public IDebugListener getDebugListener() {
		return debugListener;
	}
}