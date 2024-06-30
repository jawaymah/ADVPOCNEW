using AdvansysPOC.Helpers;
using AdvansysPOC.Logic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;

namespace AdvansysPOC
{
    [Transaction(TransactionMode.Manual)]
    public class ConvertToDetailCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var UiDoc = commandData.Application.ActiveUIDocument;
            var Doc = UiDoc.Document;

            var selection = UiDoc.Selection;
            if (selection == null) return Result.Cancelled;

            List<FamilyInstance> genericFamilies = new List<FamilyInstance>();
            if (selection != null) {
                var ids = selection.GetElementIds();
                foreach (var id in ids)
                {
                    Element e = Doc.GetElement(id);
                    if (e != null && e is FamilyInstance)
                    {
                        if ((e as FamilyInstance).Symbol.FamilyName == "Straight")
                            genericFamilies.Add(e as FamilyInstance);
                    }
                }
            }
            if (genericFamilies.Count == 0) {
                message = "There are no Generic family selected";
                return Result.Failed; 
            } 

            using (Transaction tr = new Transaction(Doc))
            {
                tr.Start("Converting to Detail");
                foreach (var family in genericFamilies)
                {
                    var detailed = ConversionManager.ConvertToDetail(family);
                }


                tr.Commit();
            }

            return Result.Succeeded;
        }
    }
}
