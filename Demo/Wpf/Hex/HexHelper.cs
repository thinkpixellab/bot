using System;
using System.Windows;
using PixelLab.Common;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif

namespace PixelLab.Wpf.Demo.Hex
{
    static class HexHelper
    {
        public static readonly double HeightOverWidth = Math.Sqrt(3) / 2;

        public static Point GetTopLeft(int count, double itemHeight, PointInt location)
        {
            Contract.Requires<ArgumentOutOfRangeException>(count >= 1, "count");
            Contract.Requires<ArgumentOutOfRangeException>(itemHeight > 0 && itemHeight.IsValid(), "itemHeight");

            double itemWidth = itemHeight / HeightOverWidth;

            //determine the location of 0,0
            Point point = new Point(0, itemHeight * .5 * (count - 1));

            //determine the start point for the row
            point.X += (itemWidth * 3 / 4) * location.Row;
            point.Y += (itemHeight / 2) * location.Row;

            //calculate the offset for the column
            point.X += (itemWidth * 3 / 4) * location.Column;
            point.Y -= (itemHeight / 2) * location.Column;

            return point;
        }
    }
}
