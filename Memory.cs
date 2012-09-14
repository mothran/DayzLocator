using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Memory
{
    class MemoryTool
    {
        [DllImport("Kernel32.dll")]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, UInt32 nSize, ref UInt32 lpNumberOfBytesRead);
        private string pname;
        private IntPtr hand;

        public MemoryTool(String procName)
        {
            pname = procName;
            Process[] handles = Process.GetProcessesByName(pname);
            Process target;

            if (handles.Length == 0)
            {
                Console.WriteLine("Notepad process not found");
                Environment.Exit(0);
            }
            target = handles[0];
            Console.WriteLine(target.ToString());
            if (target.Handle == IntPtr.Zero)
            {
                Console.WriteLine("Could not connect to process");
                Environment.Exit(0);
            }
            hand = target.Handle;
        }

        public byte[] read(int Address, int length)
        {
            byte[] ret = new byte[length];
            uint o = 0;

            // Read an address and return a byte array
            if (ReadProcessMemory(hand, (IntPtr)Address, ret, (UInt32)ret.Length, ref o) == false)
            {
                Console.WriteLine("ReadProcessMemory error, dont know what though");
                throw new System.Exception("Memory derp");
            }
            return ret;
        }
    }
}