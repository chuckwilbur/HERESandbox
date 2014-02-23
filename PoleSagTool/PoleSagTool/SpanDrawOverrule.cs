using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace PoleSagTool
{
    class SpanDrawOverrule : DrawableOverrule
    {
        static RXClass _targetClass = RXObject.GetClass(typeof(Polyline3d));
        static SpanDrawOverrule _instance = new SpanDrawOverrule();

        public static void Add()
        {
            Overrule.AddOverrule(_targetClass, _instance, false);
            _instance.SetSpanDataFilter();
        }

        public static void Remove()
        {
            Overrule.RemoveOverrule(_targetClass, _instance);
        }

        public override bool WorldDraw(Drawable drawable, WorldDraw wd)
        {
            var pline = drawable as Polyline3d;
            if (pline == null) return base.WorldDraw(drawable, wd);
            double? extraWirePct = pline.GetExtraWirePct();
            if (!extraWirePct.HasValue) return base.WorldDraw(drawable, wd);

            // TODO: Factor actual catenary calculations into common Non-AutoCAD-dependent class
            // Catenary: http://en.wikipedia.org/wiki/Catenary#Derivation_of_equations_for_the_curve
            // y = a cosh (x/a) where x is distance along line here
            // sqrt(s^2 - v^2) = 2a sinh (h/2a)
            double s = pline.Length * (100 + extraWirePct.Value) / 100;
            Point3d p1 = pline.GetPointAtDist(0);
            Point3d p2 = pline.GetPointAtDist(pline.Length);
            double v = p1.Z - p2.Z;
            double sqrtSSqrdMinusVSqrd =
                Math.Sqrt(Math.Pow(s, 2) - Math.Pow(v, 2));
            double h = Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
            double a = 0, increment = 1;
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
                        i = 1;
                        break;
                    }
                    ++i;
                }
            }

            // For very small (zero) a, draw straight line
            if (a < .0000000000001) return base.WorldDraw(drawable, wd);

            // Calculate the offset of the minimum point
            // Equation from solving y-ym = a cosh ((x-xm)/a)
            // at (0,y1) and (h,y2) for xm
            // xm=(h-2a arsinh(v/(2a sinh(-h/2a))))/2
            double xm = (h - 2 * a * Arsinh(v / (2 * a * Math.Sinh(-h / (2 * a))))) / 2;
            double ym = p2.Z - a * Math.Cosh((h-xm) / a);

            var pts = new Point3dCollection();
            for (int i = 0; i <= 100; ++i)
            {
                double x = i * pline.Length / 100;
                if (x > pline.Length) x = pline.Length;
                Point3d pt = pline.GetPointAtDist(x);
                Point3d adjustedPt = new Point3d(pt.X, pt.Y, a * Math.Cosh((x - xm) / a) + ym);
                pts.Add(adjustedPt);
            }

            // Store old graphics color and set to the color we want
            short oldColor = wd.SubEntityTraits.Color;
            wd.SubEntityTraits.Color = 1;

            wd.Geometry.Polyline(pts, Vector3d.ZAxis, IntPtr.Zero);

            if (xm > 0 && xm < pline.Length)
            {
                // Draw dimension from lowest point to ground
                Point3d pt = pline.GetPointAtDist(xm);
                pts.Clear();
                pts.Add(new Point3d(pt.X, pt.Y, a + ym));
                pts.Add(new Point3d(pt.X, pt.Y, 0));

                wd.SubEntityTraits.Color = 5;

                // Draw the polyline
                wd.Geometry.Polyline(pts, Vector3d.YAxis, IntPtr.Zero);

                // Draw the dimension text
                Plane xyPlane=new Plane();
                Vector3d lineDirInPlane =new Vector3d(xyPlane, (p2-p1).Convert2d(xyPlane));
                wd.Geometry.Text(
                    new Point3d(pt.X, pt.Y, (a + ym) / 2),
                    lineDirInPlane.RotateBy(-Math.PI / 2, Vector3d.ZAxis),
                    lineDirInPlane,
                    1, 1, 0,
                    (a + ym).ToString("0.00"));
            }

            // Restore old settings
            wd.SubEntityTraits.Color = oldColor; 

            return true;
        }

        public static double Arsinh(double z)
        {
            return Math.Log(z + Math.Sqrt(Math.Pow(z, 2) + 1));
        }
    }
}
