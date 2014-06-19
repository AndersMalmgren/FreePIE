package com.freepie.android.imu;

import java.util.List;

import android.content.Context;
import android.widget.ArrayAdapter;
import android.widget.Spinner;

public abstract class Util {
	private Util() {}
	
    public static <T>void populateSpinner(Context context, Spinner spinner, List<T> items, T selectedItem) {
    	ArrayAdapter<T> adapter = new ArrayAdapter<T>(context,
    			R.layout.spinner_item, items);
    	spinner.setAdapter(adapter);
    	spinner.setSelection(items.indexOf(selectedItem), false);
    }
}
