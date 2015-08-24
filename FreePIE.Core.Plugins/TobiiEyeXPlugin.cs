using System;
using System.Collections.Generic;
using EyeXFramework;
using FreePIE.Core.Contracts;
using SlimDX;
using Tobii.EyeX.Client;
using Tobii.EyeX.Framework;

namespace FreePIE.Core.Plugins
{
    [GlobalType(Type = typeof(TobiiEyeXGlobal))]
    public class TobiiEyeXPlugin : Plugin
    {
        private EyeXHost host;
        private EyePositionDataStream eyeDataStream;
        private GazePointDataStream gazeDataStream;
        private Vector3 eyeOffsetMm;
        private Vector3 eyeOffsetNorm;
        private float targetRoll;
        private float targetYaw;
        private Vector3 targetAverageHeadPositionMm;
        private Vector3 targetAverageHeadPositionNorm;


        public float Yaw { get; private set; }
        public float Roll { get; private set; }
        // You can't calculate pitch using eye tracker at the moment.
        public Vector3 AverageHeadPositionMm { get; private set; }
        public Vector3 AverageHeadPositionNorm { get; private set; }

        public EyePositionEventArgs LastEyePositionEventArgs { get; private set; }
        public GazePointEventArgs LastGazePointEventArgs { get; private set; }

        public Vector2 NormalizedGazePoint { get; private set; }

        public Vector2 NormalizedCenterDelta { get; private set; }

        public UpdateReason UpdateReason { get; private set; }
        public UserPresence UserPresence { get; private set; }
        public EyeTrackingDeviceStatus DeviceStatus { get; private set; }
        public string CurrentProfileName { get; private set; }
        public Vector2 DisplaySize { get; private set; }
        public TrackStatus TrackStatus { get; private set; }


        public override object CreateGlobal()
        {
            return new TobiiEyeXGlobal(this);
        }

        public override string FriendlyName
        {
            get { return "Tobii EyeX"; }
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
            host = new EyeXHost();
            host.Start();

            host.UserPresenceChanged += HostOnUserPresenceChanged;
            host.EyeTrackingDeviceStatusChanged += HostOnEyeTrackingDeviceStatusChanged;
            host.UserProfileNameChanged += HostOnUserProfileNameChanged;
            host.DisplaySizeChanged += HostOnDisplaySizeChanged;

            gazeDataStream = host.CreateGazePointDataStream(GazePointDataMode.Unfiltered);
            eyeDataStream = host.CreateEyePositionDataStream();

            gazeDataStream.Next += GazeDataStreamOnNext;
            eyeDataStream.Next += EyeDataStreamOnNext;

            return null;
        }

        public override void Stop()
        {
            gazeDataStream.Next -= GazeDataStreamOnNext;
            eyeDataStream.Next -= EyeDataStreamOnNext;
            gazeDataStream.Dispose();
            eyeDataStream.Dispose();

            host.DisplaySizeChanged -= HostOnDisplaySizeChanged;
            host.UserProfileNameChanged -= HostOnUserProfileNameChanged;
            host.EyeTrackingDeviceStatusChanged -= HostOnEyeTrackingDeviceStatusChanged;
            host.UserPresenceChanged -= HostOnUserPresenceChanged;
            host.Dispose();
        }

        public override void DoBeforeNextExecute()
        {
        }


        private void HostOnDisplaySizeChanged(object sender, EngineStateValue<Size2> engineStateValue)
        {
            if(!engineStateValue.IsValid)
                return;

            DisplaySize = new Vector2((float)engineStateValue.Value.Width, (float)engineStateValue.Value.Height);
            UpdateReason = UpdateReason.DisplaySizeChanged;
            OnUpdate();
        }

        private void HostOnUserProfileNameChanged(object sender, EngineStateValue<string> engineStateValue)
        {
            if(!engineStateValue.IsValid)
                return;

            CurrentProfileName = engineStateValue.Value;
            UpdateReason = UpdateReason.CurrentProfileNameChanged;
            OnUpdate();
        }

        private void HostOnEyeTrackingDeviceStatusChanged(object sender, EngineStateValue<EyeTrackingDeviceStatus> engineStateValue)
        {
            if(!engineStateValue.IsValid)
                return;

            DeviceStatus = engineStateValue.Value;
            UpdateReason = UpdateReason.DeviceStatusChanged;
            OnUpdate();
        }

        private void HostOnUserPresenceChanged(object sender, EngineStateValue<UserPresence> engineStateValue)
        {
            if(!engineStateValue.IsValid)
                return;

            UserPresence = engineStateValue.Value;
            UpdateReason = UpdateReason.UserPresenceChanged;
            OnUpdate();
        }

