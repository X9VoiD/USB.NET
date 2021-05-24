using System.Runtime.InteropServices;

namespace USB.NET.Platform.Windows.Enumerators.USB
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct HubInformation
    {
        public HubDescriptor HubDescriptor;
        public bool HubIsBusPowered;
    }
}