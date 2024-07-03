using AdvansysPOC.Helpers;
using AdvansysPOC.Logic;
using AdvansysPOC.Logic.Sym3Export;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;

namespace AdvansysPOC
{
    [Transaction(TransactionMode.Manual)]
    public class Sym3ExportCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var UiDoc = commandData.Application.ActiveUIDocument;
            var Doc = UiDoc.Document;

            var selection = UiDoc.Selection;
            if (selection == null) return Result.Cancelled;

            List<FamilyInstance> genericFamilies = new List<FamilyInstance>();
            //GetAllUnits
            List<DetailedUnit> units = DocumentHelper.getAllUnits();
            Sym3Exporter.RunTestFile2();
            return Result.Succeeded;
        }
    }
}
