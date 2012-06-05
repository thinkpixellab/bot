using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using PixelLab.Common;

namespace PixelLab.SL.IsolatedStorage
{
    public abstract class IsolatedFileSystemInfo
    {
        internal IsolatedFileSystemInfo(IsolatedStorageFile isf, IEnumerable<string> path)
        {
            Contract.Requires(isf != null);
            Contract.Requires(path != null);
            m_isf = isf;
            m_path = path.ToReadOnlyCollection();
            m_pathString = String.Join(Path.DirectorySeparatorChar.ToString(), m_path.ToArray());
        }

        public string Name { get { return m_path.LastOrDefault() ?? Path.DirectorySeparatorChar.ToString(); } }

        public virtual string PathString { get { return m_pathString; } }

        public IList<string> PathItems { get { return m_path; } }

        public override string ToString()
        {
            return PathString;
        }

        protected readonly IsolatedStorageFile m_isf;
        private readonly ReadOnlyCollection<string> m_path;
        private readonly string m_pathString;
    }

    public sealed class IsolatedFileInfo : IsolatedFileSystemInfo
    {
        internal IsolatedFileInfo(IsolatedStorageFile isf, IEnumerable<string> path)
            : base(isf, path)
        {
            Contract.Requires(!path.IsEmpty());
        }
    }

    public sealed class IsolatedDirectoryInfo : IsolatedFileSystemInfo
    {
        private IsolatedDirectoryInfo(IsolatedStorageFile isf, IEnumerable<string> path)
            : base(isf, path) { }

        public IEnumerable<IsolatedDirectoryInfo> GetDirectories()
        {
            return m_isf.GetDirectoryNames(getWildcard()).Select(str => new IsolatedDirectoryInfo(m_isf, base.PathItems.Concat(str)));
        }

        public IEnumerable<IsolatedFileInfo> GetFiles()
        {
            return m_isf.GetFileNames(getWildcard()).Select(str => new IsolatedFileInfo(m_isf, base.PathItems.Concat(str)));
        }

        public static IsolatedDirectoryInfo GetRoot(IsolatedStorageFile isolatedStorageFile)
        {
            return new IsolatedDirectoryInfo(isolatedStorageFile, Enumerable.Empty<string>());
        }

        public override string PathString
        {
            get
            {
                return base.PathString + Path.DirectorySeparatorChar;
            }
        }

        private string getWildcard()
        {
            return this.PathString + '*';
        }
    }
}
