#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif

namespace PixelLab.Common
{
    // This class exists for a very simple reason.
    // Exposing the Mapper publicly from a class is stupid. We don't want AddCommand exposed for anyone.
    // So this exists to allow CommandMapper plumbing to get at the needed functionality, without exposing
    //  its world to the whole world
    public class CommandHandler
    {
        internal CommandHandler(CommandMapper mapper)
        {
            Contract.Requires(mapper != null);
            m_mapper = mapper;
        }
        internal CommandMapper Mapper
        {
            get
            {
                Contract.Ensures(Contract.Result<CommandMapper>() != null);
                return m_mapper;
            }
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(m_mapper != null);
        }
        private readonly CommandMapper m_mapper;
    }

}
