using USB.NET.Platform.Windows.Exceptions;

namespace USB.NET.Platform.Windows.Enumerators
{
    public class UsbEnumeratorException : WindowsNativeException
    {
        public UsbEnumeratorException(string msg, bool getError = true)
            : base(msg, getError)
        {
        }
    }
}