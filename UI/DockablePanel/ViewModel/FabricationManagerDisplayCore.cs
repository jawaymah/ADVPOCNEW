using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace AdvansysPOC
{
    /// <summary>
    /// Core function of FabricationManagerDisplayCommand
    /// </summary>
    public static class FabricationManagerDisplayCore
    {
        /// <summary>
        /// Display or Hide Fabrication Manager
        /// </summary>
        public static void DisplayORHide(Document doc, UIDocument uiDoc)
        {
            // -- Get Pane from Application
            DockablePane dockablePane = App.GetFabricationManagerPane();
            if (dockablePane == null)
            {
                return;
            }

            // -- Show or Hide
            if (dockablePane.IsShown())
            {
                dockablePane.Hide();
            }
            else
            {
                FabricationManagerContext.Document = doc;
                FabricationManagerContext.UIDocument = uiDoc;
                dockablePane.Show();
            }
        }
    }
}
