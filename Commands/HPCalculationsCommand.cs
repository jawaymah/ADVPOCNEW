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
using static AdvansysPOC.Logic.CalculationsManager;

namespace AdvansysPOC.Commands
{
    [Transaction(TransactionMode.Manual)]
    internal class HPCalculationsCommand : IExternalCommand
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
            List<FamilyInstance> driveBeds = new List<FamilyInstance>();
            List<LiveRollerCalculationInputs> inputs = new List<LiveRollerCalculationInputs>();
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
                            driveBeds.Add(bed);
                        }
                    }
                }
                LiveRollerCalculationInputs input = new LiveRollerCalculationInputs { ConveyorNumber = conveyorNumber, Length = length, RollerCenters = rollerCenter };
                if (driveSpeed > 0) input.Speed = driveSpeed;
                inputs.Add(input);
            }
            Tuple<List<LiveRollerCalculationResult>, string> res = CalculationsManager.DisplayLiveRollerCalculation(inputs);
            if (!string.IsNullOrEmpty(res.Item2))
            {
                message = res.Item2;
                return Result.Failed;
            }
            using (Autodesk.Revit.DB.Transaction tr = new Autodesk.Revit.DB.Transaction(Doc))
            {
                tr.Start("HP calculations");
                for (int i = 0; i < detailedUnits.Count; i++)
                {
                    detailedUnits[i].SetParameter(Constants.HP, res.Item1[i].HP);
                    detailedUnits[i].SetParameter(Constants.Center_Drive, res.Item1[i].DriveSize);
                    detailedUnits[i].SetParameter(Constants.Conveyor_Speed, inputs[i].Speed.ToString());
                    driveBeds[i].SetParameter("CLR_MOTOR_HP", res.Item1[i].HP);
                    driveBeds[i].SetParameter("CLR_DRIVE_SIZE", res.Item1[i].DriveSizeInt);
                }
                tr.Commit();
            }
            return Result.Succeeded;
        }
    }
}
