using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security;
using PixelLab.Common;

namespace PixelLab.Wpf.Demo.FolderPicker
{
    public class LocalDrives : Changeable
    {
        public LocalDrives()
        {
            var drives = Enumerable.Empty<SelectableDirectory>();

            try
            {
                drives = from d in DriveInfo.GetDrives()
                         where d.DriveType == DriveType.Fixed && d.IsReady
                         select new SelectableDirectory(d);
            }
            catch (SecurityException) { } // This will fail in an XBap

            m_drives = drives.ToReadOnlyCollection();

            foreach (var sd in m_drives)
            {
                sd.AddWatcher(SelectableDirectory.c_selectedDirectoriesPropertyName, () => OnPropertyChanged(SelectableDirectory.c_selectedDirectoriesPropertyName));
            }
        }

        public IList<SelectableDirectory> Drives
        {
            get
            {
                return m_drives;
            }
        }
        public IEnumerable<SelectableDirectory> SelectedDirectories
        {
            get
            {
                return SelectableDirectory.GetSelectedDirectories(m_drives);
            }
        }

        #region Implementation

        private readonly ReadOnlyCollection<SelectableDirectory> m_drives;

        #endregion
    }
}
