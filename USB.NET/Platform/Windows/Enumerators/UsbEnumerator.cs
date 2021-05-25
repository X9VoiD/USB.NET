using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Native.Windows;
using USB.NET.Descriptors;
using USB.NET.Platform.Windows.Enumerators.USB;
using static Native.Windows.Kernel32;
using static Native.Windows.SetupAPI;
using static Native.Windows.Windows;
using static USB.NET.Platform.Windows.Tools;

namespace USB.NET.Platform.Windows.Enumerators
{
    internal partial class UsbEnumerator : IDeviceEnumerator
    {
        private readonly List<UsbDevice> usbDevices = new List<UsbDevice>();
        private readonly List<DeviceInfoNode> deviceList = new List<DeviceInfoNode>();
        private readonly List<DeviceInfoNode> hubList = new List<DeviceInfoNode>();

        private IntPtr DeviceInfoSet;

        public IEnumerable<Device> GetDevices()
        {
            EnumerateHostControllers();
            return usbDevices;
        }

        private void EnumerateHostControllers()
        {
            EnumerateAllDevices();

            var enumeratedHostController = new List<string>();

            var usbHostControllerGuid = GUID_DEVINTERFACE_USB_HOST_CONTROLLER;
            DeviceInfoSet = SetupDiGetClassDevs(ref usbHostControllerGuid, IntPtr.Zero, IntPtr.Zero, DIGCF.Present | DIGCF.DeviceInterface);

            var deviceInfoData = SP_DEVINFO_DATA.AllocateNew();

            for (uint i = 0; SetupDiEnumDeviceInfo(DeviceInfoSet, i, ref deviceInfoData); i++)
            {
                var deviceInterfaceData = SP_DEVICE_INTERFACE_DATA.AllocateNew();
                var deviceInterfaceDetailData = SP_DEVICE_INTERFACE_DETAIL_DATA.AllocateNew();

                if (!SetupDiEnumDeviceInterfaces(DeviceInfoSet, IntPtr.Zero, ref usbHostControllerGuid, i, ref deviceInterfaceData))
                    throw new UsbEnumeratorException("Failed to get device interface");

                var size = (uint)Marshal.SizeOf<SP_DEVICE_INTERFACE_DETAIL_DATA>();
                if (!SetupDiGetDeviceInterfaceDetail(DeviceInfoSet, ref deviceInterfaceData, ref deviceInterfaceDetailData, size, ref size, IntPtr.Zero))
                    throw new UsbEnumeratorException("Failed to get device data");

                var devicePath = deviceInterfaceDetailData.DevicePath;
                if (enumeratedHostController.Contains(devicePath))
                    continue;

                enumeratedHostController.Add(devicePath);

                using var hostController = File.Open(devicePath, FileMode.Open, FileAccess.Write, FileShare.Write);
                var hostControllerHandle = hostController.SafeFileHandle;
                if (!hostControllerHandle.IsInvalid)
                    EnumerateHostController(hostControllerHandle.DangerousGetHandle());
            }
        }

        private void EnumerateAllDevices()
        {
            deviceList.AddRange(EnumerateAllDevicesWithGuid(GUID_DEVINTERFACE_USB_DEVICE));
            hubList.AddRange(EnumerateAllDevicesWithGuid(GUID_DEVINTERFACE_USB_HUB));
        }

        private static IEnumerable<DeviceInfoNode> EnumerateAllDevicesWithGuid(Guid guid)
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

                if (!SetupDiEnumDeviceInterfaces(deviceInfoNode.DeviceInfoSet, IntPtr.Zero, ref guid, i, ref deviceInfoNode.DeviceInterfaceData))
                    throw new UsbEnumeratorException($"Failed to retrieve device interface data");

                var size = (uint)Marshal.SizeOf<SP_DEVICE_INTERFACE_DETAIL_DATA>();
                if (!SetupDiGetDeviceInterfaceDetail(deviceInfoNode.DeviceInfoSet, ref deviceInfoNode.DeviceInterfaceData, ref deviceInfoNode.DeviceDetailData, size, ref size, IntPtr.Zero))
                    throw new UsbEnumeratorException($"Failed to get device data");

