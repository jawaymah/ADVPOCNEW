using AdvansysPOC.Helpers;
using AdvansysPOC.Logic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Transactions;
using System.Windows.Controls;
using static AdvansysPOC.Logic.CalculationsManager;
using static Autodesk.Revit.DB.SpecTypeId;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

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
            if (selection != null)
            {
                var ids = selection.GetElementIds();
                foreach (var id in ids)
                {
                    Element e = Doc.GetElement(id);
                    if (e != null && e is FamilyInstance)
                    {
                        //if ((e as FamilyInstance).Symbol.FamilyName == "Straight")
                        genericFamilies.Add(e as FamilyInstance);
                    }
                }
            }
            if (genericFamilies.Count == 0)
            {
                message = "There are no Generic family selected";
                return Result.Failed;
            }
            LiverRollerConversionManager managner = new LiverRollerConversionManager();

            List<FamilyInstance> detailed = new List<FamilyInstance>();
            string allmessage = "Conversion Report: " + System.Environment.NewLine;
            using (TransactionGroup group = new TransactionGroup(Doc))
            {
                group.Start("Converting to Detail");
                foreach (var family in genericFamilies)
                {
                    Line cl = (family.Location as LocationCurve).Curve as Line;
                    XYZ startPoint = cl.GetEndPoint(0);
                    XYZ endPoint = cl.GetEndPoint(1);
                    double oal = System.Math.Round(startPoint.DistanceTo(endPoint), 4);

                    Parameter zoneLength = family.LookupParameter("Zone_Length");
                    double zl = zoneLength.AsDouble();
                    int zone = (int)zl;
                    Parameter unitId = family.LookupParameter(Constants.ConveyorNumber);
                    string uId = unitId.AsValueString();

                    if (oal > 200)
                    {
                        allmessage = addReportItem(allmessage, $"unit {uId}: Failed! -Length exceeds 200ft");
                        continue;
                    }
                    else if (zone == 2 && oal < 17)
                    {
                        allmessage = addReportItem(allmessage, $"unit {uId}: Failed! -Length is less than 17ft");
                        continue;
                    }
                    else if (zone == 3 && oal < 19)
                    {
                        allmessage = addReportItem(allmessage, $"unit {uId}: Failed! -Length is less than 19ft");
                        continue;
                    }
                    else
                    {
                        using (Autodesk.Revit.DB.Transaction tr = new Autodesk.Revit.DB.Transaction(Doc))
                        {
                            tr.Start("Create Beds");
                            FailureHandlingOptions options = tr.GetFailureHandlingOptions();
                            WarningDiscard preproccessor = new WarningDiscard();
                            options.SetFailuresPreprocessor(preproccessor);
                            tr.SetFailureHandlingOptions(options);

                            detailed = managner.ConvertToDetailed(family, uId);
                            Doc.Regenerate();

                            tr.Commit();
                        }
                        using (Autodesk.Revit.DB.Transaction tr = new Autodesk.Revit.DB.Transaction(Doc))
                        {
                            tr.Start("add assemblies");
                            addElementsToaSSEMBLY(detailed.Select(s => s.Id).ToList(), Convert.ToInt32(uId));
                            tr.Commit();
                        }
                        allmessage = addReportItem(allmessage, $"unit {uId}: Succeded!");
                    }

                }
                group.Commit();
            }
            Autodesk.Revit.UI.TaskDialog.Show("Conversion Report", allmessage);
            return Result.Succeeded;
        }
        public string addReportItem(string allMessage, string message)
        {
            allMessage += System.Environment.NewLine;
            allMessage += message;
            return allMessage;
        }
        public void addElementsToaSSEMBLY(List<ElementId> elementIds, int unitId)
        {
            if (elementIds.Count > 0)
            {
                ElementId categoryId = Globals.Doc.GetElement(elementIds[0]).Category.Id;
                AssemblyInstance assemblyInstance = AssemblyInstance.Create(Globals.Doc, elementIds, categoryId);

                double length = 0;
                double rollerCenter = 0;
                var memeberIds = assemblyInstance.GetMemberIds();
                double driveSpeed = 0;
                    assemblyInstance.SetUnitId(unitId);
                int conveyorNumber = assemblyInstance.LookupParameter(Constants.ConveyorNumber).AsInteger();
                foreach (var memeberId in memeberIds)
                {
                    Element e = Globals.Doc.GetElement(memeberId);
                    if (e != null && e is FamilyInstance bed)
                    {
                        if (bed.Symbol.FamilyName != Constants.GenericFamilyName && bed.LookupParameter(Constants.Bed_Length) is Parameter lengthParameter)
                        {
                            length += lengthParameter.AsDouble();
                            rollerCenter = bed.LookupParameter(Constants.Roller_CenterToCenter).AsDouble() * 12;
                        }
                        if (bed.Symbol.FamilyName != Constants.GenericFamilyName && bed.LookupParameter(Constants.DriveBed_Speed) is Parameter speedParameter)
                        {
                            driveSpeed = speedParameter.AsDouble();
                        }
                    }
                    LiveRollerCalculationInputs input = new LiveRollerCalculationInputs { ConveyorNumber = conveyorNumber, Length = length, RollerCenters = rollerCenter };
                    if (driveSpeed > 0) input.Speed = driveSpeed;
                    LiveRollerCalculationResult res = CalculationsManager.GetLiveRollerCalculationResult(input);
                    assemblyInstance.SetParameter(Constants.HP, (int)res.HP);
                    assemblyInstance.SetParameter(Constants.Center_Drive, res.DriveSize);
                }

                //// Create the assembly instance
                //AssemblyInstance assemblyInstance = AssemblyInstance.Create(Globals.Doc, elementIds, getSpoolNamingCategory());

                //if (assemblyInstance != null)
                //{
                //    // Optionally, you can set properties of the assembly instance
                //    assemblyInstance.AssemblyTypeName = "Custom Assembly Type"; // Example of setting assembly type
                //}
            }
        }

        public static ElementId getSpoolNamingCategory()
        {
            ElementId pID = ElementId.InvalidElementId;
            //var lSpools = new FilteredElementCollector(Globals.Doc).OfClass(typeof(AssemblyInstance)).OfType<AssemblyInstance>().ToList();
            //foreach (var pSpool in lSpools)
            //{
            //    if (pSpool.NamingCategoryId != ElementId.InvalidElementId)
            //    {
            //        pID = pSpool.NamingCategoryId;
            //        break;
            //    }
            //}
            return pID;
        }
    }
}
