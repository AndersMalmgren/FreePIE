FreePIE
=======

Programmable Input Emulator
 
Latest downloadable installer can be found [here](http://andersmalmgren.github.io/FreePIE/)

[Please visit wiki or scripting and plugin reference manual](https://github.com/AndersMalmgren/FreePIE/wiki)

**FreePIE** (Programmable Input Emulator) is a application for bridging and emulating input devices. It has applications primarily in video gaming but can also be used for VR interaction, remote control, and other applications. A typical application might be controlling the mouse in a PC game using a Wiimote. Device control schemes are customized for specific applications by executing scripts from the FreePIE GUI. The script language is based on the **Python** syntax and offers non-programmers an easy way to interface devices.

FreePIE is very similar to the popular utility GlovePIE, but encourages open development and integration with any device. The software is designed to allow third party developers to add their own I/O plugins either through direct integration into the core library or through a separately compiled plugin mechanism.

FreePIE is licensed under GPLv2  

Changelog 1.9.611.0
* Added support for Tobii EyeX
* Stability and performance fixes for the Android APK
* Added ThreadYieldMicroSeconds to be able to run FreePIE in intervals below 1ms
* Various stability fixes

Changelog 1.8.569.0
* Latest version did not contain the fix for vjoy because of a commit miss

Changelog 1.8.567.0
* Fixed support for latest version of vjoy driver
* Cosmetic fix for code completion and Windows 8

Changelog 1.8.563.0
* Added support for MIDI devices (Read only for now)
* Python engine update
* Android APK rewritten as a service for a more stable experience
* Some small fixes like removing unused update event from mouse and keyboard plugin (They wont fire)

Changelog 1.7.528.0
* Android protocol optimized (Install new APK required)
* Android raw data accessible to scripts
* Razor hydra hemisphere tracking
* Improvements to AHRS protocol, retart of script no longer required
* Better logging while starting up script engine
* Updated YEI API
* Updated Oculus VR (SDK 0.4.3)
* Improved curve editor

Changelog 1.6.512.0
* Portable mode added, app files will be saved in program folder
* Autorun file from cmd using <file.py> /r
* Improved editor, ctrl+F among others
* Oculus VR DK2 support (SDK 4.2)
* Various stability and logging fixes 
* Added new filters (scaled deadband, ensureMapRange, stopWatch)
* Added xRotation and yRotation to JoystickPlugin

Changelog 1.5.475.0
* Spelling filters.continousRotation > filters.continuousRotation
* Added possibility to load scripts from command line
* Fixed bug with iPhone
* Added possibility to change voice with MS Speech

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
