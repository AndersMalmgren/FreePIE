package com.freepie.android.imu;

import android.hardware.SensorManager;

public class TargetSettings {
	private String toIp;
	private int port;
	private byte deviceIndex;
	private SensorManager sensorManager;
	private boolean debug;
	private IDebugListener debugListener;
	private IErrorHandler errorHandler;
	
	public TargetSettings(String toIp, int port, byte deviceIndex, SensorManager sensorManager, boolean debug, IDebugListener debugListener, IErrorHandler errorHandler) {
		this.toIp = toIp;
		this.port = port;
		this.deviceIndex = deviceIndex;
		this.sensorManager = sensorManager;
		this.debug = debug;
		this.debugListener = debugListener;
		this.errorHandler = errorHandler;
	}
	
	public String getToIp()
	{
		return toIp;
	}
	
	public int getPort()
	{
		return port;
	} 
	
	public byte getDeviceIndex() {
		return deviceIndex;
	}
	
	public SensorManager getSensorManager()
	{
		return sensorManager;
	}	
	
	public boolean getDebug() {
		return debug;
	}
	
	public IDebugListener getDebugListener() {
		return debugListener;
	}
	
	public IErrorHandler getErrorHandler() {
		return this.errorHandler;
	}

	public void setDebug(boolean debug) {
		this.debug = debug;
	}
}