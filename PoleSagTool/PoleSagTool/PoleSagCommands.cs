using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.Runtime;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace PoleSagTool
{
    public class PoleSagCommands
    {
        static Autodesk.AutoCAD.EditorInput.Editor Ed
        {
            get { return AcadApp.DocumentManager.MdiActiveDocument.Editor; }
        }

        [CommandMethod("CP")]
        public void CreatePole()
        {
            Ed.WriteMessage("Called Create Pole");
        }

        [CommandMethod("CS")]
        public void CreateSpan()
        {
            Ed.WriteMessage("Called Create Span");
        }
    }
}
