using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using PixelLab.Common;

namespace PixelLab.Wpf.Demo.FolderPicker
{
    public class SelectableDirectory : Changeable
    {
        #region Constructors

        internal SelectableDirectory(string path) : this(new DirectoryInfo(path)) { }
        internal SelectableDirectory(DirectoryInfo di)
        {
            m_directoryInfo = di;
            m_isDrive = false;
        }
        internal SelectableDirectory(DriveInfo di)
        {
            m_directoryInfo = di.RootDirectory;
            m_isDrive = true;
        }

        #endregion

        public IList<SelectableDirectory> SubDirectories
        {
            get
            {
                if (m_subDirectories == null)
                {
                    var directoryList = Enumerable.Empty<SelectableDirectory>();

                    try
                    {
                        directoryList = from di in m_directoryInfo.GetDirectories()
                                        select new SelectableDirectory(di);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        //the directory may be restricted
                    }

                    m_subDirectories = directoryList.ToReadOnlyCollection();
                    foreach (var subDir in m_subDirectories)
                    {
                        subDir.AddWatcher(c_isSelectedPropertyName, c_selectedDirectoriesPropertyName, childSelectionChanged);
                    }
                }

                return m_subDirectories;
            }
        }
        public IEnumerable<SelectableDirectory> SelectedDirectories
        {
            get
            {
                return GetSelectedDirectories(this.m_subDirectories);
            } // get
        } //*** IEnumerable<SelectableDirectory> SelectedDirectiories

        public string Name
        {
            get
            {
                if (m_isDrive)
                {
                    string name = m_directoryInfo.Name;
                    name = (name.EndsWith("\\")) ? name.Substring(0, name.Length - 1) : name;
                    return string.Format("Local Disk ({0})", name);
                }
                else
                {
                    return m_directoryInfo.Name;
                }
            }
        }
        public string Path
        {
            get
            {
                return m_directoryInfo.FullName;
            }
        }
        public bool IsSelected
        {
            get
            {
                return m_isSelected;
            }
            set
            {
                bool changed = (m_isSelected != value);
                if (changed)
                {
                    m_isSelected = value;
                    isSelectedChanged();
                }
            }
        }
        public ChildSelection ChildSelection
        {
            get
            {
                int childCount, selectionCount;
                if (m_subCountCache.HasValue && m_subSelectionCountCache.HasValue)
                {
                    childCount = m_subCountCache.Value;
                    selectionCount = m_subSelectionCountCache.Value;
                }
                else
                {
                    getChildSelectionCount(out childCount, out selectionCount);

                    m_subSelectionCountCache = selectionCount;
                    m_subCountCache = childCount;
                }

                if (selectionCount == 0)
                {
                    return ChildSelection.None;
                }
                else if (childCount == selectionCount)
                {
                    return ChildSelection.All;
                }
                else
                {
                    return ChildSelection.Some;
                }
            }
        }
        public bool IsDrive
        {
            get
            {
                return m_isDrive;
            }
        }

        public override string ToString()
        {
            return m_directoryInfo.FullName;
        }

        internal static IEnumerable<SelectableDirectory> GetSelectedDirectories(IEnumerable<SelectableDirectory> source)
        {
            if (source == null)
            {
                return Enumerable.Empty<SelectableDirectory>();
            }
            else
            {
                return source
                    .SelectRecursive(directory => directory.m_subDirectories.EmptyIfNull())
                    .Where(directory => directory.IsSelected);
            }
        }

        #region Implementation

        private void isSelectedChanged()
        {
            OnPropertyChanged(c_isSelectedPropertyName);
            OnPropertyChanged(c_selectedDirectoriesPropertyName);
        }

        private void childSelectionChanged()
        {
            m_subSelectionCountCache = null;
            m_subCountCache = null;

            OnPropertyChanged(c_childSelectionPropertyName);
            OnPropertyChanged(c_selectedDirectoriesPropertyName);
        }

        private void getChildSelectionCount(out int childCount, out int selectionCount)
        {
            childCount = 0;
            selectionCount = 0;

            if (m_subDirectories != null)
            {
                foreach (SelectableDirectory directory in m_subDirectories.SelectRecursive(dir => dir.m_subDirectories.EmptyIfNull()))
                {
                    childCount++;
                    if (directory.IsSelected)
                    {
                        selectionCount++;
                    }
                }
            }
        }

        private ReadOnlyCollection<SelectableDirectory> m_subDirectories;

        private int? m_subCountCache;
        private int? m_subSelectionCountCache;
        private bool m_isSelected;

        private readonly DirectoryInfo m_directoryInfo;
        private readonly bool m_isDrive;

        private const string c_isSelectedPropertyName = "IsSelected";
        private const string c_childSelectionPropertyName = "ChildSelection";
        internal const string c_selectedDirectoriesPropertyName = "SelectedDirectories";

        #endregion
    }
}
