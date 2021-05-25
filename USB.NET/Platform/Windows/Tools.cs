using System;
using System.Runtime.InteropServices;
using static Native.Windows.Kernel32;

namespace USB.NET.Platform.Windows
{
    public static class Tools
    {
        public static bool IOControl<T>(IntPtr handle, uint ioctl, out T data, out uint bytesReturned) where T : struct
        {
            var structSize = (uint)Marshal.SizeOf<T>();
            var structPtr = Marshal.AllocHGlobal((int)structSize);

            if (!DeviceIoControl(handle, ioctl, structPtr, structSize, structPtr, structSize, out bytesReturned, IntPtr.Zero))
            {
                data = default;
                return false;
            }

            data = Marshal.PtrToStructure<T>(structPtr);
            Marshal.FreeHGlobal(structPtr);

            return true;
        }

        public static bool IOControl<T>(IntPtr handle, uint ioctl, T input, out T data, out uint bytesReturned) where T : struct
        {
            var structSize = (uint)Marshal.SizeOf<T>();
            var structPtr = Marshal.AllocHGlobal((int)structSize);
            Marshal.StructureToPtr(input, structPtr, true);

            try
            {
                if (!DeviceIoControl(handle, ioctl, structPtr, structSize, structPtr, structSize, out bytesReturned, IntPtr.Zero))
                {
                    data = default;
                    return false;
                }

                data = Marshal.PtrToStructure<T>(structPtr);
                return true;
            }
            finally
            {
                Marshal.FreeHGlobal(structPtr);
            }
        }
    }
}