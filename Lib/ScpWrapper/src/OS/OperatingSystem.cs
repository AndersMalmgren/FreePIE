using System;
using System.Management;
using System.Text.RegularExpressions;

namespace JonesCorp.OS
{
    public class OperatingSystem
    {
        public static string OSInfo()
        {
            string info = string.Empty;

            try
            {
                using (var mos = new ManagementObjectSearcher("SELECT * FROM  Win32_OperatingSystem"))
                {
                    foreach (ManagementObject mo in mos.Get())
                    {
                        try
                        {
                            info = Regex.Replace(mo.GetPropertyValue("Caption").ToString(), "[^A-Za-z0-9 ]", "").Trim();

                            try
                            {
                                object spv = mo.GetPropertyValue("ServicePackMajorVersion");

                                if (spv != null && spv.ToString() != "0")
                                {
                                    info += " Service Pack " + spv.ToString();
                                }
                            }
                            catch { }

                            info =
                                $"{info} ({System.Environment.OSVersion.Version.ToString()} {System.Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE")})";

                        }
                        catch { }

                        mo.Dispose();
                    }
                }
            }
            catch { }

            return info;
        }

        public static OSType OSParse(string Info)
        {
            OSType Valid = OSType.INVALID;

            try
            {
                string Architecture = System.Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE").ToUpper().Trim();

                if (Environment.Is64BitOperatingSystem == Environment.Is64BitProcess && (Architecture == "X86" || Architecture == "AMD64"))
                {
                    Valid = OSType.DEFAULT;

                    if (!string.IsNullOrEmpty(Info))
                    {
                        string[] Token = Info.Split(new char[] { ' ' });

                        if (Token[0].ToUpper().Trim() == "MICROSOFT" && Token[1].ToUpper().Trim() == "WINDOWS")
                        {
                            switch (Token[2].ToUpper().Trim())
                            {
                                case "XP":

                                    if (!System.Environment.Is64BitOperatingSystem) Valid = OSType.XP;
                                    break;

                                case "VISTA":

                                    Valid = OSType.VISTA;
                                    break;

                                case "7":

                                    Valid = OSType.WIN7;
                                    break;

                                case "8":

                                    Valid = OSType.WIN8;
                                    break;

                                case "81":

                                    Valid = OSType.WIN81;
                                    break;

                                case "SERVER":

                                    switch (Token[3].ToUpper().Trim())
                                    {
                                        case "2008":

                                            if (Token[4].ToUpper().Trim() == "R2")
                                            {
                                                Valid = OSType.WIN7;
                                            }
                                            else
                                            {
                                                Valid = OSType.VISTA;
                                            }
                                            break;

                                        case "2012":

                                            Valid = OSType.WIN8;
                                            break;
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
            catch { }

            return Valid;
        }
    }
}
