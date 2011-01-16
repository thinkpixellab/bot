#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif

namespace PixelLab.Common
{
    [ContractClass(typeof(IMapCommandTargetContract))]
    public interface IMapCommandTarget
    {
        CommandHandler Handler { get; }
    }

    [ContractClassFor(typeof(IMapCommandTarget))]
    abstract class IMapCommandTargetContract : IMapCommandTarget
    {
        public CommandHandler Handler
        {
            get
            {
                Contract.Ensures(Contract.Result<CommandHandler>() != null);
                return default(CommandHandler);
            }
        }
    }
}
