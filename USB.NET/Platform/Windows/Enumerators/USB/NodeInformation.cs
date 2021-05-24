using System.Runtime.InteropServices;

namespace USB.NET.Platform.Windows.Enumerators.USB
{
    [StructLayout(LayoutKind.Explicit)]
    public struct NodeInformation
    {
        [FieldOffset(0)]
        public HubNode NodeType;

        [FieldOffset(4)]
        public HubInformation HubInformation;

        [FieldOffset(4)]
        public USBMiParentInformation MiParentInformation;
    }
}