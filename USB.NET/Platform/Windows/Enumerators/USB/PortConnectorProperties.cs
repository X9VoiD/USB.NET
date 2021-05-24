using System.Runtime.InteropServices;

namespace USB.NET.Platform.Windows.Enumerators.USB
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PortConnectorProperties
    {
        public uint ConnectionIndex;        // Starts on 1
        public uint ActualLength;
        public uint UsbPortProperties;
        public ushort CompanionIndex;       // Starts on 0
        public ushort CompanionPortNumber;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string CompanionHubSymbolicLinkName;
    }
}