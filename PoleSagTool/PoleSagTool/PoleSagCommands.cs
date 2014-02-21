using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.Runtime;

namespace PoleSagTool
{
    public class PoleSagCommands
    {
        [CommandMethod("CPole")]
        public void CreatePole()
        {
            AcadApp.Ed.WriteMessage("Called Create Pole");
        }

        [CommandMethod("CSpan")]
        public void CreateSpan()
        {
            AcadApp.Ed.WriteMessage("Called Create Span");
        }
    }
}
