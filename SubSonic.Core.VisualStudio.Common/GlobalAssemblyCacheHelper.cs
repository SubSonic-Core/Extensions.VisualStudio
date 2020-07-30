using Microsoft.TeamFoundation.Common.Internal;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace SubSonic.Core.VisualStudio.Common
{
    public static class GlobalAssemblyCacheHelper
    {
        public static readonly Regex strongNameRegEx = new Regex("(?<name>[A-z|.]*), Version=(?<version>[0-9|.]*), Culture=(?<culture>[A-z]*), PublicKeyToken=(?<token>[A-z|0-9]*)", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);

        public static bool IsStrongName(this string name)
        {
            return strongNameRegEx.IsMatch(name);
        }

        private enum AssemblyCacheEnum : uint
        {
            QUERYASMINFO_FLAG_VALIDATE = 0x00000001,
            QUERYASMINFO_FLAG_GETSIZE = 0x00000002
        }

        public static string GetLocation(string strongName)
        {
            string location = strongName ?? throw new ArgumentNullException(nameof(strongName));

            if (Environment.OSVersion.Platform == PlatformID.Win32NT && IsStrongName(strongName))
            {
                IAssemblyCache cache;

                AssemblyName name = null;
                try
                {
                    name = new AssemblyName(strongName);
                }
                catch(FileLoadException)
                {
                    return location;
                }

                if (!Win32NTMethods.Failed(Win32NTMethods.CreateAssemblyCache(out cache, 0)) && (cache != null))
                {
                    try
                    {
                        if (name.ProcessorArchitecture != ProcessorArchitecture.None)
                        {
                            location = GetLocationImpl(cache, strongName, null);
                        }
                        else
                        {
                            string targetProcessorArchitecture = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
                            if (!string.IsNullOrEmpty(targetProcessorArchitecture))
                            {
                                location = GetLocationImpl(cache, strongName, targetProcessorArchitecture);
                            }
                            if (string.IsNullOrEmpty(location))
                            {   // let's try the MSIL Architecture
                                location = GetLocationImpl(cache, strongName, "MSIL");

                                if ((location == null) || (location.Length <= 0))
                                {
                                    location = GetLocationImpl(cache, strongName, null);
                                }
                            }
                        }
                    }
                    finally
                    {
                        Marshal.FinalReleaseComObject(cache);
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(location) && string.Equals(Path.GetExtension(strongName), ".dll", StringComparison.OrdinalIgnoreCase))
                    {
                        location = GetLocation(Path.ChangeExtension(strongName, null));
                    }
                }
            }
            else if (Environment.OSVersion.Platform == PlatformID.Unix && IsStrongName(strongName))
            {
                throw new PlatformNotSupportedException();
            }

            return location;
        }

        private static string GetLocationImpl(IAssemblyCache assemblyCache, string strongName, string targetProcessorArchitecture)
        {
            ASSEMBLY_INFO pAsmInfo = new ASSEMBLY_INFO
            {
                cbAssemblyInfo = (uint)Marshal.SizeOf(typeof(ASSEMBLY_INFO))
            };

            if (targetProcessorArchitecture != null)
            {
                strongName = strongName + ", ProcessorArchitecture=" + targetProcessorArchitecture;
            }
            int hr = assemblyCache.QueryAssemblyInfo(3, strongName, ref pAsmInfo);
            if ((Win32NTMethods.Failed(hr) && (hr != Win32NTMethods.E_INSUFFICIENT_BUFFER)) || (pAsmInfo.cbAssemblyInfo == 0))
            {
                return string.Empty;
            }
            pAsmInfo.pszCurrentAssemblyPathBuf = new string(new char[pAsmInfo.cchBuf]);
            return (!Win32NTMethods.Failed(assemblyCache.QueryAssemblyInfo(3, strongName, ref pAsmInfo)) ? pAsmInfo.pszCurrentAssemblyPathBuf : string.Empty);
        }
    }
}
