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
        private const string keyName = "Software\\NaturalPoint\\NATURALPOINT\\NPClient Location";
        private const string valueName = "Path";
        private const string freepieValueName = "Freepie_RealPath";

        public static string InjectFakeTrackIRDll(string dllPath)
        {
            var currentLocation = (string)Registry.GetValue(GetNPClientKey(), valueName, null);

            if (currentLocation == dllPath)
                return null;

            if (currentLocation != null)
                Registry.SetValue(GetNPClientKey(), freepieValueName, currentLocation);

            Registry.SetValue(GetNPClientKey(), valueName, dllPath);

            return currentLocation;
        }

        private static string GetNPClientKey()
        {
            return Path.Combine(Registry.CurrentUser.ToString(), keyName);
        }

        public static void EjectFakeTrackIRDll()
        {
            var realLocation = (string)Registry.GetValue(GetNPClientKey(), freepieValueName, null);

            if (realLocation != null)
                Registry.SetValue(GetNPClientKey(), valueName, realLocation);
        }
    }
}
