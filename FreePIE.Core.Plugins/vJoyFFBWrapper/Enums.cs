using FreePIE.Core.Contracts;

//https://github.com/shauleiz/vJoy/blob/master/apps/common/vJoyInterfaceCS/vJoyInterfaceWrap/Wrapper.cs

namespace vJoyFFBWrapper
{
    [GlobalEnum]
    public enum PacketType : byte
    {
        // Write
        Effect = 0x01,
        Envelope = 0x02,
        Condition = 0x3,
        Periodic = 0x4,
        ConstantForce = 0x5,
        RampForce = 0x6,
        CustomForceData = 0x7,
        DownloadForceSample = 0x8,
        EffectOperation = 0xA,
        PIDBlockFree = 0xB,
        PIDDeviceControl = 0xC,
        DeviceGain = 0xD,
        SetCustomForce = 0xE,

        // Feature
        CreateNewEffect = 0x01 + 0x10,
        BlockLoad = 0x02 + 0x10,
        PIDPool = 0x03 + 0x10
    }

    [GlobalEnum]
    public enum EffectOperation : byte
    {
        Start = 1,
        SoloStart = 2,
        Stop = 3
    }

    [GlobalEnum]
    public enum EffectType : byte
    {
        None = 0,
        ConstantForce = 1,
        Ramp = 2,
        Square = 3,
        Sine = 4,
        Triangle = 5,
        SawtoothUp = 6,
        SawtoothDown = 7,
        Spring = 8,
        Damper = 9,
        Inertia = 10,
        Friction = 11,
        CustomForce = 12
    }

    [GlobalEnum]
    public enum PIDDeviceControl : byte
    {
        EnableActuators = 1,
        DisableActuators = 2,
        StopAll = 3,
        DeviceReset = 4,
        DevicePause = 5,
        DeviceContinue = 6
    }
}
