using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreePIE.Core.Contracts;
using FreePIE.Core.Plugins.OculusVR;

namespace FreePIE.Core.Plugins
{
    [GlobalType(Type = typeof(OculusGlobal))]
    public class OculusPlugin : Plugin
    {
        public override object CreateGlobal()
        {
            return new OculusGlobal(this);
        }

        public override string FriendlyName
        {
            get { return "Oculus VR"; }
        }

        public override bool GetProperty(int index, IPluginProperty property)
        {
            return false;
        }

        public override bool SetProperties(Dictionary<string, object> properties)
        {
            return true;
        }

        public override Action Start()
        {
            if (!Api.Init())
                throw new Exception("Oculus VR SDK failed to init");

            return null;
        }

        public override void Stop()
        {
            Api.Dispose();
        }

        public override void DoBeforeNextExecute()
        {
            _data = Api.Read();
            OnUpdate();
        }

        public void Center()
        {
            Api.Center();
        }

        private OculusVrData _data;

        public OculusVr6Dof HeadPose { get { return _data.HeadPose; } }

        public OculusVr6Dof LeftTouchPose{ get { return _data.LeftTouchPose; } }

        public OculusVr6Dof RightTouchPose{ get { return _data.RightTouchPose; } }

        public float LeftTrigger{ get { return _data.LeftTrigger; } }

        public float RightTrigger{ get { return _data.RightTrigger; } }

        public float LeftGrip{ get { return _data.LeftGrip; } }

        public float RightGrip{ get { return _data.RightGrip; } }

        public Pointf LeftStick{ get { return _data.LeftStick; } }

        public Pointf RightStick{ get { return _data.RightStick; } }

        public OvrControllerType ControllerType{ get { return (OvrControllerType) _data.ControllerType; } }

        public OvrStatus RightTouchStatus { get { return (OvrStatus) _data.RightTouchStatus; } }
        public OvrStatus LeftTouchStatus { get { return (OvrStatus) _data.LeftTouchStatus; } }
        public OvrStatus HeadStatus { get { return (OvrStatus) _data.HeadStatus; } }

        public bool IsHmdMounted { get { return _data.IsHmdMounted > 0; } }
        public OvrTouchButton Buttons{ get { return (OvrTouchButton) _data.Buttons; } }

        public OvrTouch Touches{ get { return (OvrTouch) _data.Touches; } }
    }

    [Global(Name = "oculusVR")]
    public class OculusGlobal : UpdateblePluginGlobal<OculusPlugin>
    {
        public OculusGlobal(OculusPlugin plugin) : base(plugin){}

        public OvrTouchButton buttons { get { return plugin.Buttons; } }
        public OvrTouch touches { get { return plugin.Touches; } }

        public OculusVr6Dof headPose { get { return plugin.HeadPose; } }
        public OculusVr6Dof leftTouchPose{ get { return plugin.LeftTouchPose; } }
        public OculusVr6Dof rightTouchPose{ get { return plugin.RightTouchPose; } }

        
        public OvrStatus headStatus { get { return plugin.HeadStatus; } }
        public OvrStatus leftTouchStatus { get { return plugin.LeftTouchStatus; } }
        public OvrStatus rightTouchStatus { get { return plugin.RightTouchStatus; } }

        public bool isWorn { get { return plugin.IsHmdMounted; } }
        public bool isHeadTracking { get { return plugin.HeadStatus == (OvrStatus.OrientationTracked | OvrStatus.PositionTracked);} }
        public bool isLeftTouchTracking { get { return plugin.LeftTouchStatus == (OvrStatus.OrientationTracked | OvrStatus.PositionTracked); } }

        public bool isRightTouchTracking { get { return plugin.RightTouchStatus == (OvrStatus.OrientationTracked | OvrStatus.PositionTracked); } }

        #region Buttons
        

        public bool a { get { return (plugin.Buttons & OvrTouchButton.A) > 0; } }
        public bool b { get { return (plugin.Buttons & OvrTouchButton.B) > 0; } }
        public bool x { get { return (plugin.Buttons & OvrTouchButton.X) > 0; } }
        public bool y { get { return (plugin.Buttons & OvrTouchButton.Y) > 0; } }

        public bool leftThumb { get { return (plugin.Buttons & OvrTouchButton.LeftThumb) > 0; } }

        public bool rightThumb { get { return (plugin.Buttons & OvrTouchButton.RightThumb) > 0; } }
        
        public float leftTrigger { get { return plugin.LeftTrigger; } }
        public float rightTrigger { get { return plugin.RightTrigger; } }

        public float leftGrip { get { return plugin.LeftGrip; } }
        public float rightGrip { get { return plugin.RightGrip; } }

        public float leftStickX { get { return plugin.LeftStick.x; } }
        public float rightStickX { get { return plugin.RightStick.x; } }

        public float leftStickY { get { return -plugin.LeftStick.y; } }
        public float rightStickY{ get { return -plugin.RightStick.y; } }


        public bool enter { get { return (plugin.Buttons & OvrTouchButton.Enter) > 0; } }

        public bool home { get { return (plugin.Buttons & OvrTouchButton.Home) > 0; } }

        #endregion //Buttons

        #region Touches

        

        public bool touchingA { get { return (plugin.Touches & OvrTouch.A) > 0; } }
        public bool touchingB { get { return (plugin.Touches & OvrTouch.B) > 0; } }
        public bool touchingX { get { return (plugin.Touches & OvrTouch.X) > 0; } }
        public bool touchingY { get { return (plugin.Touches & OvrTouch.Y) > 0; } }

        public bool touchingLeftThumb { get { return (plugin.Touches & OvrTouch.LThumb) > 0; } }
        public bool touchingRightThumb { get { return (plugin.Touches & OvrTouch.RThumb) > 0; } }


        public bool touchingLeftThumbRest { get { return (plugin.Touches & OvrTouch.LThumbRest) > 0; } }
        public bool touchingRightThumbRest { get { return (plugin.Touches & OvrTouch.RThumbRest) > 0; } }

        public bool touchingLeftTrigger { get { return (plugin.Touches & OvrTouch.LTrigger) > 0; } }
        public bool touchingRightTrigger { get { return (plugin.Touches & OvrTouch.RTrigger) > 0; } }
        //gestures
        public bool leftIndexPointing { get { return (plugin.Touches & OvrTouch.LIndexPointing) > 0; } }
        public bool rightIndexPointing { get { return (plugin.Touches & OvrTouch.RIndexPointing) > 0; } }
        public bool leftThumbUp{ get { return (plugin.Touches & OvrTouch.LThumbUp) > 0; } }
        public bool rightThumbUp{ get { return (plugin.Touches & OvrTouch.RThumbUp) > 0; } }

        
        #endregion // Touches
        public void center()
        {
            plugin.Center();
        }
    }
}
