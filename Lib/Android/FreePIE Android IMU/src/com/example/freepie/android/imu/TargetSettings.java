package com.example.freepie.android.imu;

import android.hardware.SensorManager;

public class TargetSettings {
	private String toIp;
	private int port;
	private SensorManager sensorManager;
	private boolean sendOrientation;
	private boolean sendRaw;
	
	public TargetSettings(String toIp, int port, SensorManager sensorManager, boolean sendOrientation, boolean sendRaw) {
		this.toIp = toIp;
		this.port = port;
		this.sensorManager = sensorManager;
		this.sendOrientation = sendOrientation;
		this.sendRaw = sendRaw;
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
}