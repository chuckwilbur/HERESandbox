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
    static class PointConvert
    {
        public static Common.Point3d ToCommonPoint3d(this Point3d acadPt)
        {
            return new Common.Point3d(acadPt.ToArray());
        }

        public static Point3d ToAcadPoint3d(this Common.Point3d commonPt)
        {
            return new Point3d(commonPt.ToArray());
        }
    }

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

            Point3d p1 = pline.GetPointAtDist(0);
            Point3d p2 = pline.GetPointAtDist(pline.Length);
            SortedList<double, double> xys;
            double xm;
            double minZ;
            Common.Catenary.CalculateCatenary(
                p1.ToCommonPoint3d(), p2.ToCommonPoint3d(),
                extraWirePct, out xys, out xm, out minZ);

            var plineXY = new Vector3d(p2.X - p1.X, p2.Y - p1.Y, 0);

            var pts = new Point3dCollection();
            foreach (var xy in xys)
            {
                Vector3d currDisp = plineXY * xy.Key / plineXY.Length;
                Point3d pt = p1 + currDisp;
                var adjustedPt = new Point3d(pt.X, pt.Y, xy.Value);
                pts.Add(adjustedPt);
            }

            // Store old graphics color and set to the color we want
            short oldColor = wd.SubEntityTraits.Color;
            wd.SubEntityTraits.Color = 1;

            wd.Geometry.Polyline(pts, Vector3d.ZAxis, IntPtr.Zero);

            if (xm > 0 && xm < plineXY.Length)
            {
                // Draw dimension from lowest point to ground
                Vector3d mDisp = plineXY * xm / plineXY.Length;
                Point3d pt = p1 + mDisp;
                pts.Clear();
                pts.Add(new Point3d(pt.X, pt.Y, minZ));
                pts.Add(new Point3d(pt.X, pt.Y, 0));

                wd.SubEntityTraits.Color = 5;

                // Draw the polyline
                wd.Geometry.Polyline(pts, Vector3d.YAxis, IntPtr.Zero);

                // Draw the dimension text
                Plane xyPlane=new Plane();
                Vector3d lineDirInPlane =new Vector3d(xyPlane, (p2-p1).Convert2d(xyPlane));
                wd.Geometry.Text(
                    new Point3d(pt.X, pt.Y, minZ / 2),
                    lineDirInPlane.RotateBy(-Math.PI / 2, Vector3d.ZAxis),
                    lineDirInPlane,
                    1, 1, 0,
                    minZ.ToString("0.00"));
            }

            // Restore old settings
            wd.SubEntityTraits.Color = oldColor; 

            return true;
        }
    }
}
