using System.Runtime.InteropServices;
using USB.NET.Packets;

namespace USB.NET.Platform.Windows.Enumerators.USB
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DescriptorHeader
    {
        public RequestType bmRequestType;
        public Request bRequest;
    }
}