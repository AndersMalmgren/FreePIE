package com.freepie.android.imu;

public class SampleRate {
	private int id;	
	private String name;
	
	public SampleRate(int id, String name) {
		this.id = id;
		this.name = name;
	}
	
	public int getId()  {
		return id;
	}
	
	public String toString()
	{
	    return name;
	}
}
