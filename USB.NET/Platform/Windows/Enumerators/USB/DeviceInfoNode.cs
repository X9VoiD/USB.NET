using System;
using static Native.Windows.SetupAPI;

namespace USB.NET.Platform.Windows.Enumerators.USB
{
    public class DeviceInfoNode
    {
        public IntPtr DeviceInfoSet;
        public SP_DEVINFO_DATA DeviceInfoData;
        public SP_DEVICE_INTERFACE_DATA DeviceInterfaceData;
        public SP_DEVICE_INTERFACE_DETAIL_DATA DeviceDetailData;
        public string DeviceDescName;
        public string DeviceDriverName;
    }
}