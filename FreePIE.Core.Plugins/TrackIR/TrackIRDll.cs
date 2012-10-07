using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

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
        private byte errorCount;

        protected NativeDll dll;
        private readonly _GetSignature getSignature;
        private readonly GetHeadposePosition getPosition;
        private readonly _QueryVersion queryVersion;
        private readonly _RegisterWindowHandle registerWindowHandle;
        private readonly _UnregisterWindowHandle unregisterWindowHandle;
        private readonly _RegisterProgramProfileId registerProgramProfileId;
        private readonly _RequestData requestData;
        private readonly _StartDataTransmission startDataTransmission;
        private readonly _StopDataTransmission stopDataTransmission;
        private readonly _StartCursor startCursor;
        private readonly _StopCursor stopCursor;
        private readonly _ReCenter reCenter;

        public TrackIRDll(string path)
        {
            TrackIRPlugin.Log("Loading dll at " + path);
            dll = new NativeDll(path);
            getSignature = dll.GetDelegateFromFunction<_GetSignature>(GetSignatureName);
            getPosition = dll.GetDelegateFromFunction<GetHeadposePosition>(GetDataName);
            queryVersion = dll.GetDelegateFromFunction<_QueryVersion>(QueryVersionName);
            registerWindowHandle = dll.GetDelegateFromFunction<_RegisterWindowHandle>(RegisterWindowHandleName);
            unregisterWindowHandle = dll.GetDelegateFromFunction<_UnregisterWindowHandle>(UnregisterWindowHandleName);
            registerProgramProfileId = dll.GetDelegateFromFunction<_RegisterProgramProfileId>(RegisterProgramProfileIdName);
            requestData = dll.GetDelegateFromFunction<_RequestData>(RequestDataName);
            startDataTransmission = dll.GetDelegateFromFunction<_StartDataTransmission>(StartDataTransmissionName);
            stopDataTransmission = dll.GetDelegateFromFunction<_StopDataTransmission>(StopDataTransmissionName);
            startCursor = dll.GetDelegateFromFunction<_StartCursor>(StartCursorName);
            stopCursor = dll.GetDelegateFromFunction<_StopCursor>(StopCursorName);
            reCenter = dll.GetDelegateFromFunction<_ReCenter>(ReCenterName);
        }

        private void ExecuteAndCheckReturnValue(string functionName, Func<int> function)
        {
            int retval = function();

            if (retval != 0)
            {
                IncreaseErrorCount();
                if(errorCount <= 3)
                    TrackIRPlugin.Log("Error: " + functionName + " resulted in error code " + retval);
            }
        }

        void IncreaseErrorCount()
        {
            unchecked
            {
                errorCount++;
            }
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

        private delegate int _GetSignature(IntPtr signature);
        private delegate int GetHeadposePosition(IntPtr data);
        private delegate int _QueryVersion(IntPtr version);
        private delegate int _RegisterWindowHandle(IntPtr handle);
        private delegate int _UnregisterWindowHandle();
        private delegate int _RegisterProgramProfileId(short id);
        private delegate int _RequestData(short data);
        private delegate int _StartDataTransmission();
        private delegate int _StopDataTransmission();
        private delegate int _StartCursor();
        private delegate int _StopCursor();
        private delegate int _ReCenter();

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
