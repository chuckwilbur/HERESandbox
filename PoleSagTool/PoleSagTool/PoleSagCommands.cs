using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

using PoleSagTool.EED;

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

                // Based on http://www.blpole.com/products/5/dimension,
                // a class 2 pole has a circumference (in inches) of
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

                Point3d[] pts = { firstPole.GetPoleTop(), secondPole.GetPoleTop() };

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
                firstPole.UpgradeOpen();
                firstPole.SetSpanData(pline);
                secondPole.UpgradeOpen();
                secondPole.SetSpanData(pline);

                tr.Commit();
            }
        }

        [CommandMethod("MSpan")]
        public void ModifySpan()
        {
            using (Transaction tr = AcadApp.TM.StartTransaction())
            {
                Polyline3d span;
                if (PromptForSpan(tr, out span) != PromptStatus.OK) return;

                double extraWirePct = span.GetExtraWirePct().Value;

                PromptDoubleOptions pdo =
                    new PromptDoubleOptions("\nEnter % extra wire: ");
                pdo.DefaultValue = extraWirePct;
                pdo.AllowNone = true;
                PromptDoubleResult dRes = AcadApp.Ed.GetDouble(pdo);
                if (dRes.Status != PromptStatus.OK) return;
                extraWirePct = dRes.Value;

                span.UpgradeOpen();
                span.SetExtraWirePct(extraWirePct);

                tr.Commit();
            }
        }

        [CommandMethod("TDim")]
        public void ToggleSpanDimensioning()
        {
            SpanDrawOverrule.ToggleSpanDimensioning();
            AcadApp.Ed.Regen();
        }

        private static PromptStatus PromptForSpan(Transaction tr, out Polyline3d span)
        {
            span = null;
            while (span == null || !span.GetExtraWirePct().HasValue)
            {
                PromptEntityResult per =
                    AcadApp.Ed.GetEntity("\nPick span: ");
                if (per.Status != PromptStatus.OK) return per.Status;
                span = tr.GetObject(
                    per.ObjectId, OpenMode.ForRead) as Polyline3d;
            }
            return PromptStatus.OK;
        }
    }
}
