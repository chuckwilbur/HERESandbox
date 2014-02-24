using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace PoleSagTool.EED
{
    public static class PoleExt
    {
        const string SPANS = "SPANS";

        public static Point3d GetPoleTop(this Solid3d pole)
        {
            Point3d max = pole.GeometricExtents.MaxPoint;
            Point3d min = pole.GeometricExtents.MinPoint;
            Point3d ptTop = new Point3d(
                (max.X + min.X) / 2,
                (max.Y + min.Y) / 2,
                Math.Max(max.Z, min.Z));
            return ptTop;
        }

        public static List<Handle> GetSpans(this Solid3d pole)
        {
            var xData = new XData(pole, Ext.RegisteredAppName);
            Dictionary<string, object> data = xData.GetAppData();
            var result = new List<Handle>();
            if (data.Count > 0)
            {
                string handleList = data[SPANS].ToString();
                string[] handleStrings = handleList.Split(',');
                result.AddRange(handleStrings.Select<string, Handle>(
                    h => new Handle(Int64.Parse(h, NumberStyles.HexNumber))));
            }
            return result;
        }

        public static void SetSpanData(this Solid3d pole, Polyline3d span)
        {
            List<Handle> spans = pole.GetSpans();
            if (spans.Contains(span.Handle)) return;

            spans.Add(span.Handle);
            string handleList = string.Join(",", spans.ToArray());
            Dictionary<string, object> data = new Dictionary<string, object>(3);
            data.Add("SPANS", handleList);
            var xData = new XData(pole, Ext.RegisteredAppName);
            xData.SetAppData(data);
        }
    }
}
