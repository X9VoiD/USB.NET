using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace USB.NET.Platform.Windows.Exceptions
{
    public class WindowsNativeException : Exception
    {
        public WindowsNativeException()
            : base()
        {
        }

        public WindowsNativeException(string message, bool getError = true)
            : base(FormatMessage(message, getError))
        {
        }

        private static string FormatMessage(string message, bool getError)
        {
            if (getError)
            {
                var err = Marshal.GetLastWin32Error();
                return $"{message}: {new Win32Exception(err).Message} (Error {err})";
            }
            else
            {
                return message;
            }
        }
    }
}