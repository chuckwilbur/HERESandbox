﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AcadApplication = Autodesk.AutoCAD.ApplicationServices.Application;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace PoleSagTool
{
    public class AcadApp
    {
        public static Document CurDoc
        {
            get { return AcadApplication.DocumentManager.MdiActiveDocument; }
        }

        public static Database DB
        {
            get { return CurDoc.Database; }
        }

        public static Autodesk.AutoCAD.DatabaseServices.TransactionManager TM
        {
            get { return DB.TransactionManager; }
        }

        public static Editor Ed
        {
            get { return CurDoc.Editor; }
        }
    }
}
