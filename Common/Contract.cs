using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PixelLab.Common;

namespace PixelLab.Contracts
{
    public static class Contract
    {
        public static void Requires(bool truth, string message = null)
        {
            Requires<Exception>(truth, message);
        }

        public static void Requires<TException>(bool truth, string message = null) where TException : Exception
        {
            if (!truth)
            {
                throw InstanceFactory.CreateInstance<TException>(message);
            }
        }

        public static void Requires<TException>(bool truth) where TException : Exception, new()
        {
            if (!truth)
            {
                throw new TException();
            }
        }

        public static void Assume(bool truth, string message = null)
        {
            Debug.Assert(truth, message);
        }

        [Conditional("NEVER")]
        public static void Ensures(bool truth) { }

        [Conditional("NEVER")]
        public static void Invariant(bool truth)
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
