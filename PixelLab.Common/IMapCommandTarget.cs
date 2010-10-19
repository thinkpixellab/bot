
using System.Diagnostics.Contracts;
namespace PixelLab.Common {
  [ContractClass(typeof(IMapCommandTargetContract))]
  public interface IMapCommandTarget {
    CommandMapper.Manager Manager { get; }
  }

  [ContractClassFor(typeof(IMapCommandTarget))]
  abstract class IMapCommandTargetContract : IMapCommandTarget {
    public CommandMapper.Manager Manager {
      get {
        Contract.Ensures(Contract.Result<CommandMapper.Manager>() != null);
        return default(CommandMapper.Manager);
      }
    }
  }
}
