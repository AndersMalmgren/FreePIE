package com.freepie.android.imu;

import java.nio.ByteBuffer;
import android.app.Activity;
import android.content.SharedPreferences;
import android.content.SharedPreferences.Editor;

public abstract class DataProducer {
	public static final byte SEND_RAW = 0x01;
	public static final byte SEND_ORIENTATION = 0x02;
	public static final byte SEND_NONE = 0x00;
	
	private UdpSenderTask mSenderTask;
	
	public DataProducer() {		
	}
	
	public void setSenderTask(UdpSenderTask udpSenderTask) {
		mSenderTask = udpSenderTask;
	}
	
	protected void notifySenderTask() {
		mSenderTask.releaseSendThread();
	}
	
	public abstract void start(TargetSettings target);
	public abstract void stop();
	public abstract void fillBuffer(ByteBuffer buffer);

	protected byte getFlagByte(boolean raw, boolean orientation) {
		return (byte)((raw ? SEND_RAW : SEND_NONE) | (orientation ? SEND_ORIENTATION : SEND_NONE)); 
	}
	
	public abstract void onCreateOptions(Activity context, SharedPreferences preferences);
	public abstract int getOptionsLayoutId();
	public abstract Editor savePreferences(Editor editor);
}
