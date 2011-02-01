// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

namespace System.Windows.Controls
{
    /// <summary>
    /// Describes the direction that content is scaled.
    /// </summary>
    /// <QualityBand>Stable</QualityBand>
    public enum StretchDirection
    {
        /// <summary>
        /// The content scales upward only when it is smaller than the parent.
        /// If the content is larger, no scaling downward is performed.
        /// </summary>
        UpOnly = 0,

        /// <summary>
        /// The content scales downward only when it is larger than the parent.
        /// If the content is smaller, no scaling upward is performed.
        /// </summary>
        DownOnly = 1,

        /// <summary>
        /// The content stretches to fit the parent according to the
        /// <see cref="P:System.Windows.Controls.Viewbox.Stretch" /> property.
        /// </summary>
        Both = 2
    }
}