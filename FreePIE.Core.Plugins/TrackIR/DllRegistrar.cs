using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

            TrackIRPlugin.Log("Injecting fake dll located at: " + dllPath);

            if (currentLocation == dllPath)
            {
                TrackIRPlugin.Log("Freepie dll was already registered as the real dll");
                return;
            }

            if (currentLocation != null)
            {
                TrackIRPlugin.Log("Storing path to old dll located at: " + currentLocation);
                Registry.SetValue(GetNPClientKey(), FreepieValueName, currentLocation);
            }

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

            TrackIRPlugin.Log("Ejecting fake dll");

            if (realLocation != null)
            {
                TrackIRPlugin.Log("Restoring to: " + realLocation);
                Registry.SetValue(GetNPClientKey(), ValueName, realLocation);
            } else TrackIRPlugin.Log("Nothing to restore to, doing nothing");
        }
    }
}
