using System;
using System.IO;
using System.Runtime.InteropServices;
using USB.NET.Descriptors;
using USB.NET.Platform.Windows.Enumerators.USB;
using USB.NET.Platform.Windows.Exceptions;
using static Native.Windows.Kernel32;
using static Native.Windows.SetupAPI;
using static Native.Windows.Windows;

namespace USB.NET.Platform.Windows
{
    public sealed class UsbDevice : Device
    {
        internal UsbDevice(string path, IntPtr deviceInfoSet, SP_DEVINFO_DATA deviceInfoData, string hubName, uint hubPort, DeviceDescriptor deviceDescriptor, Configuration config)
        {
            this.deviceInfoSet = deviceInfoSet;
            this.deviceInfoData = deviceInfoData;
            this.hubName = hubName;
            this.hubPort = hubPort;
            this.DeviceDescriptor = deviceDescriptor;
            this.VendorID = deviceDescriptor.idVendor;
            this.ProductID = deviceDescriptor.idProduct;
            if (deviceDescriptor.iManufacturer != 0)
                this.Manufacturer = GetIndexedString(deviceDescriptor.iManufacturer);
            if (deviceDescriptor.iProduct != 0)
                this.ProductName = GetIndexedString(deviceDescriptor.iProduct);
            if (deviceDescriptor.iSerialNumber != 0)
                this.SerialNumber = GetIndexedString(deviceDescriptor.iSerialNumber);
            this.Configuration = config;
            this.InternalFilePath = path;
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

        public unsafe override string GetIndexedString(byte index)
        {
            var descriptorRequest = new DescriptorRequest
            {
                ConnectionIndex = this.hubPort,
                wValue = (ushort)(((ushort)DescriptorType.String << 8) | index),
                wLength = (ushort)StringDescriptor.GetSize()
            };

            var hubHandle = CreateFile(hubName, FileAccess.Write, FileShare.Write, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
            if (hubHandle == InvalidHandle)
                throw new WindowsNativeException("Failed to open parent hub");

            var size = Marshal.SizeOf<DescriptorRequest>() + StringDescriptor.GetSize();
            var requestPtr = stackalloc byte[size];
            Marshal.StructureToPtr(descriptorRequest, (IntPtr)requestPtr, true);

            var ret = DeviceIoControl(hubHandle, IOCTL_USB_GET_DESCRIPTOR_FROM_NODE_CONNECTION,
                                (IntPtr)requestPtr, (uint)size, (IntPtr)requestPtr, (uint)size,
                                out _, IntPtr.Zero);

            string deviceString = null;

            try
            {
                if (ret)
                {
                    StringDescriptor* stringDescriptor = (StringDescriptor*)(IntPtr)(((DescriptorRequest*)requestPtr) + 1);
                    if (stringDescriptor->bDescriptorType != DescriptorType.String)
                        throw new IOException("Invalid descriptor received");

                    if (stringDescriptor->bLength == 0)
                    {
                        deviceString = "";
                    }
                    else
                    {
                        var actualLength = (stringDescriptor->bLength - 1) / 2;
                        deviceString = new Span<char>(&stringDescriptor->bString, actualLength).ToString();
                        if (deviceString.Split('\0')[0].Length != actualLength)
                            throw new StringDescriptor.MalformedStringDescriptor(this, index, new Span<byte>(&stringDescriptor->bString, stringDescriptor->bLength).ToArray());
                    }
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                CloseHandle(hubHandle);
            }

            return ret ? deviceString : null;
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