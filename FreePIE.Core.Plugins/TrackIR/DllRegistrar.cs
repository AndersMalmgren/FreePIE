using System.IO;
using Microsoft.Win32;

namespace FreePIE.Core.Plugins.TrackIR
{
    public static class DllRegistrar
    {
        private const string KeyName = "Software\\NaturalPoint\\NATURALPOINT\\NPClient Location";
        private const string ValueName = "Path";
        private const string FreepieValueName = "Freepie_RealPath";

        public static void InjectFakeTrackIRDll(string dllPath)
        {
            var currentLocation = GetRealPath();

            if (currentLocation == dllPath)
                return;
            

            if (currentLocation != null)
                Registry.SetValue(GetNPClientKey(), FreepieValueName, currentLocation);

            Registry.SetValue(GetNPClientKey(), ValueName, dllPath);
        }

        public static string GetRealTrackIRDllPath(string freepieDllPath)
        {
            var realPath = GetRealPath();

            return realPath == freepieDllPath ? GetStoredRealPath() : realPath;
        }

        private static string GetRealPath()
        {
           return (string)Registry.GetValue(GetNPClientKey(), ValueName, null);
        }

        private static string GetStoredRealPath()
        {
            return (string)Registry.GetValue(GetNPClientKey(), FreepieValueName, null);
        }

        private static string GetNPClientKey()
        {
            return Path.Combine(Registry.CurrentUser.ToString(), KeyName);
        }

        public static void EjectFakeTrackIRDll()
        {
            var realLocation = GetStoredRealPath();

            if (realLocation != null)
                Registry.SetValue(GetNPClientKey(), ValueName, realLocation);
        }
    }
}
