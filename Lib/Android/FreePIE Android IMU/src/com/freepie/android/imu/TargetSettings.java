package com.example.freepie.android.imu;

import android.hardware.SensorManager;

public class TargetSettings {
	private String toIp;
	private int port;
	private SensorManager sensorManager;
	private boolean sendOrientation;
	private boolean sendRaw;
	private int sampleRate;
	
	public TargetSettings(String toIp, int port, SensorManager sensorManager, boolean sendOrientation, boolean sendRaw, int sampleRate) {
		this.toIp = toIp;
		this.port = port;
		this.sensorManager = sensorManager;
		this.sendOrientation = sendOrientation;
		this.sendRaw = sendRaw;
		this.sampleRate = sampleRate;
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
	
	public boolean getSendOrientation()
	{
		return sendOrientation;
	} 
	
	public boolean getSendRaw()
	{
		return sendRaw;
	} 
	
	public int getSampleRate() {
		return sampleRate;
	}
}