FreePIE
=======

Programmable Input Emulator

Latest downloadable installer can be found [here](http://andersmalmgren.github.io/FreePIE/)

[Please visit wiki or scripting and plugin reference manual](https://github.com/AndersMalmgren/FreePIE/wiki)

Changelog 1.5.459.0
* Fixed problems with Wiimote M+ calibration
* Added Wiimote calibration success event
* Added Wiimote setting for M+ data fusion
* Added mouse wheel support
* Fixed performance issue with large script files
* Fixed so that FreePIE always inject its own Freetrack client path in registry (Freetrack should now work even if Facetracknoir is installed)
* MS Speech support addded
* Headsoft vJoy replaced with Sourceforge open source vJoy
* FreePIE should now log all managed code crashes

Changelog 1.4.433.0
* Fixed bug with TrackIR emulation for games like Euro truck sim 2 among others
* Oculus VR SDK updated

Changelog 1.3.422.0
* Added support for multiple Android devices (Requires reinstall of APK)
* Added Hydra (Sixense) SDK emulation 
* Added possibility to set thread timing, enabling the script to run at 1000hz
* Wiimote M+ and Nunchuck support added, custom library that should work with most motes (Beta)
* Yei plugin updated to work with Wireless devices
* Oculus VR SDK updated,  sensor prediction added in settings

Changelog 1.2.375.0
* Fixed mouse emulation so that it works with very small movements
* Deadband filter added that can be used to eliminate drift with the new more sensitive mouse emulation
* Added continous rotation filter that can be used to eliminate jumps in mouse emulation when passing tracker center
* Added support for VJoy

Changelog 1.1.362.0
* Added support for Oculus Rift

Changelog 1.0.355.0
* FreePIE should now work on w7 and w8 without admin rights
* Fixed bugs with Android data contract (Requires reinstall of APK)
* Freetrack client is no longer required to spoof Freetrack (Client dll is included with FreePIE)
* Added a IO plugin that third party software can use to read/write from/to FreePIE
* Added support for Carl Zeiss Cinemizer OLED
* Added support for Yei 3 Space trackers
* Added support for multiple XBox controllers
* GUI performance fixes
* Various stability fixes

Changelog 0.6.277.0
* Totaly remade GUI to use a Visual Studio like docking view
* Added a apply button to Edit Selected Curve point for usability reasons (Curve settings)
* Improved Watch window
* Improved Error window, added script linenumber for error
* Improved Console window and added a Context menu
* Various stability fixes for code completion

Changelog 0.5.249.0
* TrackIR should now work with TrackIR fixer titles
* Added Razer Hydra Support
* Dataformat bug fixed for Android APK
* Added generic button support to Mouse plugin
* Added a stopping global that scripts can listen to for tear down tasks

Changelog 0.4.237.0
* Various stability fixes for the python engine
* Various bug fixes for code completion
* Added menu shortcuts

Changelog 0.4.211.0
* Changed scripting language to Python
* Added support for TrackIR
* Added support for Android
* Added support for Xbox360 controller
* Added support for DirectX joysticks and controllers

Changelog 0.3.115.0
* Added code completion (Early beta)
* Added support for WiiMote (Only buttons)
* Added support for Hillcrest Labs Freespace IMU
* Added support for ppJoy Joystick emulation
* Added proper enum support
* Added Property support to plugins

Changelog 0.2.47.0
* Added support for iDevices via Sensor data app
* Added support for Vuzix tracker
