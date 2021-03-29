using System;
using System.Runtime.InteropServices;

namespace Native.Windows
{
    public static partial class SetupAPI
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct SP_DEVICE_INTERFACE_DATA
        {
            public int cbSize;
            public Guid interfaceClassGuid;
            public int flags;
            private UIntPtr reserved;

            public static SP_DEVICE_INTERFACE_DATA AllocateNew()
            {
                return new SP_DEVICE_INTERFACE_DATA { cbSize = Marshal.SizeOf(typeof(SP_DEVICE_INTERFACE_DATA)) };
            }
        }
    }
}