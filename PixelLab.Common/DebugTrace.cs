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
        public static void Write(string message)
        {
            message = message ?? string.Empty;

            if (Debugger.IsLogging())
            {
                Debugger.Log(0, null, message);
            }
        }

        public static void Write(object value)
        {
            var message = value == null ? string.Empty : value.ToString();
            Write(message);
        }

        public static void Write(string format, params object[] args)
        {
            Contract.Requires(format != null);
            Contract.Requires(args != null);
            Write(format.DoFormat(args));
        }
    }
}
