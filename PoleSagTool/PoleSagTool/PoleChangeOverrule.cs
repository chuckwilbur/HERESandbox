using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

using PoleSagTool.EED;

namespace PoleSagTool
{
    /// <summary>
    /// This was going to be a TransformOverrule, but TransformBy
    /// doesn't catch changes to Solid3d objects
    /// Even though I'm using a reactor, I'm keeping the same
    /// public interface as the SpanDrawOverrule for external
    /// consistency
    /// </summary>
    class PoleChangeOverrule
    {
        static readonly RXClass _targetClass = RXObject.GetClass(typeof(Solid3d));
        static PoleChangeOverrule _instance = new PoleChangeOverrule();

        public static void Add()
        {
            AcadApp.DB.ObjectModified += DB_ObjectModified;
        }

        static void DB_ObjectModified(object sender, ObjectEventArgs e)
        {
            if (e.DBObject is Solid3d)
            {
                var db = sender as Database;
                if (db == null) return;
                var pole = e.DBObject as Solid3d;
                if (pole == null) return;

                using (OpenCloseTransaction tr = db.TransactionManager.StartOpenCloseTransaction())
                {
                    foreach (Handle spanHandle in pole.GetSpans())
                    {
                        ObjectId spanId = AcadApp.DB.GetObjectId(false, spanHandle, 0);
                        if (spanId == ObjectId.Null) continue;
                        var span = tr.GetObject(
                            spanId, OpenMode.ForRead) as Polyline3d;
                        if (span == null) continue;

                        ObjectId vertexId = span.GetVertexAtPole(pole.Handle);
                        var vertex = tr.GetObject(
                            vertexId, OpenMode.ForWrite) as PolylineVertex3d;
                        vertex.Position = pole.GetPoleTop();
                    }

                    tr.Commit();
                }
            }
        }

        public static void Remove()
        {
            AcadApp.DB.ObjectModified -= DB_ObjectModified;
        }
    }
}
