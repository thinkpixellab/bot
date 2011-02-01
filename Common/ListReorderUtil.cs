using System.Collections.Generic;
using System.Diagnostics;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif
using System.Linq;

namespace PixelLab.Common
{
    public static class ListReorderUtil
    {
        public static bool CanReorder(IList<int> indicies, int length, ReorderDirection direction)
        {
            var result = Reorder(indicies, length, direction);
            return result != null;
        }

        public static IList<int> Reorder(IList<int> indicies, int length, ReorderDirection moveDirection)
        {
            Contract.Requires(indicies != null, "indicies cannot be null");
            Contract.Requires(length >= 0, "the target length cannot be negative");
            Contract.Requires(indicies.Count <= length, "cannot have more indicies than the target length");
            Contract.Requires(Contract.ForAll(indicies, i => i >= 0), "All indicies must be non-negative");
            Contract.Requires(Contract.ForAll(indicies, i => i < length), "All indicies must be less than the target length");
            Debug.Assert(indicies.TrueForAllAdjacentPairs((a, b) => a < b), "indicies should be sorted (and unique)");

            if (indicies.Count > 0 && indicies.Count < length)
            {
                // a bit silly, but I hate doing math in comparisons
                int lastPossibleIndex = length - 1;

                // Two cases: bunched and unbunched
                // 1 - Unbunched: two or more selected items that are not packed together
                //     Can always move unbunched items
                // TODO: unbunched not supported at this time

                // 2 - Bunched: one or more items, all together
                //     If items are bunched, can only move if there is room to move them in the desired direction

                var bunched = indicies.TrueForAllAdjacentPairs((a, b) => (b - a) == 1);
                if (bunched)
                {
                    if (moveDirection == ReorderDirection.Beginning)
                    {
                        int leadIndex = indicies.First();
                        if (leadIndex == 0)
                        {
                            // already at the beginning
                            // cannot move
                            return null;
                        }
                        else
                        {
                            // not at the beginning!
                            // move everything towards beginning!
                            return indicies.Select(initial => initial - 1).ToArray();
                        }
                    }
                    else
                    {
                        Debug.Assert(moveDirection == ReorderDirection.End);
                        int leadIndex = indicies.Last();
                        if (leadIndex == lastPossibleIndex)
                        {
                            // already at the end
                            // cannot move
                            return null;
                        }
                        else
                        {
                            return indicies.Select(initial => initial + 1).ToArray();
                        }
                    }
                }
            }
            return null;
        }
    }

    public enum ReorderDirection { Beginning, End };
}
