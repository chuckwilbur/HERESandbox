using System;
using System.Collections.Generic;
using System.Text;

using Autodesk.AutoCAD.Runtime;

namespace PoleSagTool
{
    /// <summary>
    /// Entry point class of this assembly.
    /// </summary>
    public class AppEntry : IExtensionApplication
    {
        #region IExtensionApplication Members

        /// <summary>
        /// Entry point of this assembly.
        /// </summary>
        void IExtensionApplication.Initialize()
        {
            AcadApp.Ed.WriteMessage("\nPole Sag Tool is loaded...\n");
        }

        /// <summary>
        /// .Net assembly can't be unloaded from AutoCAD like ARX libraries.
        /// So this method wouldn't be invoked until AutoCAD exits.
        /// You're not encouraged to do anything in this method.
        /// </summary>
        void IExtensionApplication.Terminate() { }

        #endregion
    }
}
