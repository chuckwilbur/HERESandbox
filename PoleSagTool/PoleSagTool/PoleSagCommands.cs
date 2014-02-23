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

            using (Transaction tr = AcadApp.DB.TransactionManager.StartTransaction())
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
                BlockTable bt = (BlockTable)tr.GetObject(
                    AcadApp.DB.BlockTableId, OpenMode.ForRead);

                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(
                    bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                ms.AppendEntity(sol);
                tr.AddNewlyCreatedDBObject(sol, true);

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
            AcadApp.Ed.WriteMessage("Called Create Span");
        }
    }
}
