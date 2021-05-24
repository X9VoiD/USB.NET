using System;
using System.Runtime.InteropServices;
using USB.NET.Descriptors;

namespace USB.NET.Platform.Windows.Enumerators.USB
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NodeConnectionInformationEx
    {
        public uint ConnectionIndex;
        public DeviceDescriptor DeviceDescriptor;
        public byte CurrentConfigurationValue;
        public byte Speed;
        public bool DeviceIsHub;
        public ushort DeviceAddress;
        public uint NumberOfOpenPipes;
        public ConnectionStatus ConnectionStatus;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
        public USBPipeInfo[] PipeList;
    }
}