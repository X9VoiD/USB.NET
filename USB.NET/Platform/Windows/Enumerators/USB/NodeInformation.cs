using System.Runtime.InteropServices;

namespace USB.NET.Platform.Windows.Enumerators.USB
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NodeInformation
    {
        public HubNode NodeType;
        public HubInformation HubInformation;
    }
}