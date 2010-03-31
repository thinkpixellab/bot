using System.Windows;

namespace PixelLab.Common {
  public struct Line {
    public Line(Point p1, Point p2) {
      Util.RequireArgument(p1.IsValid(), "p1");
      Util.RequireArgument(p2.IsValid(), "p2");

      m_p1 = p1;
      m_p2 = p2;
    }

    public Point P1 {
      get { return m_p1; }
      set {
        Util.RequireArgument(value.IsValid(), "value");
        m_p1 = value;
      }
    }

    public Point P2 {
      get { return m_p2; }
      set {
        Util.RequireArgument(value.IsValid(), "value");
        m_p2 = value;
      }
    }

    public Vector Vector { get { return m_p2.Subtract(m_p1); } }

    public double Length { get { return (m_p1.Subtract(m_p2)).Length; } }

    public double Dot(Line other) {
      return Dot(this, other);
    }

    public static double Dot(Line l1, Line l2) {
      return GeoHelper.Dot(l1.Vector, l2.Vector);
    }

    private Point m_p1, m_p2;
  }
}
