using System.Runtime.InteropServices;
using USB.NET.Descriptors;

namespace USB.NET.Platform.Windows.Enumerators.USB
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct USBPipeInfo
    {
        public EndpointDescriptor EndpointDescriptor;
        public uint ScheduleOffset;
    }
}