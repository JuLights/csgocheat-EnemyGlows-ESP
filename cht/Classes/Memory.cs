using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;


namespace cht.Classes
{
    public class Memory
    {
        private static int read = 0;

        public static T ReadMemory<T>(int processHandle, int address) where T : struct
        {
            var size = Marshal.SizeOf(typeof(T));
            var buffer = new byte[size];
            Win32API.ReadProcessMemory(processHandle, address, buffer, buffer.Length, ref read);
            return ByteArrayToStructure<T>(buffer);
        }

        public static void WriteMemory<T>(int processHandle, int address, object value)
        {
            var buffer = StructureToByteArray(value);
            Win32API.WriteProcessMemory(processHandle, address, buffer, buffer.Length, out int write);
        }

        public static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }
        }

        public static byte[] StructureToByteArray(object obj)
        {
            var length = Marshal.SizeOf(obj);
            var buffer = new byte[length];
            var ptr = Marshal.AllocHGlobal(length);
            Marshal.StructureToPtr(obj, ptr, true);
            Marshal.Copy(ptr, buffer, 0, length);
            Marshal.FreeHGlobal(ptr);
            return buffer;
        }
    }
}
