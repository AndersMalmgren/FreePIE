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

        public OculusVr6Dof Head {get { return _data.Head; }}

        public OculusVr6Dof leftHand{get { return _data.LeftHand; }}

        public OculusVr6Dof rightHand{get { return _data.RightHand; }}

        public float LTrigger{get { return _data.LTrigger; }}

        public float RTrigger{get { return _data.RTrigger; }}

        public float LGrip{get { return _data.LGrip; }}

        public float RGrip{get { return _data.RGrip; }}

        public Pointf Lstick{get { return _data.Lstick; }}

        public Pointf Rstick{get { return _data.Rstick; }}

        public ovrControllerType ControllerType{get { return (ovrControllerType) _data.ControllerType; }}

        public ovrStatus StatusRightHand { get { return (ovrStatus) _data.StatusRightHand; } }
        public ovrStatus StatusLeftHand { get { return (ovrStatus) _data.StatusLeftHand; } }
        public ovrStatus StatusHead { get { return (ovrStatus) _data.StatusHead; } }

        public bool isHmdMounted { get { return _data.HmdMounted > 0; } }
        public ovrTouchButton Buttons{get { return (ovrTouchButton) _data.Buttons; }}

        public ovrTouch Touches{get { return (ovrTouch) _data.Touches; }}
    }

    [Global(Name = "oculusVR")]
    public class OculusGlobal : UpdateblePluginGlobal<OculusPlugin>
    {
        public OculusGlobal(OculusPlugin plugin) : base(plugin){}
        
        public OculusVr6Dof head { get { return plugin.Head; } }
        public OculusVr6Dof leftHand{get { return plugin.leftHand; }}
        public OculusVr6Dof rightHand{get { return plugin.rightHand; }}

        public bool isHmdMounted { get { return plugin.isHmdMounted; } }
        public ovrStatus headStatus{get { return plugin.StatusHead; }}

        public ovrStatus leftHandStatus{get { return plugin.StatusLeftHand; }}

        public ovrStatus rightHandStatus{get { return plugin.StatusRightHand; }}

        public bool headIsTracking { get { return plugin.StatusHead == (ovrStatus.OrientationTracked | ovrStatus.PositionTracked);} }
        public bool leftHandIsTracking { get { return plugin.StatusLeftHand == (ovrStatus.OrientationTracked | ovrStatus.PositionTracked); } }

        public bool rightHandIsTracking { get { return plugin.StatusRightHand == (ovrStatus.OrientationTracked | ovrStatus.PositionTracked); } }

        #region Buttons
        public ovrTouchButton buttons{get { return plugin.Buttons; }}

        #region Right Touch Controller
        
        public bool A { get { return (plugin.Buttons & ovrTouchButton.A) > 0; } }

        public bool B { get { return (plugin.Buttons & ovrTouchButton.B) > 0; } }

        public bool RThumb{get { return (plugin.Buttons & ovrTouchButton.RThumb) > 0; }}

        public bool Home{get { return (plugin.Buttons & ovrTouchButton.Home) > 0; }}

        public float RTrigger{get { return plugin.RTrigger; }}

        public float RGrip{get { return plugin.RGrip; }}

        public float RX{get { return plugin.Rstick.x; }}

        public float RY{get { return -plugin.Rstick.y; }}

        #endregion //Right Touch Controller

        #region Left Touch Controller
        

        public bool X{get { return (plugin.Buttons & ovrTouchButton.X) > 0; }}

        public bool Y{get { return (plugin.Buttons & ovrTouchButton.Y) > 0; }}

        public bool LThumb{get { return (plugin.Buttons & ovrTouchButton.LThumb) > 0; }}

        public bool Enter{get { return (plugin.Buttons & ovrTouchButton.Enter) > 0; }}

        public float LTrigger{get { return plugin.LTrigger; }}

        public float LGrip{get { return plugin.LGrip; }}

        public float LX{get { return plugin.Lstick.x; }}

        public float LY{get { return -plugin.Lstick.y; }}

        #endregion //Left Touch Controller

        #endregion //Buttons

        #region Touches

        public ovrTouch touches{get { return plugin.Touches; }}

        #region Left Hand
        public bool touchingX{get { return (plugin.Touches & ovrTouch.X) > 0; }}

        public bool touchingY{get { return (plugin.Touches & ovrTouch.Y) > 0; }}

        public bool touchingLThumb{get { return (plugin.Touches & ovrTouch.LThumb) > 0; }}

        public bool touchingLThumbRest{get { return (plugin.Touches & ovrTouch.LThumbRest) > 0; }}

        public bool touchingLTrigger{get { return (plugin.Touches & ovrTouch.LTrigger) > 0; }}

        //gestures
        public bool LIndexPointing{get { return (plugin.Touches & ovrTouch.LIndexPointing) > 0; }}

        public bool LThumbUp{get { return (plugin.Touches & ovrTouch.LThumbUp) > 0; }}

        #endregion //Left Hand

        #region Right Hand
        public bool touchingA{get { return (plugin.Touches & ovrTouch.A) > 0; }}

        public bool touchingB{get { return (plugin.Touches & ovrTouch.B) > 0; }}

        public bool touchingRThumb{get { return (plugin.Touches & ovrTouch.RThumb) > 0; }}

        public bool touchingRThumbRest{get { return (plugin.Touches & ovrTouch.RThumbRest) > 0; }}

        public bool touchingRTrigger{get { return (plugin.Touches & ovrTouch.RTrigger) > 0; }}

        //gestures
        public bool RIndexPointing{get { return (plugin.Touches & ovrTouch.RIndexPointing) > 0; }}

        public bool RThumbUp{get { return (plugin.Touches & ovrTouch.RThumbUp) > 0; }}

        #endregion //Right Hand





        #endregion // Touches
        public void center()
        {
            plugin.Center();
        }
    }
}
