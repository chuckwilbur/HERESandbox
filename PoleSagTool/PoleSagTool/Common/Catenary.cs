using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoleSagTool.Common
{
    /// <summary>
    /// Quick and dirty duplicate of the Autodesk.AutoCAD.Geometry.Point3d
    /// with just the functions I needed for the catenary calculation
    /// </summary>
    class Point3d
    {
        public Point3d(double[] xyz) : this(xyz[0], xyz[1], xyz[2]) { }
        public Point3d(double x, double y, double z) { X = x; Y = y; Z = z; }

        public double X { get; private set; }
        public double Y { get; private set; }
        public double Z { get; private set; }

        public double DistanceTo(Point3d point)
        {
            return Math.Sqrt(
                Math.Pow(X - point.X, 2) +
                Math.Pow(Y - point.Y, 2) +
                Math.Pow(Z - point.Z, 2));
        }
        public double[] ToArray() { return new double[] { X, Y, Z }; }
    }

    class Catenary
    {/*
        public static void CalculateCatenary(
           Point3d p1, Point3d p2, double? extraWirePct,
           out double a, out double xm, out double ym)
        {
            // Catenary: http://en.wikipedia.org/wiki/Catenary#Derivation_of_equations_for_the_curve
            // y = a cosh (x/a) where x is distance along xy plane below line here
            // sqrt(s^2 - v^2) = 2a sinh (h/2a)
            double s = p1.DistanceTo(p2) * (100 + extraWirePct.Value) / 100;
            double v = p1.Z - p2.Z;
            double sqrtSSqrdMinusVSqrd =
                Math.Sqrt(Math.Pow(s, 2) - Math.Pow(v, 2));
            double h = Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
            a = 0;
            double increment = 1;

            while (increment > .0000000000001)
            {
                int i = 1;
                while (true)
                {
                    double test = a + (i * increment);
                    double diff = 2 * test * Math.Sinh(h / (2 * test)) - sqrtSSqrdMinusVSqrd;
                    if (diff < 0)
                    {
                        a = a + ((i - 1) * increment);
                        increment = increment / 10;
                        break;
                    }
                    ++i;
                }
            }

            if (a < .0000000000001)
            {
                xm = h;
                ym = p2.Z;
                return;
            }

            // Calculate the offset of the minimum point
            // Equation from solving y-ym = a cosh ((x-xm)/a)
            // at (0,y1) and (h,y2) for xm
            // xm=(h-2a arsinh(v/(2a sinh(-h/2a))))/2
            xm = (h - 2 * a * Arsinh(v / (2 * a * Math.Sinh(-h / (2 * a))))) / 2;
            ym = p2.Z - a * Math.Cosh((h - xm) / a);
        }
        */
        public static void CalculateCatenary(
            Point3d p1, Point3d p2, double? extraWirePct,
            out SortedList<double, double> xys, out double xm, out double minZ)
        {
            // Catenary: http://en.wikipedia.org/wiki/Catenary#Derivation_of_equations_for_the_curve
            // y = a cosh (x/a) where x is distance along xy plane below line here
            // sqrt(s^2 - v^2) = 2a sinh (h/2a)
            double s = p1.DistanceTo(p2) * (100 + extraWirePct.Value) / 100;
            double v = p1.Z - p2.Z;
            double sqrtSSqrdMinusVSqrd =
                Math.Sqrt(Math.Pow(s, 2) - Math.Pow(v, 2));
            double h = Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
            double a = 0;
            double ym;
            double increment = 1;

            while (increment > .0000000000001)
            {
                int i = 1;
                while (true)
                {
                    double test = a + (i * increment);
                    double diff = 2 * test * Math.Sinh(h / (2 * test)) - sqrtSSqrdMinusVSqrd;
                    if (diff < 0)
                    {
                        a = a + ((i - 1) * increment);
                        increment = increment / 10;
                        break;
                    }
                    ++i;
                }
            }

            xys = new SortedList<double, double>();
            // For very small (zero) a, draw straight line
            if (a < .0000000000001)
            {
                xys.Add(0, p1.Z);
                xys.Add(h, p2.Z);
                xm = 0;
                minZ = 0;
                return;
            }

            // Calculate the offset of the minimum point
            // Equation from solving y-ym = a cosh ((x-xm)/a)
            // at (0,y1) and (h,y2) for xm
            // xm=(h-2a arsinh(v/(2a sinh(-h/2a))))/2
            xm = (h - 2 * a * Arsinh(v / (2 * a * Math.Sinh(-h / (2 * a))))) / 2;
            ym = p2.Z - a * Math.Cosh((h - xm) / a);

            const int segmentCount = 100;
            for (int i = 0; i <= segmentCount; ++i)
            {
                double currX = h * i / segmentCount;
                xys.Add(currX, a * Math.Cosh((currX - xm) / a) + ym);
            }

            minZ = ym + a;
        }

        public static double Arsinh(double z)
        {
            return Math.Log(z + Math.Sqrt(Math.Pow(z, 2) + 1));
        }
    }
}
