using AdvansysPOC.Helpers;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections;
using System.Collections.Generic;
namespace AdvansysPOC
{
    [Transaction(TransactionMode.Manual)]
    public class EnvelopShowHideCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var UiDoc = commandData.Application.ActiveUIDocument;
            var Doc = UiDoc.Document;
            // -- Display or Hide Fabrication Manager
            var lSpools = new FilteredElementCollector(Globals.Doc).OfClass(typeof(AssemblyInstance)).ToElements();
            using (Transaction tr = new Transaction(Globals.Doc))
            {
                tr.Start("Flip envelop parameter state");
                foreach (AssemblyInstance unit in lSpools)
                {
                    //Parameter para = unit.LookupParameter(Constants.ConveyorNumber);
                    //if (para != null)
                    //{
                        foreach (var itemId in unit.GetMemberIds())
                        {
                        FamilyInstance inst= Globals.Doc.GetElement(itemId) as FamilyInstance;
                        string name = inst.Symbol.FamilyName;
                        List<string> parass = new List<string>();
                        foreach (Parameter parameter in inst.ParametersMap)
                        {
                            parass.Add(parameter.Definition.Name);
                        }
                            Parameter p = inst.LookupParameter(Constants.Conveyor_Envelop);
                            if (p != null)
                            {
                                if (p.AsInteger() == 0)
                                    p.Set(1);
                                else
                                    p.Set(0);
                            }
                        }
                    //}
                }
                tr.Commit();
            }

            return Result.Succeeded;
        }
    }
}

