using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

namespace PoleSagTool
{
    public static class SpanExt
    {
        static string RegisteredAppName { get { return "HERE_PoleSag"; } }

        public static void SetSpanDataFilter(this Overrule overrule)
        {
            overrule.SetXDataFilter(RegisteredAppName);
        }

        public static bool HasPole(this Polyline3d pline, string poleHandle)
        {
            var xData = new XData(pline, RegisteredAppName);
            Dictionary<string, object> data = xData.GetAppData();
            if (data.Count < 1) return false;
            if (data["POLE1"].ToString() == poleHandle) return true;
            if (data["POLE2"].ToString() == poleHandle) return true;
            return false;
        }

        public static double? GetExtraWirePct(this Polyline3d pline)
        {
            var xData = new XData(pline, RegisteredAppName);
            Dictionary<string, object> data = xData.GetAppData();
            if (data.Count < 1) return null;
            return Convert.ToDouble(data["EXTRA_PCT"]);
        }

        public static void SetSpanData(this Polyline3d pline, Solid3d pole1, Solid3d pole2, double extraWirePct)
        {
            Dictionary<string, object> data = new Dictionary<string, object>(3);
            data.Add("POLE1", pole1.Handle.ToString());
            data.Add("POLE2", pole2.Handle.ToString());
            data.Add("EXTRA_PCT", extraWirePct);
            var xData = new XData(pline, RegisteredAppName);
            xData.SetAppData(data);
        }

        public static void SetExtraWirePct(this Polyline3d pline, double extraWirePct)
        {
            var xData = new XData(pline, RegisteredAppName);
            Dictionary<string, object> data = xData.GetAppData();
            data["EXTRA_PCT"] = extraWirePct;
            xData.SetAppData(data);
        }
    }
}
