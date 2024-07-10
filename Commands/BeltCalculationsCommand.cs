using AdvansysPOC.Helpers;
using AdvansysPOC.Logic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AdvansysPOC.Logic.BeltCalculationsManager;
using static AdvansysPOC.Logic.CalculationsManager;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

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

            List<AssemblyInstance> detailedUnits = new List<AssemblyInstance>();
            List<FamilyInstance> detailedBeds = new List<FamilyInstance>();
            if (selection != null)
            {
                var ids = selection.GetElementIds();
                foreach (var id in ids)
                {
                    Element e = Doc.GetElement(id);
                    if (e != null && e is AssemblyInstance assembly)
                    {
                        if (assembly.AssemblyTypeName != Constants.GenericFamilyName)
                            detailedUnits.Add(assembly);
                    }
                }
            }
            if (detailedUnits.Count == 0)
            {
                message = "There are no Detailed family selected";
                return Result.Failed;
            }

            List<LiveRollerCalculationInputs> inputs = new List<LiveRollerCalculationInputs>();
            List<int> failedUnitIds = new List<int>();
            for (int i = 0; i < detailedUnits.Count; i++)
            {
                double length = 0;
                double rollerCenter = 0;
                var memeberIds = detailedUnits[i].GetMemberIds();
                int conveyorNumber = detailedUnits[i].LookupParameter(Constants.ConveyorNumber).AsInteger();
                double driveSpeed = 0;
                foreach (var memeberId in memeberIds)
                {
                    Element e = Doc.GetElement(memeberId);
                    string name = (e as FamilyInstance).Symbol.FamilyName;
                    if (e != null && e is FamilyInstance bed)
                    {
                        if (bed.Symbol.FamilyName != Constants.GenericFamilyName && bed.LookupParameter(Constants.Bed_Length) is Parameter lengthParameter)
                        {
                            detailedBeds.Add(bed);
                            length += lengthParameter.AsDouble();
                            rollerCenter = bed.LookupParameter(Constants.Roller_CenterToCenter).AsDouble() * 12;
                        }
                        else if (bed.Symbol.FamilyName != Constants.GenericFamilyName && bed.LookupParameter(Constants.Drive_Speed) is Parameter speedParameter)
                        {
                            driveSpeed = speedParameter.AsDouble();
                        }
                    }
                }
                LiveRollerCalculationInputs input = new LiveRollerCalculationInputs { ConveyorNumber = conveyorNumber, Length = length, RollerCenters = rollerCenter };
                if (driveSpeed > 0) input.Speed = driveSpeed;
                LiveRollerCalculationResult res = CalculationsManager.GetLiveRollerCalculationResult(input);
                if (res.HP == 0)
                {
                    failedUnitIds.Add(conveyorNumber);
                }
                else
                {
                    using (Autodesk.Revit.DB.Transaction tr = new Autodesk.Revit.DB.Transaction(Doc))
                    {
                        tr.Start("add assemblies");
                        detailedUnits[i].SetParameter(Constants.HP, (int)res.HP);
                        detailedUnits[i].SetParameter(Constants.Center_Drive, res.DriveSize);
                        tr.Commit();
                    }
                }
            }
            if (failedUnitIds.Count > 0)
            {
                message = $"Couldn't calculate HP for units ({string.Join(',', failedUnitIds)}).\nPlease reconfigure the conveyor parameters to be able to calculate HP";
                return Result.Failed;
            }
            List<BeltCalculationInputs> beltInputs = new List<BeltCalculationInputs>();
            for (int i = 0; i < detailedUnits.Count; i++)
            {
                double interBedsLength = 0;
                double bedWidth = 0;
                int TE18Qty = 0, TE30Qty = 0, TE42QtY = 0;
                int cd6Qty = 0, cd8Qty = 0, cd10Qty = 0;
                var memeberIds = detailedUnits[i].GetMemberIds();
                int conveyorNumber = detailedUnits[i].LookupParameter(Constants.ConveyorNumber).AsInteger();
                string centerDrive = detailedUnits[i].LookupParameter(Constants.Center_Drive).AsString();
                switch (centerDrive)
                {
                    case "6CD":
                        cd6Qty++;
                        break;
                    case "8CD":
                        cd8Qty++;
                        break;
                    case "10CD":
                        cd10Qty++;
                        break;
                }
                foreach (var memeberId in memeberIds)
                {
                    Element e = Doc.GetElement(memeberId);
                    if (e != null && e is FamilyInstance bed)
                    {
                        if (bed.Symbol.FamilyName != Constants.GenericFamilyName && bed.LookupParameter(Constants.Bed_Length) is Parameter lengthParameter)
                        {
                            if (bed.LookupParameter(Constants.Bed_Width) is Parameter widthParameter)
                            {
                                bedWidth = widthParameter.AsDouble();
                            }
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
                            detailedBeds.Add(bed);
                        }
                    }
                }
                beltInputs.Add(new BeltCalculationsManager.BeltCalculationInputs
                {
                    ConveyorNumber = conveyorNumber,
                    BedWidth = bedWidth,
                    TE18BedQuantity = TE18Qty,
                    TE30BedQuantity = TE30Qty,
                    TE42BedQuantity = TE42QtY,
                    CD6Quantity = cd6Qty,
                    CD8Quantity = cd8Qty,
                    CD10Quantity = cd10Qty,
                    InterBedsLength = interBedsLength
                });
            }
            if (!BeltCalculationsManager.DisplayBeltCalculation(beltInputs))
            {
                message = "Something went wrong";
                return Result.Failed;
            }
            return Result.Succeeded;
        }


    }
}
