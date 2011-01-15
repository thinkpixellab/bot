using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PixelLab.Contracts
{
    public static class Contract
    {
        public static void Requires(bool truth, string message = null)
        {
        }

        public static void Requires<TException>(bool truth, string message = null) where TException : Exception { }

        public static void Assume(bool truth, string message = null)
        {
            Debug.Assert(truth, message);
        }

        public static void Ensures(bool truth)
        {
        }

        public static void Invariant(bool truth)
        {
            throw new NotSupportedException();
        }

        public static T Result<T>()
        {
            throw new NotSupportedException();
        }

        public static bool ForAll<T>(IEnumerable<T> source, Func<T, bool> func)
        {
            throw new NotSupportedException();
        }
    }

    public class PureAttribute : Attribute
    {

    }

    public class ContractClassAttribute : Attribute
    {
        public ContractClassAttribute(Type contractType) { }
    }
    public class ContractClassForAttribute : Attribute
    {
        public ContractClassForAttribute(Type contractForType) { }
    }
    public class ContractInvariantMethodAttribute : Attribute { }
}
