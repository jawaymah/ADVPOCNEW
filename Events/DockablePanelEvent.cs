using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;

namespace AdvansysPOC.Events
{
    public class DockablePanelEvent : IExternalEventHandler
    {
        public void Execute(UIApplication app)
        {
            using (Transaction t = new Transaction(Globals.Doc, "do anything"))
            {
                t.Start();


                t.Commit();
            }
            Autodesk.Revit.UI.TaskDialog.Show("External Event", "Click Close to close.");
        }

        public string GetName()
        {
            return "External Event Example";
        }
    }
}

