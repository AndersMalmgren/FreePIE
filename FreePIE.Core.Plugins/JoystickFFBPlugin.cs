using SlimDX.DirectInput;
using System;
using System.Collections.Generic;
using vJoyFFBWrapper;
using EffectType = vJoyFFBWrapper.EffectType;

namespace FreePIE.Core.Plugins
{
    public partial class Device
    {
        private const int BlockSize = 8;
        private Effect[] Effects = new Effect[BlockSize];
        private EffectParameters[] EffectParams = new EffectParameters[BlockSize];
        private int[] Axes;

        public string Name { get { return joystick.Properties.ProductName; } }
        public Guid InstanceGuid { get { return joystick.Information.InstanceGuid; } }
        public bool SupportsFFB { get { return joystick.Capabilities.Flags.HasFlag(DeviceFlags.ForceFeedback); } }

        public int Gain
        {
            get { return joystick.Properties.ForceFeedbackGain; }
            set
            {
                CheckFFB("Can't set gain");
                joystick.Properties.ForceFeedbackGain = value;
            }
        }

        private void CheckFFB(string message)
        {
            if (!SupportsFFB)
                throw new Exception(message + " - this device does not support FFB");
        }

        private void PrepareFFB()
        {
            List<int> ax = new List<int>();
            foreach (DeviceObjectInstance deviceObject in joystick.GetObjects())
            {
                if ((deviceObject.ObjectType & ObjectDeviceType.ForceFeedbackActuator) != 0)
                {
                    ax.Add((int)deviceObject.ObjectType);
                }
            }
            Axes = ax.ToArray();
        }

        public void CreateEffect(int blockIndex, EffectType effectType, int duration, int[] dirs)
        {
            CheckFFB("Can't create effect");

            EffectParams[blockIndex] = new EffectParameters()
            {
                Duration = duration,
                Flags = EffectFlags.Cartesian | EffectFlags.ObjectIds,
                Gain = 10000,
                SamplePeriod = 0,
                StartDelay = 0,
                TriggerButton = -1,
                TriggerRepeatInterval = 0,
                Envelope = null
            };
            EffectParams[blockIndex].SetAxes(Axes, dirs);
            switch (effectType)
            {
                case EffectType.ConstantForce:
                    EffectParams[blockIndex].Parameters = new ConstantForce();
                    break;
            }

            try
            {
                Effects[blockIndex] = new Effect(joystick, EffectTypeGuidMap(effectType), EffectParams[blockIndex]);
            } catch (Exception e)
            {
                throw new Exception("Unable to create effect: " + e.Message);
            }
        }

        public void CreateEffect(EffectReport effectReport)
        {
            CheckFFB("Can't create effect");
            EffectParams[effectReport.BlockIndex] = new EffectParameters()
            {
                Duration = effectReport.Duration,
                Flags = EffectFlags.ObjectIds,
                Gain = 10000,
                SamplePeriod = 0,
                StartDelay = 0,
                TriggerButton = -1,
                TriggerRepeatInterval = 0,
                Envelope = null
            };
            if (effectReport.Polar)
            {
                EffectParams[effectReport.BlockIndex].Flags |= EffectFlags.Polar;
                EffectParams[effectReport.BlockIndex].SetAxes(Axes, new int[] { effectReport.AngleInDegrees, 0 });
            } else
            {
                EffectParams[effectReport.BlockIndex].Flags |= EffectFlags.Cartesian;
                EffectParams[effectReport.BlockIndex].SetAxes(Axes, new int[] { effectReport.DirectionX, effectReport.DirectionY });
            }

            switch (effectReport.EffectType)
            {
                case EffectType.ConstantForce:
                    EffectParams[effectReport.BlockIndex].Parameters = new ConstantForce();
                    break;
            }

            try
            {
                Effects[effectReport.BlockIndex] = new Effect(joystick, EffectTypeGuidMap(effectReport.EffectType), EffectParams[effectReport.BlockIndex]);
            } catch (Exception e)
            {
                throw new Exception("Unable to create effect: " + e.Message);
            }
        }

        public void SetConstantForce(int blockIndex, int magnitude)
        {
            CheckFFB("Can't set constant force");


            if (Effects[blockIndex] == null)
            {
                return;
                //throw new Exception("No effect has been created in block " + blockIndex);
            }
            if (EffectParams[blockIndex].Parameters == null)
                EffectParams[blockIndex].Parameters = new ConstantForce();
            EffectParams[blockIndex].Parameters.AsConstantForce().Magnitude = magnitude;
            Effects[blockIndex].SetParameters(EffectParams[blockIndex], EffectParameterFlags.TypeSpecificParameters);
        }



        public void OperateEffect(int blockIndex, EffectOperation effectOperation, int loopCount = 0)
        {
            CheckFFB("Can't operate effect");

            if (Effects[blockIndex] != null)
            {
                switch (effectOperation)
                {
                    case EffectOperation.Start:
                        Effects[blockIndex].Start(loopCount);
                        break;
                    case EffectOperation.SoloStart:
                        Effects[blockIndex].Start(loopCount, EffectPlayFlags.Solo);
                        break;
                    case EffectOperation.Stop:
                        Effects[blockIndex].Stop();
                        break;
                }
            }/* else throw new Exception("No effect has been created in block " + blockIndex);*/
        }

        private void DisposeEffects()
        {
            Console.WriteLine("Joystick {0} has {1} active effects. Disposing...", Name, joystick.CreatedEffects.Count);
            if (SupportsFFB)
                foreach (Effect e in joystick.CreatedEffects)
                    if (e != null && !e.Disposed)
                        e.Dispose();

        }

        public void DisposeEffect(int blockIndex)
        {
            if (Effects[blockIndex] != null && Effects[blockIndex].Status == EffectStatus.Playing)
            {
                //OperateEffect(blockIndex, EffectOperation.Stop, 0);
                Effects[blockIndex].Dispose();
            }
        }

        private Guid EffectTypeGuidMap(EffectType et)
        {
            switch (et)
            {
                case EffectType.ConstantForce:
                    return EffectGuid.ConstantForce;
                case EffectType.Ramp:
                    return EffectGuid.RampForce;
                case EffectType.Square:
                    return EffectGuid.Square;
                case EffectType.Sine:
                    return EffectGuid.Sine;
                case EffectType.Triangle:
                    return EffectGuid.Triangle;
                case EffectType.SawtoothUp:
                    return EffectGuid.SawtoothUp;
                case EffectType.SawtoothDown:
                    return EffectGuid.SawtoothDown;
                case EffectType.Spring:
                    return EffectGuid.Spring;
                case EffectType.Damper:
                    return EffectGuid.Damper;
                case EffectType.Inertia:
                    return EffectGuid.Inertia;
                case EffectType.Friction:
                    return EffectGuid.Friction;
                case EffectType.CustomForce:
                    return EffectGuid.CustomForce;
                case EffectType.None:
                default:
                    return new Guid();
            }
        }
    }
}
