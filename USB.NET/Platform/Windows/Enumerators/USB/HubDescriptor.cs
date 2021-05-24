using System.Runtime.InteropServices;

namespace USB.NET.Platform.Windows.Enumerators.USB
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct HubDescriptor
    {
        public byte bDescriptorLength;
        public byte bDescriptorType;
        public byte bNumberOfPorts;
        public ushort wHubCharacteristics;
        public byte bPowerOnToPowerGood;
        public byte bHubControlCurrent;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public byte[] bRemoveAndPowerMask;
    }
}