using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.Runtime;

namespace PoleSagTool.EED
{
    public static class Ext
    {
        internal static string RegisteredAppName { get { return "HERE_PoleSag"; } }

        public static void SetPoleSagDataFilter(this Overrule overrule)
        {
            overrule.SetXDataFilter(RegisteredAppName);
        }
    }
}
