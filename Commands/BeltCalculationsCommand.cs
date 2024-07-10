using AdvansysPOC.Logic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AdvansysPOC.Logic.CalculationsManager;

namespace AdvansysPOC.Commands
{
    [Transaction(TransactionMode.Manual)]
    internal class BeltCalculationsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var UiDoc = commandData.Application.ActiveUIDocument;
            var Doc = UiDoc.Document;
            var selection = UiDoc.Selection;
            if (selection == null)
            {
                message = "There are no selected detailed family";
                return Result.Failed;
            }

            List<AssemblyInstance> detailedAssemblies = new List<AssemblyInstance>();
            List<FamilyInstance> detailedFamilies = new List<FamilyInstance>();
            if (selection != null)
            {
                var ids = selection.GetElementIds();
                foreach (var id in ids)
                {
                    Element e = Doc.GetElement(id);
                    if (e != null && e is AssemblyInstance assembly)
                    {
                        if (assembly.AssemblyTypeName != Constants.GenericFamilyName)
                            detailedAssemblies.Add(assembly);
                    }
                }
            }
            if (detailedAssemblies.Count == 0)
            {
                message = "There are no Detailed family selected";
                return Result.Failed;
            }
            List<LiveRollerCalculationInputs> inputs = new List<LiveRollerCalculationInputs>();
            for (int i = 0; i < detailedAssemblies.Count; i++)
            {
                double interBedsLength = 0;
                double bedWidth = 0;
                double rollerCenter = 0;
                double TE18Qty = 0, TE30Qty = 0, TE42QtY = 0;
                var memeberIds = detailedAssemblies[i].GetMemberIds();
                int conveyorNumber = detailedAssemblies[i].LookupParameter(Constants.Roller_CenterToCenter).AsInteger();
                foreach (var memeberId in memeberIds)
                {
                    Element e = Doc.GetElement(memeberId);
                    if (e != null && e is FamilyInstance bed)
                    {
                        if (bed.Symbol.FamilyName != Constants.GenericFamilyName && bed.LookupParameter(Constants.Bed_Length) is Parameter lengthParameter)
                        {
                            bedWidth = bed.LookupParameter(Constants.Bed_Width).AsDouble();
                            double bedLength = lengthParameter.AsDouble();
                            if (bed.Symbol.FamilyName == Constants.EntranceBedFamilyName || bed.Symbol.FamilyName == Constants.ExitBedFamilyName)
                            {
                                switch (bedLength)
                                {
                                    case 18:
                                        TE18Qty++;
                                        break;
                                    case 30:
                                        TE30Qty++;
                                        break;
                                    case 42:
                                        TE42QtY++;
                                        break;
                                }
                            }
                            else
                            {
                                interBedsLength += bedLength;
                            }
                            detailedFamilies.Add(bed);
                        }
                    }
                }
                inputs.Add(new BeltCalculationsManager.BeltCalculationInputs
                {
                    ConveyorNumber = conveyorNumber,
                    BedWidth = bedWidth,
                    RollerCenters = rollerCenter
                });
            }
            if (!CalculationsManager.DisplayLiveRollerCalculation(inputs))
            {
                message = "Something went wrong";
                return Result.Failed;
            }
            return Result.Succeeded;
        }


    }
}