        private void EyeDataStreamOnNext(object sender, EyePositionEventArgs eyePositionEventArgs)
        {
            LastEyePositionEventArgs = eyePositionEventArgs;
            UpdateReason = UpdateReason.EyeDataChanged;
            TrackStatus = TrackStatus.NoEyes;

            var rightEyePositionMm = new Vector3((float)eyePositionEventArgs.RightEye.X, (float)eyePositionEventArgs.RightEye.Y, (float)eyePositionEventArgs.RightEye.Z);
            var leftEyePositionMm = new Vector3((float)eyePositionEventArgs.LeftEye.X, (float)eyePositionEventArgs.LeftEye.Y, (float)eyePositionEventArgs.LeftEye.Z);
            var rightEyePositionNorm = new Vector3((float)eyePositionEventArgs.RightEyeNormalized.X, (float)eyePositionEventArgs.RightEyeNormalized.Y, (float)eyePositionEventArgs.RightEyeNormalized.Z);
            var leftEyePositionNorm = new Vector3((float)eyePositionEventArgs.LeftEyeNormalized.X, (float)eyePositionEventArgs.LeftEyeNormalized.Y, (float)eyePositionEventArgs.LeftEyeNormalized.Z);

            if(eyePositionEventArgs.LeftEye.IsValid && eyePositionEventArgs.RightEye.IsValid)
            {
                TrackStatus = TrackStatus.BothEyes;
                eyeOffsetMm = rightEyePositionMm - leftEyePositionMm;
                eyeOffsetNorm = rightEyePositionNorm - leftEyePositionNorm;
            }
            else if(eyePositionEventArgs.LeftEye.IsValid)
            {
                TrackStatus = TrackStatus.OnlyLeftEye;
            }
            else if(eyePositionEventArgs.RightEye.IsValid)
            {
                TrackStatus = TrackStatus.OnlyRightEye;
            }

            switch(TrackStatus)
            {
                case TrackStatus.BothEyes:
                    targetAverageHeadPositionMm = (rightEyePositionMm + leftEyePositionMm) / 2f;
                    targetAverageHeadPositionNorm = (rightEyePositionNorm + leftEyePositionNorm) / 2f;
                    break;

                case TrackStatus.OnlyLeftEye:
                    targetAverageHeadPositionMm = leftEyePositionMm + eyeOffsetMm / 2f;
                    targetAverageHeadPositionNorm = leftEyePositionNorm + eyeOffsetNorm / 2f;
                    break;

                case TrackStatus.OnlyRightEye:
                    targetAverageHeadPositionMm = rightEyePositionMm - eyeOffsetMm / 2f;
                    targetAverageHeadPositionNorm = rightEyePositionNorm - eyeOffsetNorm / 2f;
                    break;

                case TrackStatus.NoEyes:
                default:
                    //Don't update D:
                    break;
            }

            targetRoll = (float)Math.Atan2(eyeOffsetMm.Y, eyeOffsetMm.X);
            targetYaw = -(float)Math.Atan2(eyeOffsetMm.Z, eyeOffsetMm.X);

            Roll = Lerp(Roll, targetRoll, 0.6f);
            Yaw = Lerp(Yaw, targetYaw, 0.6f);

            AverageHeadPositionMm = Lerp(AverageHeadPositionMm, targetAverageHeadPositionMm, 0.6f);
            AverageHeadPositionNorm = Lerp(AverageHeadPositionNorm, targetAverageHeadPositionNorm, 0.6f);

            OnUpdate();
        }

        private void GazeDataStreamOnNext(object sender, GazePointEventArgs gazePointEventArgs)
        {
            LastGazePointEventArgs = gazePointEventArgs;

            const double screenExtensionFactor = 0;
            var screenExtensionX = host.ScreenBounds.Value.Width * screenExtensionFactor;
            var screenExtensionY = host.ScreenBounds.Value.Height * screenExtensionFactor;

            var gazePointX = gazePointEventArgs.X + screenExtensionX / 2;
            var gazePointY = gazePointEventArgs.Y + screenExtensionY / 2;

            var screenWidth = host.ScreenBounds.Value.Width + screenExtensionX;
            var screenHeight = host.ScreenBounds.Value.Height + screenExtensionY;


            var normalizedGazePointX = (float)Math.Min(Math.Max((gazePointX / screenWidth), 0.0), 1.0);
            var normalizedGazePointY = (float)Math.Min(Math.Max((gazePointY / screenHeight), 0.0), 1.0);

            NormalizedGazePoint = new Vector2(normalizedGazePointX, normalizedGazePointY);

            var normalizedDistanceFromCenterX = (normalizedGazePointX - 0.5f) * 2.0f;
            var normalizedDistanceFromCenterY = (normalizedGazePointY - 0.5f) * 2.0f;

            NormalizedCenterDelta = new Vector2(normalizedDistanceFromCenterX, normalizedDistanceFromCenterY);

            UpdateReason = UpdateReason.GazeDataChanged;
            OnUpdate();
        }

