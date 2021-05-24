using System.Runtime.InteropServices;

namespace USB.NET.Platform.Windows.Enumerators.USB
{

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 1)]
    public struct USBName
    {
        public int ActualLength;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Name;
    }
}