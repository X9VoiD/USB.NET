using System;
using USB.NET.Descriptors;
using static Native.Windows.SetupAPI;

namespace USB.NET.Platform.Windows
{
    public sealed class UsbDevice : Device
    {
        internal UsbDevice(IntPtr deviceInfoSet, SP_DEVINFO_DATA deviceInfoData, string hubName, uint hubPort, DeviceDescriptor deviceDescriptor, Configuration config)
        {
            this.deviceInfoSet = deviceInfoSet;
            this.deviceInfoData = deviceInfoData;
            this.hubName = hubName;
            this.hubPort = hubPort;
            this.DeviceDescriptor = deviceDescriptor;
            this.Configuration = config;
        }

        internal DeviceDescriptor DeviceDescriptor { get; init; }
        internal Configuration Configuration { get; init; }
        private IntPtr deviceInfoSet;
        private SP_DEVINFO_DATA deviceInfoData;
        private string hubName;
        private uint hubPort;

        public override void ClearFeature(ushort feature)
        {
            throw new System.NotImplementedException();
        }

        public override Configuration GetConfiguration()
        {
            return this.Configuration;
        }

        public override DeviceDescriptor GetDeviceDescriptor()
        {
            return this.DeviceDescriptor;
        }

        public override string GetIndexedString(byte index)
        {
            throw new System.NotImplementedException();
        }

        public override bool SetConfiguration(ushort index)
        {
            throw new System.NotImplementedException();
        }

        public override void SetFeature(ushort feature)
        {
            throw new System.NotImplementedException();
        }
    }
}