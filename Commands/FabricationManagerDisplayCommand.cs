using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace AdvansysPOC.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class FabricationManagerDisplayCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var UiDoc = commandData.Application.ActiveUIDocument;
            var Doc = UiDoc.Document;
            // -- Display or Hide Fabrication Manager
            FabricationManagerDisplayCore.DisplayORHide(Doc, UiDoc);
            return Result.Succeeded;
        }
    }
}
