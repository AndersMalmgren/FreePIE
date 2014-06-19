package com.freepie.android.imu.dsp;

public class Biquad implements Filter {
	public final static int LOWPASS = 0;
	public final static int BANDPASS = 1;
	public final static int HIGHPASS = 2;
	
	private double mx1, mx2, my1, my2;
	private double ma0, ma1, ma2, mb1, mb2;
	
	public Biquad(int kind, int sampleRate, float freq, float Q) {
		switch (kind) {
		case LOWPASS:
	        calcLowpass(sampleRate, freq, Q);
			break;
		case BANDPASS:
	        calcBandpass(sampleRate, freq, Q);
			break;
		case HIGHPASS:
	        calcHighpass(sampleRate, freq, Q);
			break;
		}
        mx1 = mx2 = my1 = my2 = 0;
    }
	
    public double filter(double x) {
    	if (Double.isNaN(x))
    		return x;
    	
        double result = ma0*x + ma1*mx1 + ma2*mx2 - mb1*my1 - mb2*my2;
        mx2 = mx1;
        mx1 = x;
        my2 = my1;
        my1 = result;
        return result;
    }	

    private void calcBandpass(int sampleRate, float freq, float Q) {
        double norm;
        double K;

        K = Math.tan(Math.PI * freq/sampleRate);
        norm = 1 / (1 + K / Q + K * K);
        ma0 = K / Q * norm;
        ma1 = 0;
        ma2 = -ma0;
        mb1 = 2 * (K * K - 1) * norm;
        mb2 = (1 - K / Q + K * K) * norm;
    }
    
	private void calcLowpass(int sampleRate, float freq, float Q) {
        double norm;
        double K;

        K = Math.tan(Math.PI * freq/sampleRate);
        norm = 1 / (1 + K / Q + K * K);
        ma0 = K * K * norm;
        ma1 = 2 * ma0;
        ma2 = ma0;
        mb1 = 2 * (K * K - 1) * norm;
        mb2 = (1 - K / Q + K * K) * norm;
	}
	
	private void calcHighpass(int sampleRate, float freq, float Q) {
        double norm;
        double K;

        K = Math.tan(Math.PI * freq/sampleRate);
		norm = 1 / (1 + K / Q + K * K);
        ma0 = 1 * norm;
        ma1 = -2 * ma0;
        ma2 = ma0;
        mb1 = 2 * (K * K - 1) * norm;
        mb2 = (1 - K / Q + K * K) * norm;
	}
}
