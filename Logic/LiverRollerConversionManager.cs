using AdvansysPOC.Helpers;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;

namespace AdvansysPOC.Logic
{
    public class LiverRollerConversionManager : ConversionManager
    {

        public new List<FamilyInstance> ConvertToDetailed(FamilyInstance instance)
        {
            // Get parameter values
            Parameter lengthParam = instance.LookupParameter("Conveyor Length");
            Parameter widthParam = instance.LookupParameter("Conveyor Width");
            Parameter zoneLengthParam = instance.LookupParameter("Zone Length");
            Parameter typeParam = instance.LookupParameter("Conveyor Type");

            if (lengthParam != null && typeParam != null)
            {
                // Read all needed parameters values
                double conveyorLength = lengthParam.AsDouble();
                double conveyorWidth = widthParam.AsDouble();
                double zoneLength = zoneLengthParam.AsDouble();
                string conveyorType = typeParam.AsValueString();

                // Get geometry information
                XYZ startPoint, endPoint;
                Line cl = instance.getFamilyCL();
                startPoint = cl.GetEndPoint(0);
                endPoint = cl.GetEndPoint(1);

                //Getting bed types to be placed...
                FamilySymbol entryBed, exitBed, ctfBed, withBrakeBed, repetitiveBed;


                //Placing beds in order...


                //Grouping beds into an assembly...


            }
            return null;
        }

        public FamilyInstance PlaceEntrance(XYZ startPoint, XYZ endPoint)
        {
            string error = "";
            FamilySymbol symbol = FamilyHelper.getFamilySymbol(Constants.EntranceBedFamilyName, Constants.EntranceBedFileName, null, ref error);
            return FamilyHelper.placePointFamilyWithSubTransaction(symbol, startPoint);
        }
        public FamilyInstance PlaceExit(XYZ startPoint, XYZ endPoint)
        {
            string error = "";
            FamilySymbol symbol = FamilyHelper.getFamilySymbol(Constants.ExitBedFamilyName, Constants.ExitBedFileName, null, ref error);
            return FamilyHelper.placePointFamilyWithSubTransaction(symbol, startPoint);
        }
        public FamilyInstance PlaceCutToFit(XYZ startPoint, XYZ endPoint)
        {
            string error = "";
            FamilySymbol symbol = FamilyHelper.getFamilySymbol(Constants.CTFFamilyName, Constants.CTFFamilyFileName, null, ref error);
            return FamilyHelper.placePointFamilyWithSubTransaction(symbol, startPoint);
        }
        public FamilyInstance PlaceDrive(XYZ startPoint, XYZ endPoint)
        {
            string error = "";
            FamilySymbol symbol = FamilyHelper.getFamilySymbol(Constants.DriveFamilyName, Constants.DriveFamilyFileName, null, ref error);
            return FamilyHelper.placePointFamilyWithSubTransaction(symbol, startPoint);
        }
        public FamilyInstance PlaceIntermediateBed(XYZ startPoint, XYZ endPoint)
        {
            string error = "";
            FamilySymbol symbol = FamilyHelper.getFamilySymbol(Constants.IntermediateFamilyName, Constants.IntermediateFamilyFileName, null, ref error);
            return FamilyHelper.placePointFamilyWithSubTransaction(symbol, startPoint);
        }
        public List<FamilyInstance> PlaceSupports(List<FamilyInstance> beds)
        {
            List<DetailedSupport> supports = new List<DetailedSupport>();
            foreach (var item in beds)
            {
                XYZ loc = ((item.Location) as LocationPoint).Point;
                DetailedSupport support = new DetailedSupport(item, loc);
                support.setBelowFloor();
                supports.Add(support);
            }
            return null;
        }
        public new FamilyInstance ConvertBackToGeneric(List<FamilyInstance> detailedFamilies)
        {
            return null;
        }
    }
}
