using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using PixelLab.Common;

namespace PixelLab.Wpf.Demo
{
    public class DataTree
    {
        public DataTree(Guid guid, IList<DataTree> children)
        {
            m_guid = guid;
            m_children = new ReadOnlyCollection<DataTree>(children);
        }

        public Guid Guid { get { return m_guid; } }
        public IList<DataTree> Children { get { return m_children; } }

        public override string ToString()
        {
            return m_guid.ToString();
        }

        public static DataTree GetRandomDataTree(int maxDepth, int maxWidth)
        {
            Debug.Assert(maxDepth >= 0);
            Debug.Assert(maxWidth >= 0);

            int depth = Util.Rnd.Next(maxDepth);
            int width = Util.Rnd.Next(maxWidth);

            List<DataTree> children = new List<DataTree>();
            if (depth > 0)
            {
                for (int i = 0; i < width; i++)
                {
                    children.Add(GetRandomDataTree(maxDepth - 1, maxWidth));
                }
            }

            return new DataTree(Guid.NewGuid(), children);
        }

        #region Implementation

        private readonly Guid m_guid;
        private readonly ReadOnlyCollection<DataTree> m_children;

        #endregion
    }
}