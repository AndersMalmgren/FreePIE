using System.Runtime.InteropServices;

namespace FreePIE.Core.Plugins.TrackIR
{
    internal class WritableTrackIRDll : TrackIRDll
    {
        private delegate int SetupFakeNPClient(string logPath);
        private delegate int SetHeadposePosition(float yaw, float pitch, float roll, float tx, float ty, float tz);
        private delegate NPClientSpoof.InternalHeadPoseData DecodeTrackirData(NPClientSpoof.InternalHeadPoseData data);

        private const string SetPositionName = "Freepie_SetPosition";
        private const string GetSignatureName = "NP_GetSignature";
        private const string DecodeTrackirName = "Freepie_DecodeTrackirData";
        private const string SetupNPClientName = "Freepie_Setup";

        private readonly DecodeTrackirData decodeHeadPose;
        private readonly SetupFakeNPClient setup;
        private readonly SetHeadposePosition setPosition;

        public WritableTrackIRDll(string dllPath) : base(dllPath)
        {
            setPosition = dll.GetDelegateFromFunction<SetHeadposePosition>(SetPositionName);
            decodeHeadPose = dll.GetDelegateFromFunction<DecodeTrackirData>(DecodeTrackirName);
            setup = dll.GetDelegateFromFunction<SetupFakeNPClient>(SetupNPClientName);    
        }

        public int SetupFakeNpClient(string logPath)
        {
            return setup(logPath);
        }

        public int SetPosition(float yaw, float pitch, float roll, float x, float y, float z)
        {
            return setPosition(yaw, pitch, roll, x, y, z);
        }

        public NPClientSpoof.InternalHeadPoseData DecodeTrackir(NPClientSpoof.InternalHeadPoseData data)
        {
            return decodeHeadPose(data);
        }
    }
}