using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace PoleSagTool
{
    public class PoleSagCommands
    {
        [CommandMethod("CPole")]
        public void CreatePole()
        {
            PromptPointResult ppr =
              AcadApp.Ed.GetPoint("\nSelect point: ");
            if (ppr.Status != PromptStatus.OK)return;
            Point3d pt = ppr.Value;

            PromptDoubleResult dRes = AcadApp.Ed.GetDouble("\nEnter height: ");
            if (dRes.Status != PromptStatus.OK) return;
            double height = dRes.Value;

            using (Transaction tr = AcadApp.TM.StartTransaction())
            {
                // Create the solid and set the history flag

                Solid3d sol = new Solid3d();
                sol.RecordHistory = true;

                // Based on http://www.blpole.com/products/5/dimension, a
                // class 2 pole has a circumference (in inches) of
                // roughly .27*h+26 with h in feet
                double circumferenceInches = .27 * height + 26;
                double radiusInches = circumferenceInches / (2 * Math.PI);
                double radius = radiusInches / 12;

                sol.CreateFrustum(height, radius, radius, radius);

                // Add the Solid3d to the modelspace
                tr.InsertEntity(sol);

                // And transform it to the selected point
                sol.TransformBy(Matrix3d.Displacement(pt - Point3d.Origin));
                // At this point the center of the pole is where we want
                // the bottom to be - move it up by half the height
                sol.TransformBy(Matrix3d.Displacement(Vector3d.ZAxis * height / 2));

                tr.Commit();
            }
        }

        [CommandMethod("CSpan")]
        public void CreateSpan()
        {
            using (Transaction tr = AcadApp.TM.StartTransaction())
            {
                Solid3d firstPole = null;
                while (firstPole == null)
                {
                    PromptEntityResult per =
                        AcadApp.Ed.GetEntity("\nPick first pole: ");
                    if (per.Status != PromptStatus.OK) return;
                    firstPole = tr.GetObject(
                        per.ObjectId, OpenMode.ForRead) as Solid3d;
                }

                Solid3d secondPole = null;
                while (secondPole == null)
                {
                    PromptEntityResult per =
                        AcadApp.Ed.GetEntity("\nPick second pole: ");
                    if (per.Status != PromptStatus.OK) return;
                    if (per.ObjectId == firstPole.ObjectId) continue;
                    secondPole = tr.GetObject(
                        per.ObjectId, OpenMode.ForRead) as Solid3d;
                }

                PromptDoubleOptions pdo =
                    new PromptDoubleOptions("\nEnter % extra wire: ");
                pdo.DefaultValue = 10;
                pdo.AllowNone = true;
                PromptDoubleResult dRes = AcadApp.Ed.GetDouble(pdo);
                if (dRes.Status != PromptStatus.OK) return;
                double extraWirePct = dRes.Value;

                Point3d max1 = firstPole.GeometricExtents.MaxPoint;
                Point3d min1 = firstPole.GeometricExtents.MinPoint;
                Point3d pt1 = new Point3d(
                    (max1.X + min1.X) / 2,
                    (max1.Y + min1.Y) / 2,
                    Math.Max(max1.Z, min1.Z));
                Point3d max2 = secondPole.GeometricExtents.MaxPoint;
                Point3d min2 = secondPole.GeometricExtents.MinPoint;
                Point3d pt2 = new Point3d(
                    (max2.X + min2.X) / 2,
                    (max2.Y + min2.Y) / 2,
                    Math.Max(max2.Z, min2.Z));
                Point3d[] pts = { pt1, pt2 };

                Polyline3d pline = new Polyline3d();
                tr.InsertEntity(pline);
                foreach (Point3d pt in pts)
                {
                    using (PolylineVertex3d poly3dVertex = new PolylineVertex3d(pt))
                    {
                        // add them to the 3dpoly (this adds them to the db also)
                        pline.AppendVertex(poly3dVertex);
                    }
                }

                pline.SetSpanData(firstPole, secondPole, extraWirePct);

                tr.Commit();
            }
        }
    }
}
