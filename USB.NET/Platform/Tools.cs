using USB.NET.Descriptors;

namespace USB.NET.Platform
{
    internal static class Tools
    {
        public static ushort string_index(byte index)
        {
            return (ushort)(((byte)DescriptorType.String << 8) | index);
        }

        public static ushort configuration_index(byte index)
        {
            return (ushort)(((byte)DescriptorType.Configuration << 8) | index);
        }
    }
}