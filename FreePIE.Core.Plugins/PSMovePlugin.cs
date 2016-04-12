using System;
using System.Collections.Generic;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.Globals;
using FreePIE.Core.Plugins.SensorFusion;


namespace FreePIE.Core.Plugins
{
    [GlobalType(Type = typeof(PSMoveGlobal), IsIndexed = true)]
    public class PSMovePlugin : Plugin
    {
        private Dictionary<int, PSMove.PSMoveController> holders;
        private PSMove.PSMoveTracker tracker;

        private bool useDimming;
        private float customDimming;



        public override object CreateGlobal()
        {
            tracker = new PSMove.PSMoveTracker();
            holders = new Dictionary<int, PSMove.PSMoveController>();
            return new GlobalIndexer<PSMoveGlobal>(Create);
        }

        public PSMoveGlobal Create(int index)
        {
            var holder = new PSMove.PSMoveController(index, tracker);
            holders[index] = holder;
            return holder.Global;
        }

        public override string FriendlyName
        {
            get { return "PSMove"; }
        }

        public override Action Start()
        {
            // Start the camera tracking
            tracker.Start();
            // Set custom tracker properties
            if (useDimming)
            {
                tracker.SetDimming(customDimming);
            }
            return null;
        }

        public override void Stop() {
            // Disconnect moves
            foreach (var holder in holders.Values)
            {
                holder.Disconnect();
            }
            holders.Clear();

            // Stop the camera tracking
            tracker.Stop();
        }

        public override bool GetProperty(int index, IPluginProperty property)
        {
            switch (index)
            {
                case 0:
                    property.Name = "usedimming";
                    property.Caption = "Manual dimming";
                    property.DefaultValue = true;
                    property.HelpText = "Light bulb properties are adjusted automatically";
                    return true;
                case 1:
                    property.Name = "dimming";
                    property.Caption = "Dimming value";
                    property.DefaultValue = (float)1.0;
                    property.HelpText = "Custom Light bulb dimming effect";
                    return true;
            }
            return false;
        }

        public override bool SetProperties(Dictionary<string, object> properties)
        {
            this.useDimming = (bool)properties["usedimming"];
            this.customDimming = (float)properties["dimming"];
            return false;
        }

        public override void DoBeforeNextExecute() {
            // Update Camera Image
            tracker.UpdateImage();

            // Update every move controller data
            foreach (var holder in holders.Values)
            {
                // Update the data (provisionally non-threaded update here)
                holder.Update();
                // Trigger the python event
                holder.OnUpdate();
            }
        }
    }

   
    [Global(Name = "psmove")]
    public class PSMoveGlobal : UpdateblePluginGlobal<PSMove.PSMoveController>
    {
        public PSMoveGlobal(PSMove.PSMoveController plugin) : base(plugin) { }

        public PSMove.Vector3 position { get { return plugin.Position; } }

        public void resetPosition() { plugin.resetPosition(); }

        public double yaw { get { return plugin.Yaw; } }
        public double pitch { get { return plugin.Pitch; } }
        public double roll { get { return plugin.Roll; } }

        public PSMove.Vector3 gyro { get { return plugin.Gyroscope; } }
        public PSMove.Vector3 accel { get { return plugin.Accelerometer; } }

        public void resetOrientation() { plugin.resetOrientation(); }

        public int rumble  { get { return plugin.Rumble; } set { plugin.Rumble = value; } }
        public PSMove.RGB_Color led { get { return plugin.Led; } }
        public bool autoLedColor { get { return plugin.AutoLedColor; } set { plugin.AutoLedColor = value; } }

        public bool getButtonDown(PSMove.PSMoveButton button) { return plugin.GetButtonDown(button); }
        public bool getButtonUp(PSMove.PSMoveButton button) { return plugin.GetButtonUp(button); } 
        public bool getButtonPressed(PSMove.PSMoveButton button) { return plugin.GetButtonPressed(button); }
        public bool getButtonReleased(PSMove.PSMoveButton button) { return plugin.GetButtonReleased(button); }

        public int trigger { get { return plugin.Trigger; } }
    }
}
