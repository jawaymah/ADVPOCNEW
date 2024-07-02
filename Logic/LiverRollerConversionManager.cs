using AdvansysPOC.Helpers;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

                using (Transaction tr = new Transaction(Globals.Doc))
                {
                    tr.Start("Create detailed Unit");

                    //Placing beds in order...
                    List<DetailedBed> bedsTobeInserted = GetBedsLogic(startPoint, endPoint, 3);
                    List<FamilyInstance> placedBeds = new List<FamilyInstance>();
                    foreach (var bed in bedsTobeInserted)
                    {
                        placedBeds.Add(bed.PlaceBed());
                    }
                    
                    //Grouping beds into an assembly...

                    //Rotate if needed

                    //Delete generic family
                    Globals.Doc.Delete(instance.Id);

                    tr.Commit();
                }


            }
            return null;
        }


        public List<DetailedBed> GetBedsLogic(XYZ startpoint, XYZ endpoint, int zoneLength)
        {
            List<DetailedBed> outBeds = new List<DetailedBed>();

            double entryExitLength = zoneLength + 0.5;

            XYZ direction = (endpoint - startpoint).Normalize();

            // Rules and formulas
            int oal = (int)startpoint.DistanceTo(endpoint);

            //Entry...
            DetailedBed entryBed = new DetailedBed();
            entryBed.BedType = BedType.EntryBed;
            entryBed.StartPoint = startpoint;
            entryBed.Length = entryExitLength;

            //Exit...
            DetailedBed exitBed = new DetailedBed();
            exitBed.BedType = BedType.ExitBed;
            exitBed.Length = entryExitLength;
            exitBed.StartPoint = endpoint - entryExitLength * direction;

            //Brake...
            DetailedBed brakeBed = new DetailedBed();
            brakeBed.Length = 12;
            brakeBed.BedType = BedType.Brake;
            brakeBed.StartPoint = exitBed.StartPoint - exitBed.Length * direction;

            outBeds.Add(entryBed);
            outBeds.Add(brakeBed);
            outBeds.Add(exitBed);

            int remainingLength = oal - (int) (entryBed.Length + exitBed.Length) - (int) brakeBed.Length;

            if (remainingLength / 12 == 0)
            {
                //Add the drive to brake bed...

                //Add CTF C351...
                int ctfLen = remainingLength % 12;
                DetailedBed ctf351 = new DetailedBed();
                ctf351.Length = 12;
                ctf351.BedType = BedType.C351CTF;
                ctf351.StartPoint = entryBed.GetEndPoint();

                outBeds.Add(ctf351);

            }
            else
            {
                int full352Count = remainingLength / 12;
                XYZ lastPoint;
                if (remainingLength % 12 == 1)
                {
                    //This is the case when we need CTF C352 with length 6 ft beside 7 ft C351...
                    DetailedBed ctf351 = new DetailedBed();
                    ctf351.Length = 7;
                    ctf351.BedType = BedType.C351CTF;
                    ctf351.StartPoint = entryBed.GetEndPoint();

                    DetailedBed ctf352 = new DetailedBed();
                    ctf352.Length = 6;
                    ctf352.BedType = BedType.C352;
                    ctf352.StartPoint = ctf351.GetEndPoint();

                    outBeds.Add(ctf351);
                    outBeds.Add(ctf352);

                    lastPoint = ctf352.GetEndPoint();

                    // Don't forget to deduct the full 352 by 1...
                    full352Count--;
                }
                else
                {
                    int ctfLen = remainingLength % 12;
                    DetailedBed ctf351 = new DetailedBed();
                    ctf351.Length = 12;
                    ctf351.BedType = BedType.C351CTF;
                    ctf351.StartPoint = entryBed.GetEndPoint();

                    outBeds.Add(ctf351);

                    lastPoint = ctf351.GetEndPoint();
                }

                // Now adding the intermediate beds using a loop...
                
                for (int i = 0; i<full352Count; i++)
                {
                    DetailedBed ctf352 = new DetailedBed();
                    ctf352.Length = 12;
                    ctf352.BedType = BedType.C352;
                    ctf352.StartPoint = lastPoint;

                    outBeds.Add(ctf352);

                    lastPoint = ctf352.GetEndPoint();
                }
            }
            return outBeds;
        }


        public FamilyInstance PlaceEntrance(XYZ startPoint, XYZ vector, double length)
        {
            // XYZ vector = endpoint.substract(startpoint);

            string error = "";
            FamilySymbol symbol = FamilyHelper.getFamilySymbol(Constants.EntranceBedFamilyName, Constants.EntranceBedFileName, null, ref error);
            return FamilyHelper.placePointFamilyWithSubTransaction(symbol, startPoint);
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
