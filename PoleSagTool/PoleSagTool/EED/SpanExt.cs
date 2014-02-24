using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

namespace PoleSagTool.EED
{
    public static class SpanExt
    {
        const string POLE1 = "POLE1";
        const string POLE2 = "POLE2";
        const string EXTRA_PCT = "EXTRA_PCT";

        public static ObjectId GetVertexAtPole(this Polyline3d pline, Handle poleHandle)
        {
            Handle[] poles = pline.GetPoles();
            if (poles == null) return ObjectId.Null;

            ObjectId[] verts = pline.Cast<ObjectId>().ToArray();
            if (verts.Length < 2) return ObjectId.Null;

            if (poles[0] == poleHandle) return verts[0];
            if (poles[1] == poleHandle) return verts[1];
            return ObjectId.Null;
        }

        public static ObjectId GetPoleAtVertex(this Polyline3d pline, ObjectId vertexId)
        {
            ObjectId[] verts = pline.Cast<ObjectId>().ToArray();
            if (verts.Length < 2) return ObjectId.Null;

            Handle[] poles = pline.GetPoles();
            if (poles == null) return ObjectId.Null;

            if (verts[0] == vertexId) return AcadApp.DB.GetObjectId(false, poles[0], 0);
            if (verts[1] == vertexId) return AcadApp.DB.GetObjectId(false, poles[1], 0);
            return ObjectId.Null;
        }

        static Handle[] GetPoles(this Polyline3d pline)
        {
            var xData = new XData(pline, Ext.RegisteredAppName);
            Dictionary<string, object> data = xData.GetAppData();
            Handle[] result = null;
            if (data.Count > 0)
            {
                result = new Handle[2];
                result[0] = new Handle(Int64.Parse(data[POLE1].ToString(), NumberStyles.HexNumber));
                result[1] = new Handle(Int64.Parse(data[POLE2].ToString(), NumberStyles.HexNumber));
            }
            return result;
        }

        public static double? GetExtraWirePct(this Polyline3d pline)
        {
            var xData = new XData(pline, Ext.RegisteredAppName);
            Dictionary<string, object> data = xData.GetAppData();
            if (data.Count < 1) return null;
            return Convert.ToDouble(data[EXTRA_PCT]);
        }

        public static void SetSpanData(this Polyline3d pline, Solid3d pole1, Solid3d pole2, double extraWirePct)
        {
            Dictionary<string, object> data = new Dictionary<string, object>(3);
            data.Add(POLE1, pole1.Handle.ToString());
            data.Add(POLE2, pole2.Handle.ToString());
            data.Add(EXTRA_PCT, extraWirePct);
            var xData = new XData(pline, Ext.RegisteredAppName);
            xData.SetAppData(data);
        }

        public static void SetExtraWirePct(this Polyline3d pline, double extraWirePct)
        {
            var xData = new XData(pline, Ext.RegisteredAppName);
            Dictionary<string, object> data = xData.GetAppData();
            data[EXTRA_PCT] = extraWirePct;
            xData.SetAppData(data);
        }
    }
}
