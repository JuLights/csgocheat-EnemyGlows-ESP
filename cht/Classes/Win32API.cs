using System;
using System.Runtime.InteropServices;


namespace cht.Classes
{
    public static class Win32API
    {
        public static readonly int PROCESS_VM_WRITE = 0x0020;
        public static readonly int PROCESS_VM_READ = 0x0010;
        public static readonly int PROCESS_VM_OPERATION = 0x0008;

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] buffer, int size, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(int hProcess, int lpBaseAddress, byte[] buffer, int size, out int lpNumberOfBytesWritten);
    }
}
