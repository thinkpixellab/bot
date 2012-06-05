using System;
using System.Diagnostics;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using PixelLab.Common;

namespace PixelLab.Wpf
{
    public static class WpfUtil
    {
        public static bool Animate(
            Point3D currentValue, Vector3D currentVelocity, Point3D targetValue,
            double attractionFator, double dampening,
            double terminalVelocity, double minValueDelta, double minVelocityDelta,
            out Point3D newValue, out Vector3D newVelocity)
        {
            Debug.Assert(currentValue.IsValid());
            Debug.Assert(currentVelocity.IsValid());
            Debug.Assert(targetValue.IsValid());

            Debug.Assert(dampening.IsValid());
            Debug.Assert(dampening > 0 && dampening < 1);

            Debug.Assert(attractionFator.IsValid());
            Debug.Assert(attractionFator > 0);

            Debug.Assert(terminalVelocity > 0);

            Debug.Assert(minValueDelta > 0);
            Debug.Assert(minVelocityDelta > 0);

            Vector3D diff = targetValue - currentValue;

            if (diff.Length > minValueDelta || currentVelocity.Length > minVelocityDelta)
            {
                newVelocity = currentVelocity * (1 - dampening);
                newVelocity += diff * attractionFator;
                newVelocity *= (currentVelocity.Length > terminalVelocity) ? terminalVelocity / currentVelocity.Length : 1;

                newValue = currentValue + newVelocity;

                return true;
            }
            else
            {
                newValue = targetValue;
                newVelocity = new Vector3D();
                return false;
            }
        }

        public static bool IsValid(this Point3D value)
        {
            return value.X.IsValid() && value.Y.IsValid() && value.Z.IsValid();
        }

        public static bool IsValid(this Vector3D value)
        {
            return value.X.IsValid() && value.Y.IsValid() && value.Z.IsValid();
        }

        public static void SetToVector(this TranslateTransform3D translateTransform3D, Vector3D vector3D)
        {
            Contract.Requires(translateTransform3D != null);
            translateTransform3D.OffsetX = vector3D.X;
            translateTransform3D.OffsetY = vector3D.Y;
            translateTransform3D.OffsetZ = vector3D.Z;
        }

        public static void SetToVector(this TranslateTransform translateTransform, Vector vector)
        {
            Contract.Requires(translateTransform != null);
            translateTransform.X = vector.X;
            translateTransform.Y = vector.Y;
        }

        public static Vector ToVector(this Transform transform)
        {
            Contract.Requires(transform != null);
            return new Vector(transform.Value.OffsetX, transform.Value.OffsetY);
        }

        public static Point ToPoint(this Transform transform)
        {
            Contract.Requires(transform != null);
            return new Point(transform.Value.OffsetX, transform.Value.OffsetY);
        }

        /// <param name="angleRadians">The angle, in radians, from 3-o'clock going counter-clockwise.</param>
        public static void DrawLine(this DrawingContext drawingContext, Pen pen, Point startPoint, double angleRadians, double length)
        {
            Contract.Requires<ArgumentNullException>(drawingContext != null);
            Contract.Requires<ArgumentException>(startPoint.IsValid());
            Contract.Requires<ArgumentNullException>(pen != null);

            drawingContext.DrawLine(pen, startPoint, startPoint + GeoHelper.GetVectorFromAngle(angleRadians, length));
        }
    }
} //*** namespace PixelLab.Wpf
