using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CornishRoom
{
    public class Point3D
    {
        public float X;
        public float Y;
        public float Z;

        public Point3D(float x, float y, float z = 0)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Point ConvertToPoint()
        {
            return new Point((int)X, (int)Y);
        }
        
        public float DistanceTo(Point3D p2)
        {
            return (float)Math.Sqrt((X - p2.X) * (X - p2.X) + (Y - p2.Y) * (Y - p2.Y) + (Z - p2.Z) * (Z - p2.Z));
        }
        
        public float Length()
        {
            return (float)Math.Sqrt(X * X + Y * Y + Z * Z);
        }
        
        public static float Scalar(Point3D p1, Point3D p2)
        {
            return p1.X * p2.X + p1.Y * p2.Y + p1.Z * p2.Z;
        }

        public static Point3D Normalize(Point3D p)
        {
            float z = (float)Math.Sqrt((float)(p.X * p.X + p.Y * p.Y + p.Z * p.Z));
            if (z == 0)
                return new Point3D(0, 0, 0);
            return new Point3D(p.X / z, p.Y / z, p.Z / z);
        }

        public static Point3D operator +(Point3D point1, Point3D point2)
        {
            return new Point3D(point1.X + point2.X, point1.Y + point2.Y, point1.Z + point2.Z);
        }

        public static Point3D operator -(Point3D point1, Point3D point2)
        {
            return new Point3D(point1.X - point2.X, point1.Y - point2.Y, point1.Z - point2.Z);
        }
        
        public static Point3D operator *(Point3D p1, Point3D p2)
        {
            return new Point3D(p1.Y * p2.Z - p1.Z * p2.Y, p1.Z * p2.X - p1.X * p2.Z, p1.X * p2.Y - p1.Y * p2.X);
        }

        public static Point3D operator *(float t, Point3D p1)
        {
            return new Point3D(p1.X * t, p1.Y * t, p1.Z * t);
        }


        public static Point3D operator *(Point3D p1, float t)
        {
            return new Point3D(p1.X * t, p1.Y * t, p1.Z * t);
        }

        public static Point3D operator -(Point3D p1, float t)
        {
            return new Point3D(p1.X - t, p1.Y - t, p1.Z - t);
        }

        public static Point3D operator -(float t, Point3D p1)
        {
            return new Point3D(t - p1.X, t - p1.Y, t - p1.Z);
        }

        public static Point3D operator +(Point3D p1, float t)
        {
            return new Point3D(p1.X + t, p1.Y + t, p1.Z + t);
        }

        public static Point3D operator +(float t, Point3D p1)
        {
            return new Point3D(p1.X + t, p1.Y + t, p1.Z + t);
        }

        public static Point3D operator /(Point3D p1, float t)
        {
            return new Point3D(p1.X / t, p1.Y / t, p1.Z / t);
        }

        public static Point3D operator /(float t, Point3D p1)
        {
            return new Point3D(t / p1.X, t / p1.Y, t / p1.Z);
        }
    }
}