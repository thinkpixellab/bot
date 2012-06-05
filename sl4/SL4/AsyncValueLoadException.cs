using System;

namespace PixelLab.SL
{
    public class AsyncValueLoadException : Exception
    {
        public AsyncValueLoadException(Exception innerException) : base("An exception was thrown while loading an async value.", innerException) { }
    }
}
