using System.Diagnostics.Contracts;

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
