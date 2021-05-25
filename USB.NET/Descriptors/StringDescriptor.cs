using System;
using System.Runtime.InteropServices;

namespace USB.NET.Descriptors
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public unsafe struct StringDescriptor
    {
        /// <summary>
        /// Size of this descriptor in bytes
        /// </summary>
        public byte bLength;

        /// <summary>
        /// The current descriptor type
        /// </summary>
        public DescriptorType bDescriptorType;

        /// <summary>
        /// The string from a specified index
        /// </summary>
        public char bString;

        public static int GetSize() => 255 + 2;

        public class MalformedStringDescriptor : Exception
        {
            public MalformedStringDescriptor(Device device, int index, byte[] rawContent)
                : base($"Device: {device.ProductName} returned malformed string descriptor on index {index}")
            {
                RawContent = rawContent;
            }

            public byte[] RawContent;
        }
    }
}