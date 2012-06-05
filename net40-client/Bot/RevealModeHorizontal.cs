namespace PixelLab.Wpf
{
    public enum HorizontalRevealMode
    {
        /// <summary>
        ///     No horizontal reveal animation.
        /// </summary>
        None,

        /// <summary>
        ///     Reveal from the left to the right.
        /// </summary>
        FromLeftToRight,

        /// <summary>
        ///     Reveal from the right to the left.
        /// </summary>
        FromRightToLeft,

        /// <summary>
        ///     Reveal from the center to the bounding edge.
        /// </summary>
        FromCenterToEdge,
    }
}
