using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using FreePIE.Core.Plugins.MemoryMapping;

namespace FreePIE.Core.Plugins.TrackIR
{
    internal class TrackIRDll : IDisposable
    {
        private const string GetSignatureName = "NP_GetSignature";
        private const string GetDataName = "NP_GetData";
        private const string QueryVersionName = "NP_QueryVersion";
        private const string RegisterWindowHandleName = "NP_RegisterWindowHandle";
        private const string UnregisterWindowHandleName = "NP_UnregisterWindowHandle";
        private const string RegisterProgramProfileIdName = "NP_RegisterProgramProfileID";
        private const string RequestDataName = "NP_RequestData";
        private const string StartDataTransmissionName = "NP_StartDataTransmission";
        private const string StopDataTransmissionName = "NP_StopDataTransmission";
        private const string StartCursorName = "NP_StartCursor";
        private const string StopCursorName = "NP_StopCursor";
        private const string ReCenterName = "NP_ReCenter";

        private readonly NativeDll dll;
        private readonly NpGetSignature getSignature;
        private readonly GetHeadposePosition getPosition;
        private readonly NpQueryVersion queryVersion;
        private readonly NpRegisterWindowHandle registerWindowHandle;
        private readonly NpUnregisterWindowHandle unregisterWindowHandle;
        private readonly NpRegisterProgramProfileId registerProgramProfileId;
        private readonly NpRequestData requestData;
        private readonly NpStartDataTransmission startDataTransmission;
        private readonly NpStopDataTransmission stopDataTransmission;
        private readonly NpStartCursor startCursor;
        private readonly NpStopCursor stopCursor;
        private readonly NpReCenter reCenter;
        private readonly Action<string> logger;

        public TrackIRDll(string path, Action<string> logger = null)
        {
            this.logger = logger ?? (str => {});

            dll = new NativeDll(path);
            getSignature = dll.GetDelegateFromFunction<NpGetSignature>(GetSignatureName);
            getPosition = dll.GetDelegateFromFunction<GetHeadposePosition>(GetDataName);
            queryVersion = dll.GetDelegateFromFunction<NpQueryVersion>(QueryVersionName);
            registerWindowHandle = dll.GetDelegateFromFunction<NpRegisterWindowHandle>(RegisterWindowHandleName);
            unregisterWindowHandle = dll.GetDelegateFromFunction<NpUnregisterWindowHandle>(UnregisterWindowHandleName);
            registerProgramProfileId = dll.GetDelegateFromFunction<NpRegisterProgramProfileId>(RegisterProgramProfileIdName);
            requestData = dll.GetDelegateFromFunction<NpRequestData>(RequestDataName);
            startDataTransmission = dll.GetDelegateFromFunction<NpStartDataTransmission>(StartDataTransmissionName);
            stopDataTransmission = dll.GetDelegateFromFunction<NpStopDataTransmission>(StopDataTransmissionName);
            startCursor = dll.GetDelegateFromFunction<NpStartCursor>(StartCursorName);
            stopCursor = dll.GetDelegateFromFunction<NpStopCursor>(StopCursorName);
            reCenter = dll.GetDelegateFromFunction<NpReCenter>(ReCenterName);
        }

        private void ExecuteAndCheckReturnValue(string functionName, Func<int> function)
        {
            int retval = function();

            if (retval != 0)
                logger("Function failed: " + functionName + " with error: " + retval);
        }

        public string GetSignature()
        {
            using (var signature = new MarshalledString(400))
            {
                ExecuteAndCheckReturnValue(GetSignatureName, () => getSignature(signature.Pointer));
                return signature.Value;
            }
        }

        public void GetPosition(IntPtr data)
        {
            ExecuteAndCheckReturnValue(GetDataName, () => getPosition(data));
        }

        public short QueryVersion()
        {
            using (var version = new MarshalledMemory<short>())
            {
                ExecuteAndCheckReturnValue(QueryVersionName, () => queryVersion(version.Pointer));
                return version.Value;
            }
        }

        public void RegisterWindowHandle(IntPtr handle)
        {
            ExecuteAndCheckReturnValue(RegisterWindowHandleName, () => registerWindowHandle(handle));
        }

        public void UnregisterWindowHandle()
        {
            ExecuteAndCheckReturnValue(UnregisterWindowHandleName, () => unregisterWindowHandle());
        }

        public void RegisterProgramProfileId(short id)
        {
            ExecuteAndCheckReturnValue(RegisterProgramProfileIdName, () => registerProgramProfileId(id));
        }

        public void RequestData(short data)
        {
            ExecuteAndCheckReturnValue(RequestDataName, () => requestData(data));
        }

        public void StartDataTransmission()
        {
            ExecuteAndCheckReturnValue(StartDataTransmissionName, () => startDataTransmission());
        }

        public void StopDataTransmission()
        {
            ExecuteAndCheckReturnValue(StopDataTransmissionName, () => stopDataTransmission());
        }

        public void StartCursor()
        {
            ExecuteAndCheckReturnValue(StartCursorName, () => startCursor());
        }

        public void StopCursor()
        {
            ExecuteAndCheckReturnValue(StopCursorName, () => stopCursor());
        }

        public void Recenter()
        {
            ExecuteAndCheckReturnValue(ReCenterName, () => reCenter());
        }

        private delegate int NpGetSignature(IntPtr signature);
        private delegate int GetHeadposePosition(IntPtr data);
        private delegate int NpQueryVersion(IntPtr version);
        private delegate int NpRegisterWindowHandle(IntPtr handle);
        private delegate int NpUnregisterWindowHandle();
        private delegate int NpRegisterProgramProfileId(short id);
        private delegate int NpRequestData(short data);
        private delegate int NpStartDataTransmission();
        private delegate int NpStopDataTransmission();
        private delegate int NpStartCursor();
        private delegate int NpStopCursor();
        private delegate int NpReCenter();

        public void Dispose()
        {
            dll.Dispose();
        }
    }

    public class TrackIRException : Exception
    {
        public TrackIRException(int errorCode) : base("Error occured while reading from TrackIR dll: " + errorCode)
        { }
    }
}
