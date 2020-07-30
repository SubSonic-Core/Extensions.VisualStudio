using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SubSonic.Core.VisualStudio
{
    [ComImport, Guid("E707DCDE-D1CD-11D2-BAB9-00C04F8ECEAE"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAssemblyCache
    {
        int UninstallAssembly();
        [PreserveSig]
        int QueryAssemblyInfo(uint dwFlags, [MarshalAs(UnmanagedType.LPWStr)] string pszAssemblyName, ref ASSEMBLY_INFO pAsmInfo);
        int CreateAssemblyCacheItem();
        int CreateAssemblyScavenger();
        int InstallAssembly();
    }
}
