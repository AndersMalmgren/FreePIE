package com.freepie.android.imu;

import java.io.IOException;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.util.concurrent.CyclicBarrier;

public class UdpSenderTask {
	private TargetSettings target;
	private boolean running;
	private CyclicBarrier sync;
	private DataProducer producer;	
	private DatagramSocket socket;
	private InetAddress endPoint;
	private ByteBuffer buffer;

	public UdpSenderTask(DataProducer producer) {
		this.producer = producer;
		this.producer.setSenderTask(this);
	}

	public void start(TargetSettings target) {
		this.target = target;
		
		buffer = ByteBuffer.allocate(51);
		buffer.order(ByteOrder.LITTLE_ENDIAN);	

		sync = new CyclicBarrier(2);
		running = true;
		createWorkerThread().start();
		producer.start(target);
	}

	public void stop() {
		running = false;
		producer.stop();
	}
	
	protected void releaseSendThread() {
		if(sync.getNumberWaiting() > 0) {
			sync.reset();	
		}
	}
	
	protected Thread createWorkerThread() {
		return new Thread(new Runnable() { 
            public void run(){
        		try {	
        			endPoint = InetAddress.getByName(target.getToIp());
        			socket = new DatagramSocket();
        		}
        		catch(Exception e) {
        			target.getErrorHandler().error("Can't create endpoint", e.getMessage());
        		}
            	
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
	}
	
	protected void Send() {
		producer.fillBuffer(buffer);

		byte[] arr = buffer.array();
		if(endPoint == null)
			return;
					
	    DatagramPacket p = new DatagramPacket(arr, arr.length, endPoint, target.getPort());   
	    
	    try {
	    	socket.send(p);
	    }
	    catch(IOException w) {	    	
	    }
	}

	public void setDebug(boolean debug) {
		target.setDebug(debug);
	}
}