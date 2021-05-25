using System.Runtime.InteropServices;

namespace USB.NET.Platform.Windows.Enumerators.USB
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct USBNodeName
    {
        public uint ConnectionIndex;
        public uint ActualLength;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string NodeName;
    }
}