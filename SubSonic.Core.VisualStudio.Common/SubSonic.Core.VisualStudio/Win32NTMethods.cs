namespace SubSonic.Core.VisualStudio
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;

    public static class Win32NTMethods
    {
        public const int E_INSUFFICIENT_BUFFER = -2147024774;
        public const uint VSITEMID_ROOT = 0xfffffffe;
        public const int SERVERCALL_ISHANDLED = 0;
        public const int SERVERCALL_RETRYLATER = 2;
        public const int PENDINGMSG_WAITDEFPROCESS = 2;

        //[DllImport("Ole32.dll")]
        //internal static extern int CoRegisterMessageFilter(IOleMessageFilter newFilter, out IOleMessageFilter oldFilter);
        [DllImport(@"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\fusion.dll", CharSet = CharSet.Auto)]
        public static extern int CreateAssemblyCache(out IAssemblyCache ppAsmCache, uint dwReserved);
        //[DllImport(@"C:\Windows\microsoft.net\framework\Ole32.dll")]
        //internal static extern void CreateBindCtx(int reserved, out IBindCtx ppbc);
        public static bool Failed(int hr)
        {
            return (hr < 0);
        }

        //[DllImport(@"C:\Windows\microsoft.net\framework\Ole32.dll")]
        //internal static extern void GetRunningObjectTable(int reserved, out IRunningObjectTable prot);
    }
}