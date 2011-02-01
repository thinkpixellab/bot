using System;
using System.Diagnostics;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif

namespace PixelLab.Common
{
    public static class DebugTrace
    {
        public static void WriteLine(string message)
        {
            message = message ?? string.Empty;

            if (Debugger.IsLogging())
            {
                Debugger.Log(0, null, message + Environment.NewLine);
            }
        }

        public static void WriteLine(object value)
        {
            var message = value == null ? string.Empty : value.ToString();
            WriteLine(message);
        }

        public static void WriteLine(string format, params object[] args)
        {
            Contract.Requires(format != null);
            Contract.Requires(args != null);
            WriteLine(format.DoFormat(args));
        }
    }
}
