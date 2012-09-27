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

        private void ExecuteAndCheckReturnValue(Func<int> function)
        {
            int retval = function();

            if (retval != 0)
                throw new TrackIRException(retval);
        }

        public string GetSignature()
        {
            using (var signature = new MarshalledString(400))
            {
                ExecuteAndCheckReturnValue(() => getSignature(signature.Pointer));
                return signature.Value;
            }
        }

        public void GetPosition(IntPtr data)
        {
            ExecuteAndCheckReturnValue(() => getPosition(data));
        }

        public short QueryVersion()
        {
            using (var version = new MarshalledMemory<short>())
            {
                ExecuteAndCheckReturnValue(() => queryVersion(version.Pointer));
                return version.Value;
            }
        }

        public void RegisterWindowHandle(IntPtr handle)
        {
            ExecuteAndCheckReturnValue(() => registerWindowHandle(handle));
        }

        public void UnregisterWindowHandle()
        {
            ExecuteAndCheckReturnValue(() => unregisterWindowHandle());
        }

        public void RegisterProgramProfileId(short id)
        {
            ExecuteAndCheckReturnValue(() => registerProgramProfileId(id));
        }

        public void RequestData(short data)
        {
            ExecuteAndCheckReturnValue(() => requestData(data));
        }

        public void StartDataTransmission()
        {
            ExecuteAndCheckReturnValue(() => startDataTransmission());
        }

        public void StopDataTransmission()
        {
            ExecuteAndCheckReturnValue(() => stopDataTransmission());
        }

        public void StartCursor()
        {
            ExecuteAndCheckReturnValue(() => startCursor());
        }

        public void StopCursor()
        {
            ExecuteAndCheckReturnValue(() => stopCursor());
        }

        public void Recenter()
        {
            ExecuteAndCheckReturnValue(() => reCenter());
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
