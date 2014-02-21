using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AcadApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace PoleSagTool
{
    public class AcadApp
    {
        public static Autodesk.AutoCAD.EditorInput.Editor Ed
        {
            get { return AcadApplication.DocumentManager.MdiActiveDocument.Editor; }
        }
    }
}