                yield return deviceInfoNode;
            }
        }

        private void EnumerateHostController(IntPtr hostControllerHandle)
        {
            var hubName = GetRootHubName(hostControllerHandle, IOCTL.USB_GET_ROOT_HUB_NAME);
            EnumerateHub(hubName);
        }

        private void EnumerateHub(string hubName)
        {
            var hubHandle = CreateFile(hubName, FileAccess.Write, FileShare.Write, IntPtr.Zero, FileMode.Open, (FileAttributes)0, IntPtr.Zero);
            if (hubHandle == InvalidHandle)
                throw new UsbEnumeratorException("Failed to open hub", false);

            if (!IOControl<NodeInformation>(hubHandle, IOCTL.USB_GET_NODE_INFORMATION, out var hubInfo, out _))
                throw new UsbEnumeratorException("Failed to retrieve USB hub information");

            if (hubInfo.NodeType != HubNode.UsbHub)
                throw new UsbEnumeratorException("DeviceIoControl returned wrong information type for hub");

            EnumerateHubPorts(hubName, hubHandle, hubInfo.HubInformation.HubDescriptor.bNumberOfPorts);
        }

        private void EnumerateHubPorts(string hubName, IntPtr hubNativeHandle, byte numberOfPorts)
        {
            for (uint i = 1; i <= numberOfPorts; i++)
            {
                var nodeConnectionInformationEx = new NodeConnectionInformationEx
                {
                    ConnectionIndex = i
                };

                if (!IOControl(hubNativeHandle, IOCTL.USB_GET_NODE_INFORMATION_EX, nodeConnectionInformationEx, out nodeConnectionInformationEx, out var size))
                    continue;

                if (nodeConnectionInformationEx.DeviceIsHub)
                {
                    var externalHubName = GetExternalHubName(hubNativeHandle, i);
                    if (externalHubName != null)
                    {
                        EnumerateHub(@"\\.\" + externalHubName);
                        continue;
                    }
                }
                else if (nodeConnectionInformationEx.ConnectionStatus == ConnectionStatus.NoDeviceConnected)
                    continue;

                var driverKeyName = GetDriverKeyName(hubNativeHandle, i);
                (var deviceInfoSet, var deviceInfoData, var path) = GetDeviceProperties(driverKeyName);

                var configuration = GetConfigurationDescriptor(hubNativeHandle, i, 0);

                usbDevices.Add(new UsbDevice(path, deviceInfoSet, deviceInfoData, hubName, i, nodeConnectionInformationEx.DeviceDescriptor, configuration));
            }

            CloseHandle(hubNativeHandle);
        }

        private static Configuration GetConfigurationDescriptor(IntPtr hubNativeHandle, uint port, ushort descriptorIndex)
        {
            var descriptorRequest = new DescriptorRequest
            {
                ConnectionIndex = port,
                wValue = (ushort)(((ushort)DescriptorType.Configuration << 8) | descriptorIndex),
                wLength = (ushort)Marshal.SizeOf<ConfigurationDescriptor>()
            };

            var descriptorRequestPtrA = IntPtr.Zero;
            var descriptorRequestPtrB = IntPtr.Zero;

            try
            {
                var size = Marshal.SizeOf<DescriptorRequest>() + Marshal.SizeOf<ConfigurationDescriptor>();
                descriptorRequestPtrA = Marshal.AllocHGlobal(size);
                descriptorRequestPtrB = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(descriptorRequest, descriptorRequestPtrA, true);

                if (!DeviceIoControl(hubNativeHandle, IOCTL_USB_GET_DESCRIPTOR_FROM_NODE_CONNECTION,
                                     descriptorRequestPtrA, (uint)size, descriptorRequestPtrB, (uint)size,
                                     out var outSize, IntPtr.Zero))
                    throw new UsbEnumeratorException("Failed to retrieve configuration descriptor");

                var configuration = Marshal.PtrToStructure<ConfigurationDescriptor>(descriptorRequestPtrB);
                return new USBConfiguration(configuration, IntPtr.Add(descriptorRequestPtrB, Marshal.SizeOf<ConfigurationDescriptor>()));
            }
            catch
            {
                throw new UsbEnumeratorException("Unknown failure");
            }
            finally
            {
                Marshal.FreeHGlobal(descriptorRequestPtrA);
                Marshal.FreeHGlobal(descriptorRequestPtrB);
            }
        }

        private (IntPtr, SP_DEVINFO_DATA, string) GetDeviceProperties(string driverKeyName)
        {
            var device = deviceList.Where(d => d.DeviceDriverName == driverKeyName).ToArray();
            if (!device.Any())
                return (IntPtr.Zero, default, null);

            if (device.Length > 1)
                throw new UsbEnumeratorException("Encountered unexpected device duplicate");

            return (device[0].DeviceInfoSet, device[0].DeviceInfoData, device[0].DeviceDetailData.DevicePath);
        }

        private static bool GetDeviceProperty(IntPtr deviceInfoSet, SP_DEVINFO_DATA deviceInfoData, SPDRP property, out string value)
        {
            SetupDiGetDeviceRegistryProperty(deviceInfoSet, ref deviceInfoData, property, out _, IntPtr.Zero, 0, out var size);

            var text = new StringBuilder((int)size);
            var ret = SetupDiGetDeviceRegistryProperty(deviceInfoSet, ref deviceInfoData, property, out _, text, size, out _);

            value = text.ToString();
            return ret;
        }

        private static string GetRootHubName(IntPtr hostControllerHandle, uint ioctl)
        {
            if (!IOControl<USBName>(hostControllerHandle, ioctl, out var nameStruct, out _))
                throw new UsbEnumeratorException("Failed to retrieve root hub name");

            return @"\\.\" + nameStruct.Name;
        }

        private static string GetExternalHubName(IntPtr hubHandle, uint connectionIndex)
        {
            var nodeName = new USBNodeName
            {
                ConnectionIndex = connectionIndex
            };

            if (!IOControl(hubHandle, IOCTL.USB_GET_NODE_CONNECTION_NAME, nodeName, out nodeName, out _))
                throw new UsbEnumeratorException("Failed to get external hub name");

            return string.IsNullOrEmpty(nodeName.NodeName) ? null : nodeName.NodeName;
        }

        private static string GetDriverKeyName(IntPtr hubHandle, uint connectionIndex)
        {
            var nodeName = new USBNodeName
            {
                ConnectionIndex = connectionIndex
            };

            if (!IOControl(hubHandle, IOCTL.USB_GET_NODE_CONNECTION_DRIVERKEY_NAME, nodeName, out nodeName, out _))
                throw new UsbEnumeratorException("Failed to get driver key name");

            return string.IsNullOrEmpty(nodeName.NodeName) ? null : nodeName.NodeName;
        }

        public void Dispose()
        {
            SetupDiDestroyDeviceInfoList(DeviceInfoSet);
        }
    }
}