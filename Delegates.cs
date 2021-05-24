using System;
using System.Runtime.InteropServices;


namespace SharpRDPDump
{
    public class Delegates
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool MiniDumpWriteDump(IntPtr hProcess, uint processId, SafeHandle hFile, uint dumpType, IntPtr expParam, IntPtr userStreamParam, IntPtr callbackParam);

    }
}
