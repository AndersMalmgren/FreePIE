using System;
using System.Collections.Generic;
using FreePIE.Core.Plugins.SensorFusion;


namespace FreePIE.Core.Plugins.PSMove
{

    public class PSMoveController : IUpdatable
    {
        public int Index { get; set; }
        private PSMoveTracker tracker;
        private IntPtr move;

        private Vector3 position, rawPosition, centerPosition;
        private Vector3 gyroscope, accelerometer;
        private Quaternion rotation;
        private RGB_Color led;
        private char rumble;
        private uint buttons, buttonsPressed, buttonsReleased;

        // Global holder
        public PSMoveGlobal Global { get; private set; }
        public Action OnUpdate { get; set; }
        public bool GlobalHasUpdateListener { get; set; }

        // Transitional data
        private float w, x, y, z; // for Vector3 and Quaternion components
        private char r, g, b; // for RGB_Color components

        public PSMoveController(int index, PSMoveTracker tracker)
        {
            this.Index = index;
            this.tracker = tracker;
            this.Connect();

            this.position = new Vector3();
            this.rawPosition = new Vector3();
            this.centerPosition = new Vector3();
            this.rotation = new Quaternion();
            this.gyroscope = new Vector3();
            this.accelerometer = new Vector3();
            this.led = new RGB_Color();
            this.rumble = (char)0;
            this.buttons = 0;
            this.buttonsPressed = 0;
            this.buttonsReleased = 0;

            Global = new PSMoveGlobal(this);
        }

        // **************
        // Connection
        // **************

        public bool Connect()
        {
            move = PSMoveAPI.psmove_connect_by_id(Index);
            // Enable inner psmoveapi IMU fusion
            PSMoveAPI.psmove_enable_orientation(move, 1);
            // Enable positional tracking for this device
            PSMoveAPI.psmove_tracker_enable(tracker.TrackerHandle, move);
            return isConnected();
        }

        public bool isConnected()
        {
            return (move != IntPtr.Zero);
        }

        public void Disconnect()
        {
            PSMoveAPI.psmove_disconnect(move);
        }

        // **************
        // Position
        // **************

        public Vector3 Position { 
            get {
                position = rawPosition - centerPosition;
                return position; 
            } 
        }

        public void resetPosition() { 
            centerPosition.x = rawPosition.x;
            centerPosition.y = rawPosition.y;
            centerPosition.z = rawPosition.z;
        }

        // **************
        // Orientation
        // **************

        public double Yaw { get { return rotation.Yaw; } }
        public double Pitch { get { return rotation.Pitch; } }
        public double Roll { get { return rotation.Roll; } }

        public void resetOrientation() { PSMoveAPI.psmove_reset_orientation(move); }

        public Vector3 Gyroscope { get { return gyroscope; } }
        public Vector3 Accelerometer { get { return accelerometer; } }

        // **************
        // Led and Rumble
        // **************

        public int Rumble
        {
            get
            {
                return (int)rumble;
            }
            set
            {
                rumble = RGB_Color.ClamptoChar(value);
                PSMoveAPI.psmove_set_rumble(move, rumble);
            }
        }

        public RGB_Color Led { get { return led; } }

        public bool AutoLedColor
        {
            get
            {
                return (PSMoveAPI.psmove_tracker_get_auto_update_leds(tracker.TrackerHandle, move) != 0);
            }
            set
            {
                PSMoveAPI.psmove_tracker_set_auto_update_leds(tracker.TrackerHandle, move, (value) ? 1 : 0);
            }
        }

        // **************
        // Buttons
        // **************

        public bool GetButtonDown(PSMoveButton button)
        {
            return (((int)button & buttons) != 0);
        }

        public bool GetButtonUp(PSMoveButton button)
        {
            return !GetButtonDown(button);
        }

        public bool GetButtonPressed(PSMoveButton button)
        {
            return (((int)button & buttonsPressed) != 0);
        }

        public bool GetButtonReleased(PSMoveButton button)
        {
            return (((int)button & buttonsReleased) != 0);
        }

        // Update Functions

        public void Update()
        {
            UpdatePosition();
            PollInternalData(); // Polls internal IMU and buttons data
            UpdateOrientation();
            UpdateButtons();
            UpdateRumbleAndLED();
        }

        private void UpdatePosition()
        {
            // Update positional tracking info for this move
            PSMoveAPI.psmove_tracker_update(tracker.TrackerHandle, move);

            // Retrieve positional tracking data
            PSMoveAPI.psmove_fusion_get_position(tracker.FusionHandle, move,
                ref x, ref y, ref z);
            rawPosition.Update(x, y, z);
        }

        private void PollInternalData()
        {
            // Poll data (IMU and buttons)
            while (PSMoveAPI.psmove_poll(move) != 0) ;
        }

        private void UpdateOrientation()
        {
            // Orientation data
            PSMoveAPI.psmove_get_orientation(move, ref w, ref x, ref y, ref z);
            rotation.Update(w, x, y, z);

            // Gyroscope data
            PSMoveAPI.psmove_get_gyroscope_frame(move,
                PSMove.PSMove_Frame.Frame_SecondHalf,
                ref x, ref y, ref z);
            gyroscope.Update(x, y, z);

            // Accelerometer data
            PSMoveAPI.psmove_get_accelerometer_frame(move,
                PSMove.PSMove_Frame.Frame_SecondHalf,
                ref x, ref y, ref z);
            accelerometer.Update(x, y, z);
        }

        private void UpdateButtons()
        {
            // Button Events
            buttons = PSMoveAPI.psmove_get_buttons(move);
            PSMoveAPI.psmove_get_button_events(move, ref buttonsPressed, ref buttonsReleased);
        }

        private void UpdateRumbleAndLED()
        {
            // Update led color or set the automatic tracking recommended color
            UpdateLedColor();

            // Send rumble and leds notification back to the controller
            PSMoveAPI.psmove_update_leds(move);
        }

        private void UpdateLedColor()
        {
            // Check the camera tracking color update
            if (AutoLedColor)
            {
                // If the color is automatically recommended by the camera, get it now
                PSMoveAPI.psmove_tracker_get_color(tracker.TrackerHandle, move, ref r, ref g, ref b);
                led.SetColor(r, g, b);
            }
            else
            {
                // If the color is manually set, let the tracker know
                PSMoveAPI.psmove_tracker_set_camera_color(tracker.TrackerHandle, move, led.r, led.g, led.b);
            }
            // Finally set the led color
            PSMoveAPI.psmove_set_leds(move, led.r, led.g, led.b);
        }
    }

}
