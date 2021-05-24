namespace Native.Windows
{
    public static class IOCTL
    {
        private enum FILE
        {
            DEVICE_USB = 0x000022
        }

        private enum METHOD
        {
            BUFFERED,
            IN_DIRECT,
            OUT_DIRECT,
            NEITHER,
            DIRECT_TO_HARDWARE = IN_DIRECT,
            DIRECT_FROM_HARDWARE = OUT_DIRECT
        }

        private enum FILE_ACCESS
        {
            ANY,
            READ,
            WRITE,
            SPECIAL = ANY
        }

        public static uint USB_GET_NODE_INFORMATION = GetCtlCode(FILE.DEVICE_USB, 258, METHOD.BUFFERED, FILE_ACCESS.ANY);
        public static uint USB_GET_NODE_INFORMATION_EX = GetCtlCode(FILE.DEVICE_USB, 274, METHOD.BUFFERED, FILE_ACCESS.ANY);
        public static uint USB_GET_NODE_CONNECTION_NAME = GetCtlCode(FILE.DEVICE_USB, 261, METHOD.BUFFERED, FILE_ACCESS.ANY);
        public static uint USB_GET_NODE_CONNECTION_DRIVERKEY_NAME = GetCtlCode(FILE.DEVICE_USB, 264, METHOD.BUFFERED, FILE_ACCESS.ANY);
        public static uint USB_GET_DESCRIPTOR_FROM_NODE_CONNECTION = GetCtlCode(FILE.DEVICE_USB, 260, METHOD.BUFFERED, FILE_ACCESS.ANY);
        public static uint USB_GET_ROOT_HUB_NAME = GetCtlCode(FILE.DEVICE_USB, 258, METHOD.BUFFERED, FILE_ACCESS.ANY);
        public static uint USB_GET_PORT_CONNECTOR_PROPERTIES = GetCtlCode(FILE.DEVICE_USB, 278, METHOD.BUFFERED, FILE_ACCESS.ANY);

        private static uint GetCtlCode(FILE DeviceType, uint Function, METHOD Method, FILE_ACCESS Access)
        {
            return (uint)(((uint)DeviceType << 16) | ((uint)Access << 14) | (Function << 2) | (uint)Method);
        }
    }
}