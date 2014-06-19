package com.freepie.android.imu.dsp;


public class FilterArray {
	static public interface Builder {
		Filter create();
	}
	
	private Filter[] filter;
	public FilterArray(int dimension, Builder builder) {
		filter = new Filter[dimension];
		for (int i = 0; i < dimension; i++) {
			filter[i] = builder.create();
		}
	}
	
	public void filter(float[] x, float[] y) {
		for (int i = 0; i < filter.length; i++) {
			y[i] = (float) filter[i].filter(x[i]);
		}
	}
}
