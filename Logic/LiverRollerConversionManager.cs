using AdvansysPOC.Helpers;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
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
            //// Get parameter values
            bool isLeftHand = false;
            Parameter ConveyorHand = instance.LookupParameter(Constants.ConveyorHand);
            if (ConveyorHand != null)
            {
                isLeftHand = ConveyorHand.AsString().ToLower() == "left";
            }
            //Parameter widthParam = instance.LookupParameter("Conveyor Width");
            //Parameter zoneLengthParam = instance.LookupParameter("Zone Length");
            //Parameter typeParam = instance.LookupParameter("Conveyor Type");

            //if (lengthParam != null && typeParam != null)
            //{
            //    // Read all needed parameters values
            //    double conveyorLength = lengthParam.AsDouble();
            //    double conveyorWidth = widthParam.AsDouble();
            //    double zoneLength = zoneLengthParam.AsDouble();
            //    string conveyorType = typeParam.AsValueString();

            // Get geometry information
            XYZ startPoint, endPoint;
            Line cl = (instance.Location as LocationCurve).Curve as Line;
            //Line cl = instance.getFamilyCL();
            startPoint = cl.GetEndPoint(0);
            endPoint = cl.GetEndPoint(1);

            //Getting bed types to be placed...
            FamilySymbol entryBed, exitBed, ctfBed, withBrakeBed, repetitiveBed;

            //Placing beds in order...
            List<DetailedBed> bedsTobeInserted = GetBedsLogic(startPoint, endPoint, 3);
            List<FamilyInstance> placedBeds = new List<FamilyInstance>();
            foreach (var bed in bedsTobeInserted)
            {
                FamilyInstance inst = bed.PlaceBed();
                placedBeds.Add(inst);
                if (bed.HasDrive)
                {
                    placedBeds.Add(bed.PlaceDrive(isLeftHand));
                }
                placedBeds.AddRange(bed.PlaceSupports(inst));
            }

            //Grouping beds into an assembly...
            if (placedBeds.Count > 0)
            {
                ElementId categoryId = placedBeds[0].Category.Id;
                AssemblyInstance assemblyInstance = AssemblyInstance.Create(Globals.Doc, placedBeds.Select(s => s.Id).ToList(), categoryId);
            }

            //Rotate if needed

            //Delete generic family
            Globals.Doc.Delete(instance.Id);

            return null;
        }


        public List<DetailedBed> GetBedsLogic(XYZ startpoint, XYZ endpoint, int zoneLength)
        {
            List<DetailedBed> outBeds = new List<DetailedBed>();

            double entryExitLength = zoneLength + 0.5;

            XYZ direction = (endpoint - startpoint).Normalize();

            // Rules and formulas
            int oal = (int)Math.Round(startpoint.DistanceTo(endpoint));

            //Entry...
            DetailedBed entryBed = new DetailedBed();
            entryBed.BedType = BedType.EntryBed;
            entryBed.StartPoint = startpoint;
            entryBed.Length = entryExitLength;
            entryBed.Direction = direction;

            //Exit...
            DetailedBed exitBed = new DetailedBed();
            exitBed.BedType = BedType.ExitBed;
            exitBed.Length = entryExitLength;
            exitBed.StartPoint = endpoint - entryExitLength * direction;
            exitBed.Direction = direction;

            //Brake...
            DetailedBed brakeBed = new DetailedBed();
            brakeBed.Length = 12;
            brakeBed.BedType = BedType.Brake;
            brakeBed.StartPoint = exitBed.StartPoint - (brakeBed.Length - 1) * direction;
            brakeBed.Direction = direction;

            outBeds.Add(entryBed);
            outBeds.Add(brakeBed);
            outBeds.Add(exitBed);

            // Remaining length after deducting enter, exit, brake...
            int remainingLength = oal - (int) (entryBed.Length + exitBed.Length) - (int) brakeBed.Length;

            if (remainingLength / 12 == 0)
            {
                // Add the drive to brake bed...
                brakeBed.HasDrive = true;
                
                // Add CTF C351...
                int ctfLen = remainingLength % 12;
                DetailedBed ctf351 = new DetailedBed();
                ctf351.Length = ctfLen;
                ctf351.BedType = BedType.C351CTF;
                ctf351.StartPoint = entryBed.GetEndPoint();
                ctf351.Direction = direction;

                remainingLength -= (int)ctf351.Length;

                outBeds.Add(ctf351);

            }
            else // Here we have at least one full intermediate bed, unless the case of two CTFs...
            {
                int full352Count = remainingLength / 12;
                XYZ lastPoint = new XYZ();
                if (remainingLength % 12 == 1 && full352Count > 0)
                {
                    //This is the case when we need CTF C352 with length 6 ft beside 7 ft C351...
                    DetailedBed ctf351 = new DetailedBed();
                    ctf351.Length = 7;
                    ctf351.BedType = BedType.C351CTF;
                    ctf351.StartPoint = entryBed.GetEndPoint();
                    ctf351.Direction = direction;


                    DetailedBed ctf352 = new DetailedBed();
                    ctf352.Length = 6;
                    ctf352.BedType = BedType.C352;
                    ctf352.StartPoint = ctf351.GetEndPoint();
                    ctf352.Direction = direction;


                    remainingLength -= 13;

                    outBeds.Add(ctf351);
                    outBeds.Add(ctf352);

                    lastPoint = ctf352.GetEndPoint();

                    // Don't forget to deduct the full 352 by 1...
                    full352Count--;
                }
                else // Here we have at least one full bed and a ctf 351 normally...
                {
                    int ctfLen = remainingLength % 12;
                    if (ctfLen != 0)
                    {
                        DetailedBed ctf351 = new DetailedBed();
                        ctf351.Length = ctfLen;
                        ctf351.BedType = BedType.C351CTF;
                        ctf351.StartPoint = entryBed.GetEndPoint();
                        ctf351.Direction = direction;


                        remainingLength -= (int)ctf351.Length;

                        outBeds.Add(ctf351);

                        lastPoint = ctf351.GetEndPoint();
                    }
                    else
                    {
                        lastPoint = entryBed.GetEndPoint();
                    }
                }

                // Now adding the intermediate beds using a loop...
                
                for (int i = 0; i<full352Count && remainingLength > 0; i++)
                {
                    DetailedBed ctf352 = new DetailedBed();
                    ctf352.Length = 12;
                    ctf352.BedType = BedType.C352;
                    ctf352.StartPoint = lastPoint;
                    ctf352.Direction = direction;

                    if (i == 0) ctf352.HasDrive = true;

                    outBeds.Add(ctf352);

                    lastPoint = ctf352.GetEndPoint();
                }
            }
            return outBeds;
        }


        /*
        public FamilyInstance PlaceEntrance(XYZ startPoint, XYZ vector, double length)
        {
            // XYZ vector = endpoint.substract(startpoint);

            string error = "";
            FamilySymbol symbol = FamilyHelper.getFamilySymbol(Constants.EntranceBedFamilyName, Constants.EntranceBedFileName, null, ref error);
            return FamilyHelper.placePointFamilyWithSubTransaction(symbol, startPoint, length);
        }
        */

        public FamilyInstance PlaceEntrance(XYZ startPoint, XYZ endPoint, double length)
        {
            string error = "";
            FamilySymbol symbol = FamilyHelper.getFamilySymbol(Constants.EntranceBedFamilyName, Constants.EntranceBedFileName, null, ref error);
            return FamilyHelper.placePointFamilyWithSubTransaction(symbol, startPoint, length);
        }
        public FamilyInstance PlaceExit(XYZ startPoint, XYZ endPoint, double length)
        {
            string error = "";
            FamilySymbol symbol = FamilyHelper.getFamilySymbol(Constants.ExitBedFamilyName, Constants.ExitBedFileName, null, ref error);
            return FamilyHelper.placePointFamilyWithSubTransaction(symbol, startPoint, length);
        }
        public FamilyInstance PlaceCutToFit(XYZ startPoint, XYZ endPoint, double length)
        {
            string error = "";
            FamilySymbol symbol = FamilyHelper.getFamilySymbol(Constants.CTFFamilyName, Constants.CTFFamilyFileName, null, ref error);
            return FamilyHelper.placePointFamilyWithSubTransaction(symbol, startPoint, length);
        }
        public FamilyInstance PlaceDrive(XYZ startPoint, XYZ endPoint, double length)
        {
            string error = "";
            FamilySymbol symbol = FamilyHelper.getFamilySymbol(Constants.DriveFamilyName, Constants.DriveFamilyFileName, null, ref error);
            return FamilyHelper.placePointFamilyWithSubTransaction(symbol, startPoint, length);
        }
        public FamilyInstance PlaceIntermediateBed(XYZ startPoint, XYZ endPoint, double length)
        {
            string error = "";
            FamilySymbol symbol = FamilyHelper.getFamilySymbol(Constants.IntermediateFamilyName, Constants.IntermediateFamilyFileName, null, ref error);
            return FamilyHelper.placePointFamilyWithSubTransaction(symbol, startPoint, length);
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
