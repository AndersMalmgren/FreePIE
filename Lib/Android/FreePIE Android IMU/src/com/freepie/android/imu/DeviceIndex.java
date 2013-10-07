package com.freepie.android.imu;

public class DeviceIndex {
	private byte index;
	public DeviceIndex(byte index) {
		this.index = index;
	}
	
	public byte getIndex()  {
		return index;
	}
	
	public String toString()
	{
	    return String.format("Device index %d", index);
	}
}
