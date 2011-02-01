using System;
using System.Windows;

namespace PixelLab.Common
{
    public struct Vector : IEquatable<Vector>
    {
        public Vector(double x, double y)
        {
            m_x = x;
            m_y = y;
        }

        public double X { get { return m_x; } set { m_x = value; } }
        public double Y { get { return m_y; } set { m_y = value; } }

        public double Length
        {
            get
            {
                return Math.Sqrt((m_x * m_x) + (m_y * m_y));
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector)
            {
                return Equals((Vector)obj);
            }
            else
            {
                return false;
            }
        }

        public bool Equals(Vector other)
        {
            return m_x == other.m_x && m_y == other.m_y;
        }

        public override int GetHashCode()
        {
            return Util.GetHashCode(m_x, m_y);
        }

        public override string ToString()
        {
            return "{0}: {1}, {2}".DoFormat(base.ToString(), m_x, m_y);
        }

        private double m_x, m_y;

        public static Vector operator -(Vector vector1, Vector vector2)
        {
            return new Vector(vector1.X - vector2.X, vector1.Y - vector2.Y);
        }

        public static Vector operator *(double scalar, Vector vector)
        {
            return new Vector(vector.m_x * scalar, vector.m_y * scalar);
        }

        public static Vector operator *(Vector vector, double scalar)
        {
            return new Vector(vector.m_x * scalar, vector.m_y * scalar);
        }

        public static Vector operator +(Vector vector1, Vector vector2)
        {
            return new Vector(vector1.m_x + vector2.m_x, vector1.m_y + vector2.m_y);
        }

        public static Point operator +(Vector vector, Point point)
        {
            return new Point(point.X + vector.m_x, point.Y + vector.m_y);
        }

        public static Point operator +(Point point, Vector vector)
        {
            return new Point(point.X + vector.m_x, point.Y + vector.m_y);
        }

        public static explicit operator Point(Vector vector)
        {
            return new Point(vector.X, vector.Y);
        }

        public static explicit operator Vector(Point point)
        {
            return new Vector(point.X, point.Y);
        }

        public static explicit operator Vector(Size size)
        {
            return new Vector(size.Width, size.Height);
        }

        public static bool operator ==(Vector p1, Vector p2)
        {
            return p1.Equals(p2);
        }

        public static bool operator !=(Vector p1, Vector p2)
        {
            return !p1.Equals(p2);
        }
    }
}
