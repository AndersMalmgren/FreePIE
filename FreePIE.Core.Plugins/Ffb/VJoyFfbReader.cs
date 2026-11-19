using System;
using System.Collections.Concurrent;
using vJoyInterfaceWrap;

namespace FreePIE.Core.Plugins.Ffb
{
    // vJoy has a single force feedback callback per process for all devices, it can't be unregistered
    // so it is registered once and packets are routed to the source of the device they belong to
    public static class VJoyFfbReader
    {
        private const uint Success = 0;

        private static readonly ConcurrentDictionary<uint, FfbSource> sources = new ConcurrentDictionary<uint, FfbSource>();
        private static readonly object registerLock = new object();
        private static vJoy joystick;
        private static vJoy.FfbCbFunc callback;

        public static void Register(uint deviceId, FfbSource source)
        {
            lock (registerLock)
            {
                if (callback == null)
                {
                    joystick = new vJoy();
                    callback = OnPacket;
                    joystick.FfbRegisterGenCB(callback, null);
                }
            }

            sources[deviceId] = source;
        }

        public static void Unregister(uint deviceId)
        {
            FfbSource source;
            sources.TryRemove(deviceId, out source);
        }

        private static void OnPacket(IntPtr data, object userData)
        {
            var deviceId = 0;
            if (joystick.Ffb_h_DeviceID(data, ref deviceId) != Success)
                return;

            FfbSource source;
            if (!sources.TryGetValue((uint) deviceId, out source))
                return;

            // An exception thrown here would unwind into the vJoy driver thread and take down the process
            try
            {
                Read(data, source);
            }
            catch (Exception e)
            {
                source.ReportError(e);
            }
        }

        private static void Read(IntPtr data, FfbSource source)
        {
            var type = default(FFBPType);
            if (joystick.Ffb_h_Type(data, ref type) != Success)
                return;

            switch (type)
            {
                case FFBPType.PT_EFFREP:
                    var report = new vJoy.FFB_EFF_REPORT();
                    if (joystick.Ffb_h_Eff_Report(data, ref report) == Success)
                        source.UpdateEffect(report.EffectBlockIndex, e => VJoyFfbConverter.Apply(e, report));
                    break;
                case FFBPType.PT_CONSTREP:
                    var constant = new vJoy.FFB_EFF_CONSTANT();
                    if (joystick.Ffb_h_Eff_Constant(data, ref constant) == Success)
                        source.UpdateEffect(constant.EffectBlockIndex, e => VJoyFfbConverter.Apply(e, constant));
                    break;
                case FFBPType.PT_RAMPREP:
                    var ramp = new vJoy.FFB_EFF_RAMP();
                    if (joystick.Ffb_h_Eff_Ramp(data, ref ramp) == Success)
                        source.UpdateEffect(ramp.EffectBlockIndex, e => VJoyFfbConverter.Apply(e, ramp));
                    break;
                case FFBPType.PT_PRIDREP:
                    var periodic = new vJoy.FFB_EFF_PERIOD();
                    if (joystick.Ffb_h_Eff_Period(data, ref periodic) == Success)
                        source.UpdateEffect(periodic.EffectBlockIndex, e => VJoyFfbConverter.Apply(e, periodic));
                    break;
                case FFBPType.PT_ENVREP:
                    var envelope = new vJoy.FFB_EFF_ENVLP();
                    if (joystick.Ffb_h_Eff_Envlp(data, ref envelope) == Success)
                        source.UpdateEffect(envelope.EffectBlockIndex, e => VJoyFfbConverter.Apply(e, envelope));
                    break;
                case FFBPType.PT_CONDREP:
                    var condition = new vJoy.FFB_EFF_COND();
                    if (joystick.Ffb_h_Eff_Cond(data, ref condition) == Success)
                        source.UpdateEffect(condition.EffectBlockIndex, e => VJoyFfbConverter.Apply(e, condition));
                    break;
                case FFBPType.PT_EFOPREP:
                    var operation = new vJoy.FFB_EFF_OP();
                    if (joystick.Ffb_h_EffOp(data, ref operation) != Success)
                        break;
                    if (operation.EffectOp == FFBOP.EFF_STOP)
                        source.StopEffect(operation.EffectBlockIndex);
                    else
                        source.StartEffect(operation.EffectBlockIndex, VJoyFfbConverter.ToLoopCount(operation.LoopCount), operation.EffectOp == FFBOP.EFF_SOLO);
                    break;
                case FFBPType.PT_BLKFRREP:
                    var blockIndex = 0;
                    if (joystick.Ffb_h_EBI(data, ref blockIndex) == Success)
                        source.RemoveEffect(blockIndex);
                    break;
                case FFBPType.PT_CTRLREP:
                    var control = default(FFB_CTRL);
                    if (joystick.Ffb_h_DevCtrl(data, ref control) == Success)
                        source.Control(VJoyFfbConverter.ToControl(control));
                    break;
                case FFBPType.PT_GAINREP:
                    byte gain = 0;
                    if (joystick.Ffb_h_DevGain(data, ref gain) == Success)
                        source.SetGain(VJoyFfbConverter.ToGain(gain));
                    break;
            }
        }
    }
}
