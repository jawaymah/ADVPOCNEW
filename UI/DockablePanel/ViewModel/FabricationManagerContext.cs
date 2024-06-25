using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace AdvansysPOC
{
    /// <summary>
    /// Fabrication Manager Context,
    /// </summary>
    public static class FabricationManagerContext
    {
        /// <summary>
        /// Current Document.
        /// </summary>
        public static Document Document { get; set; }

        /// <summary>
        /// Current UI Document.
        /// </summary>
        public static UIDocument UIDocument { get; set; }

        /// <summary>
        /// Current Activated View
        /// </summary>
        public static View CurrentActivatedView { get; internal set; }

        /// <summary>
        /// Fabrication Manager View in Dockable Panel.
        /// </summary>
        public static FabricationManagerView FabricationManagerView { get; set; }

        /// <summary>
        /// Gets DataContext of FabricationManagerView.
        /// </summary>
        public static FabricationManagerViewModel FabricationManagerViewModel
        {
            get
            {
                return FabricationManagerView.DataContext as FabricationManagerViewModel;
            }
        }

        /// <summary>
        /// Document First Run
        /// </summary>
        public static bool DocumentFirstRun { get; set; } = true;
    }
}
