using System;
using USB.NET.Descriptors;

namespace USB.NET.Platform.Windows.Enumerators.USB
{
    public class USBConfiguration : Configuration
    {
        public USBConfiguration(ConfigurationDescriptor descriptor, IntPtr data)
        {
            this.descriptor = descriptor;
        }

        private ConfigurationDescriptor descriptor;

        public override ConfigurationDescriptor GetConfigurationDescriptor() => descriptor;

        public override Interface GetInterface()
        {
            throw new System.NotImplementedException();
        }

        public override bool SetInterface(ushort index)
        {
            throw new System.NotImplementedException();
        }
    }
}