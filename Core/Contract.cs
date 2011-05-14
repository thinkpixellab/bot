using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PixelLab.Common;

namespace PixelLab.Contracts
{
    public static class Contract
    {
        [DebuggerStepThrough]
        public static void Requires(bool truth, string message = null)
        {
            Util.ThrowUnless(truth, message);
        }

        [DebuggerStepThrough]
        public static void Requires<TException>(bool truth, string message) where TException : Exception
        {
            Util.ThrowUnless<TException>(truth, message);
        }

        [DebuggerStepThrough]
        public static void Requires<TException>(bool truth) where TException : Exception, new()
        {
            Util.ThrowUnless<TException>(truth);
        }

        public static void Assume(bool truth, string message = null)
        {
            Debug.Assert(truth, message);
        }

        [Conditional("NEVER")]
        public static void Ensures(bool truth) { }

        [Conditional("NEVER")]
        public static void Invariant(bool truth, string message = null)
        {
            throw new NotSupportedException();
        }

        public static T Result<T>()
        {
            throw new NotSupportedException();
        }

        public static bool ForAll<T>(IEnumerable<T> source, Func<T, bool> predicate)
        {
            return source.All(predicate);
        }
    }

    public class PureAttribute : Attribute { }

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
