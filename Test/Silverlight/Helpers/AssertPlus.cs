using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif

namespace PixelLab.Test.Helpers
{
    // Question: Kevin - 2011-05-22: why throw AssertFailedException instead of Assert.Fail?
    public static class AssertPlus
    {
        public static TException ExceptionThrown<TException>(Action testAction) where TException : Exception
        {
            Contract.Requires(testAction != null);
            try
            {
                testAction();
                throw new AssertFailedException("No exception was thrown");
            }
            catch (TException ex)
            {
                return ex;
            }
            catch (Exception ex)
            {
                throw new AssertFailedException("The wrong exception was thrown", ex);
            }
        }
    }
}