        private static float Lerp(float lower, float higher, float alpha)
        {
            return lower + (higher - lower) * alpha;
        }

        private static Vector3 Lerp(Vector3 lower, Vector3 higher, float alpha)
        {
            return lower + (higher - lower) * alpha;
        }
    }

    [Global(Name = "tobiiEyeX")]
    public class TobiiEyeXGlobal : UpdateblePluginGlobal<TobiiEyeXPlugin>
    {
        public TobiiEyeXGlobal(TobiiEyeXPlugin plugin) : base(plugin) { }

        public float yaw { get { return plugin.Yaw; } }
        public float roll { get { return plugin.Roll; } }

        public float normalizedCenterDeltaX { get { return plugin.NormalizedCenterDelta.X; } }
        public float normalizedCenterDeltaY { get { return plugin.NormalizedCenterDelta.Y; } }

        public float gazePointNormalizedX { get { return plugin.NormalizedGazePoint.X; } }
        public float gazePointNormalizedY { get { return plugin.NormalizedGazePoint.X; } }

        public float gazePointInPixelsX { get { return (float)plugin.LastGazePointEventArgs.X; } }
        public float gazePointInPixelsY { get { return (float)plugin.LastGazePointEventArgs.Y; } }
        public float gazeDataTimestamp { get { return (float)plugin.LastGazePointEventArgs.Timestamp; } }

        public float leftEyePositionInMmX { get { return (float)plugin.LastEyePositionEventArgs.LeftEye.X; } }
        public float leftEyePositionInMmY { get { return (float)plugin.LastEyePositionEventArgs.LeftEye.Y; } }
        public float leftEyePositionInMmZ { get { return (float)plugin.LastEyePositionEventArgs.LeftEye.Z; } }

        public float rightEyePositionInMmX { get { return (float)plugin.LastEyePositionEventArgs.RightEye.X; } }
        public float rightEyePositionInMmY { get { return (float)plugin.LastEyePositionEventArgs.RightEye.Y; } }
        public float rightEyePositionInMmZ { get { return (float)plugin.LastEyePositionEventArgs.RightEye.Z; } }

        public float leftEyePositionNormalizedX { get { return (float)plugin.LastEyePositionEventArgs.LeftEyeNormalized.X; } }
        public float leftEyePositionNormalizedY { get { return (float)plugin.LastEyePositionEventArgs.LeftEyeNormalized.Y; } }
        public float leftEyePositionNormalizedZ { get { return (float)plugin.LastEyePositionEventArgs.LeftEyeNormalized.Z; } }

        public float rightEyePositionNormalizedX { get { return (float)plugin.LastEyePositionEventArgs.RightEyeNormalized.X; } }
        public float rightEyePositionNormalizedY { get { return (float)plugin.LastEyePositionEventArgs.RightEyeNormalized.Y; } }
        public float rightEyePositionNormalizedZ { get { return (float)plugin.LastEyePositionEventArgs.RightEyeNormalized.Z; } }

        public float averageEyePositionInMmX { get { return plugin.AverageHeadPositionMm.X; } }
        public float averageEyePositionInMmY { get { return plugin.AverageHeadPositionMm.Y; } }
        public float averageEyePositionInMmZ { get { return plugin.AverageHeadPositionMm.Z; } }

        public float averageEyePositionNormalizedX { get { return plugin.AverageHeadPositionNorm.X; } }
        public float averageEyePositionNormalizedY { get { return plugin.AverageHeadPositionNorm.Y; } }
        public float averageEyePositionNormalizedZ { get { return plugin.AverageHeadPositionNorm.Z; } }

        public float eyeDataTimestamp { get { return (float)plugin.LastEyePositionEventArgs.Timestamp; } }

        public string updateReason { get { return plugin.UpdateReason.ToString(); } }
        public string userPresence { get { return plugin.UserPresence.ToString(); } }
        public string deviceStatus { get { return plugin.DeviceStatus.ToString(); } }
        public string userProfileName { get { return plugin.CurrentProfileName; } }
        public float displaySizeX { get { return plugin.DisplaySize.X; } }
        public float displaySizeY { get { return plugin.DisplaySize.Y; } }
    }

    public enum UpdateReason
    {
        GazeDataChanged,
        EyeDataChanged,
        UserPresenceChanged,
        DeviceStatusChanged,
        CurrentProfileNameChanged,
        DisplaySizeChanged
    }

    public enum TrackStatus
    {
        BothEyes,
        OnlyLeftEye,
        OnlyRightEye,
        NoEyes
    }
}
