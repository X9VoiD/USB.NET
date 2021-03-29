using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
using static Native.Windows.SetupAPI;
using static Native.Windows.Windows;

namespace USB.NET.Platform.Windows.Enumerators
{
    internal class UsbEnumerator : IDeviceEnumerator
    {
        private readonly List<UsbDevice> usbDevices = new List<UsbDevice>();
        private readonly List<DeviceInfoNode> deviceList = new List<DeviceInfoNode>();
        private readonly List<DeviceInfoNode> hubList = new List<DeviceInfoNode>();

        public IEnumerable<Device> GetDevices()
        {
            EnumerateHostControllers();
            throw new NotImplementedException();
        }

        private void EnumerateHostControllers()
        {
            EnumerateAllDevices();

            var usbHostControllerGuid = GUID_DEVINTERFACE_USB_HOST_CONTROLLER;
            var deviceInfoSet = SetupDiGetClassDevs(ref usbHostControllerGuid, IntPtr.Zero, IntPtr.Zero, DIGCF.Present | DIGCF.DeviceInterface);

            var deviceInfoData = SP_DEVINFO_DATA.AllocateNew();

            for (uint i = 0; SetupDiEnumDeviceInfo(deviceInfoSet, i, ref deviceInfoData); i++)
            {
                var deviceInterfaceData = SP_DEVICE_INTERFACE_DATA.AllocateNew();
                var deviceInterfaceDetailData = SP_DEVICE_INTERFACE_DETAIL_DATA.AllocateNew();

                if (!SetupDiEnumDeviceInterfaces(deviceInfoSet, IntPtr.Zero, ref usbHostControllerGuid, i, ref deviceInterfaceData))
                    throw new UsbEnumeratorException("Failed to get device interface");

                var size = (uint)Marshal.SizeOf<SP_DEVICE_INTERFACE_DETAIL_DATA>();
                if (!SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref deviceInterfaceData, ref deviceInterfaceDetailData, size, ref size, IntPtr.Zero))
                    throw new UsbEnumeratorException("Failed to get device data");

                try
                {
                    using var hostController = File.Open(deviceInterfaceDetailData.DevicePath, FileMode.Open, FileAccess.Write, FileShare.Write);
                    var hostControllerHandle = hostController.SafeFileHandle;
                    EnumerateHostController(hostControllerHandle, deviceInterfaceDetailData.DevicePath, deviceInfoSet, deviceInfoData);
                }
                catch (UnauthorizedAccessException)
                {
                    // Ignore
                }
            }
        }

        private void EnumerateAllDevices()
        {
            deviceList.AddRange(EnumerateAllDevicesWithGuid(GUID_DEVINTERFACE_USB_DEVICE));
            hubList.AddRange(EnumerateAllDevicesWithGuid(GUID_DEVINTERFACE_USB_HUB));
        }

        private void EnumerateHostController(SafeFileHandle hostControllerHandle, string devicePath, IntPtr deviceInfoSet, SP_DEVINFO_DATA deviceInfodata)
        {
            
        }

        private IEnumerable<DeviceInfoNode> EnumerateAllDevicesWithGuid(Guid guid)
        {
            var deviceInfoSet = SetupDiGetClassDevs(ref guid, IntPtr.Zero, IntPtr.Zero, DIGCF.Present | DIGCF.DeviceInterface);

            if (deviceInfoSet == InvalidHandle)
                throw new UsbEnumeratorException($"Failed to retrieve device info set for {guid}");

            for (uint i = 0; ; i++)
            {
                var deviceInfoNode = new DeviceInfoNode()
                {
                    DeviceInfoSet = deviceInfoSet,
                    DeviceInfoData = SP_DEVINFO_DATA.AllocateNew(),
                    DeviceInterfaceData = SP_DEVICE_INTERFACE_DATA.AllocateNew(),
                    DeviceDetailData = SP_DEVICE_INTERFACE_DETAIL_DATA.AllocateNew()
                };

                SetupDiEnumDeviceInfo(deviceInfoSet, i, ref deviceInfoNode.DeviceInfoData);
                if (Marshal.GetLastWin32Error() == ERROR_NO_MORE_ITEMS)
                    break;

                if (!GetDeviceProperty(deviceInfoNode.DeviceInfoSet, deviceInfoNode.DeviceInfoData, SPDRP.DEVICEDESC, out deviceInfoNode.DeviceDescName))
                    throw new UsbEnumeratorException($"Failed to get device property: {SPDRP.DEVICEDESC}");

                if (!GetDeviceProperty(deviceInfoNode.DeviceInfoSet, deviceInfoNode.DeviceInfoData, SPDRP.DRIVER, out deviceInfoNode.DeviceDriverName))
                    throw new UsbEnumeratorException($"Failed to get device property: {SPDRP.DRIVER}");

                var size = (uint)Marshal.SizeOf<SP_DEVICE_INTERFACE_DETAIL_DATA>();
                if (!SetupDiGetDeviceInterfaceDetail(deviceInfoNode.DeviceInfoSet, ref deviceInfoNode.DeviceInterfaceData, ref deviceInfoNode.DeviceDetailData, size, ref size, IntPtr.Zero))
                    throw new UsbEnumeratorException($"Failed to get device data");

                yield return deviceInfoNode;
            }
        }

        private bool GetDeviceProperty(IntPtr deviceInfoSet, SP_DEVINFO_DATA deviceInfoData, SPDRP property, out string value)
        {
            SetupDiGetDeviceRegistryProperty(deviceInfoSet, ref deviceInfoData, property, out _, IntPtr.Zero, 0, out var size);

            var text = new StringBuilder((int)size);
            var ret = SetupDiGetDeviceRegistryProperty(deviceInfoSet, ref deviceInfoData, property, out _, text, size, out _);

            value = text.ToString();
            return ret;
        }

        public void Dispose()
        {
        }

        public class UsbEnumeratorException : Exception
        {
            public UsbEnumeratorException(string msg)
                : base(msg)
            {
            }
        }

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
}