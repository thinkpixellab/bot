using System;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif
using System.Windows;

namespace PixelLab.Common
{
    public struct Line : IEquatable<Line>
    {
        public Line(Point p1, Point p2)
        {
            Contract.Requires(p1.IsValid());
            Contract.Requires(p2.IsValid());

            m_p1 = p1;
            m_p2 = p2;
        }

        public Point P1
        {
            get { return m_p1; }
            set
            {
                Contract.Requires(value.IsValid());
                m_p1 = value;
            }
        }

        public Point P2
        {
            get { return m_p2; }
            set
            {
                Contract.Requires(value.IsValid());
                m_p2 = value;
            }
        }

        public Vector Vector { get { return m_p2.Subtract(m_p1); } }

        public double Length { get { return (m_p1.Subtract(m_p2)).Length; } }

        public double Dot(Line other)
        {
            return Dot(this, other);
        }

        public static double Dot(Line l1, Line l2)
        {
            return GeoHelper.Dot(l1.Vector, l2.Vector);
        }

        public bool Equals(Line other)
        {
            return m_p1.Equals(other.m_p1) && m_p2.Equals(other.m_p2);
        }

        public override bool Equals(object obj)
        {
            if (obj is Line)
            {
                return Equals((Line)obj);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public static bool operator ==(Line l1, Line l2)
        {
            return l1.Equals(l2);
        }

        public static bool operator !=(Line l1, Line l2)
        {
            return !l1.Equals(l2);
        }

        private Point m_p1, m_p2;
    }
}
