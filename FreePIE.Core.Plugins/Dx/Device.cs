using System;
using System.Collections.Generic;
using FreePIE.Core.Plugins.Strategies;
using FreePIE.Core.Plugins.VJoy;
using SlimDX.DirectInput;
using EffectType = FreePIE.Core.Plugins.VJoy.EffectType;
using System.Linq;

namespace FreePIE.Core.Plugins.Dx
{
    public class Device : IDisposable
    {
        private const int BlockSize = 8;
        private Effect[] Effects = new Effect[BlockSize];
        private readonly EffectParameters[] effectParams = new EffectParameters[BlockSize];
        private int[] Axes;

        public string Name { get { return joystick.Properties.ProductName; } }
        public Guid InstanceGuid { get { return joystick.Information.InstanceGuid; } }
        public bool SupportsFfb { get { return joystick.Capabilities.Flags.HasFlag(DeviceFlags.ForceFeedback); } }


        private readonly Joystick joystick;
        private JoystickState state;
        private readonly GetPressedStrategy<int> getPressedStrategy;

        public Device(Joystick joystick)
        {
            this.joystick = joystick;
            SetRange(-1000, 1000);
            getPressedStrategy = new GetPressedStrategy<int>(GetDown);

            if (SupportsFfb)
                PrepareFFB();
        }

        public void Dispose()
        {
            DisposeEffects();
            //Console.WriteLine("Disposing joystick: " + Name);
            joystick.Dispose();
            //Console.WriteLine("Finished disposing joystick");
        }

        public JoystickState State
        {
            get { return state ?? (state = joystick.GetCurrentState()); }
        }

        public void Reset()
        {
            state = null;
        }

        public void SetRange(int lowerRange, int upperRange)
        {
            foreach (DeviceObjectInstance deviceObject in joystick.GetObjects())
            {
                if ((deviceObject.ObjectType & ObjectDeviceType.Axis) != 0)
                    joystick.GetObjectPropertiesById((int)deviceObject.ObjectType).SetRange(lowerRange, upperRange);
            }
        }

        public bool GetPressed(int button)
        {
            return getPressedStrategy.IsPressed(button);
        }

        public bool GetDown(int button)
        {
            return State.IsPressed(button);
        }

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
            if (!SupportsFfb)
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

        /// <summary>
        /// Creates an empty effect WITHOUT EffectParameters
        /// </summary>
        public void CreateNewEffect(int blockIndex, EffectType effectType)
        {
            CheckFFB("Can't create effect");
            try
            {
                Effects[blockIndex] = new Effect(joystick, EffectTypeGuidMap(effectType));
            } catch (Exception e)
            {
                throw new Exception("Unable to create effect: " + e.Message);
            }
        }

        /// <summary>
        /// Creates a simple effect and instantly initializes it with EffectParameters
        /// </summary>
        public void CreateEffect(int blockIndex, EffectType effectType, int duration, int[] dirs)
        {
            CheckFFB("Can't create effect");

            effectParams[blockIndex] = new EffectParameters()
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
            effectParams[blockIndex].SetAxes(Axes, dirs);
            switch (effectType)
            {
                case EffectType.ConstantForce:
                    effectParams[blockIndex].Parameters = new ConstantForce();
                    break;
            }

            try
            {
                Effects[blockIndex] = new Effect(joystick, EffectTypeGuidMap(effectType), effectParams[blockIndex]);
            } catch (Exception e)
            {
                throw new Exception("Unable to create effect: " + e.Message);
            }
        }

        public void SetEffectParams(EffectReport effectReport)
        {
            CheckFFB("Can't create effect");
            effectParams[effectReport.BlockIndex] = new EffectParameters()
            {
                Duration = effectReport.Duration,
                Flags = EffectFlags.ObjectIds,
                Gain = effectReport.Gain * 39,
                SamplePeriod = effectReport.SamplePeriod,
                StartDelay = 0,//TODO use data from effectReport
                TriggerButton = -1,
                TriggerRepeatInterval = 0,
                Envelope = null
            };
            if (effectReport.Polar)
            {
                effectParams[effectReport.BlockIndex].Flags |= EffectFlags.Polar;
                //angle is in 100th degrees, so if you want to express 90 degrees (vector pointing to the right) you'll have to enter 9000
                effectParams[effectReport.BlockIndex].SetAxes(Axes, new int[] { effectReport.AngleInDegrees * 100, 0 });
            } else
            {
                effectParams[effectReport.BlockIndex].Flags |= EffectFlags.Cartesian;
                effectParams[effectReport.BlockIndex].SetAxes(Axes, new int[] { effectReport.DirectionX, effectReport.DirectionY });
            }

            switch (effectReport.EffectType)
            {
                case EffectType.ConstantForce:
                    effectParams[effectReport.BlockIndex].Parameters = new ConstantForce();
                    break;
            }

            try
            {
                if (Effects[effectReport.BlockIndex] != null && !Effects[effectReport.BlockIndex].Disposed)
                    Effects[effectReport.BlockIndex].SetParameters(effectParams[effectReport.BlockIndex], EffectParameterFlags.All);
                else
                    CreateNewEffect(effectReport.BlockIndex, effectReport.EffectType);
                //throw new Exception("No effect has been created in block " + effectReport.BlockIndex);
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
                //return;
                throw new Exception("No effect has been created in block " + blockIndex);
            }
            if (effectParams[blockIndex].Parameters == null)
                effectParams[blockIndex].Parameters = new ConstantForce();
            effectParams[blockIndex].Parameters.AsConstantForce().Magnitude = magnitude;
            Effects[blockIndex].SetParameters(effectParams[blockIndex], EffectParameterFlags.TypeSpecificParameters);
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
            } else throw new Exception("No effect has been created in block " + blockIndex);
        }

        private void DisposeEffects()
        {
            Console.WriteLine("Joystick {0} has {1} active effects. Disposing...", Name, joystick.CreatedEffects.Count);
            if (SupportsFfb)
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

        private Guid GetEffectGuid(EffectType et)
        {
            Guid effectGuid = EffectTypeGuidMap(et);
            joystick.GetEffects().Single(effectInfo => effectInfo.Guid == effectGuid);
            return effectGuid;
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
