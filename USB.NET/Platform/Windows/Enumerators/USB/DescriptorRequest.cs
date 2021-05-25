using System.Runtime.InteropServices;

namespace USB.NET.Platform.Windows.Enumerators.USB
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DescriptorRequest
    {
        public uint ConnectionIndex;
        public DescriptorHeader Header;
        public ushort wValue;
        public ushort wIndex;
        public ushort wLength;
    }
}