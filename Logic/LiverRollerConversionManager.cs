using AdvansysPOC.Helpers;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
//using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using static Autodesk.Revit.DB.SpecTypeId;

namespace AdvansysPOC.Logic
{
    public class LiverRollerConversionManager : ConversionManager
    {

        public new List<FamilyInstance> ConvertToDetailed(FamilyInstance instance, string unitId)
        {
            string error = "";
            bool isLeftHand = false;
            Parameter ConveyorHand = instance.LookupParameter(Constants.ConveyorHand);
            if (ConveyorHand != null)
            {
                isLeftHand = ConveyorHand.AsString().ToLower() == "left";
            }


            // Get geometry information
            XYZ startPoint, endPoint;
            Line cl = (instance.Location as LocationCurve).Curve as Line;
            //Line cl = instance.getFamilyCL();
            startPoint = cl.GetEndPoint(0);
            endPoint = cl.GetEndPoint(1);
            double oal = Math.Round(startPoint.DistanceTo(endPoint), 4);
            //double oal = (int)(startPoint.DistanceTo(endPoint) * 12) / 12.0;



            int convWidth = (int)(12 * instance.Symbol.LookupParameter("Conveyor_OAW").AsDouble());
            double elevation = instance.LookupParameter(Constants.Conveyor_Elevation_In).AsDouble();
            //if (elevation != 2.5)
            //{
            //    startPoint += (elevation - 2.5) * XYZ.BasisZ;
            //    endPoint += (elevation - 2.5) * XYZ.BasisZ;
            //}

            bool driveInserted = false;

            //Getting bed types to be placed...
            FamilySymbol entryBed, exitBed, ctfBed, withBrakeBed, repetitiveBed;

            //using (Transaction tr = new Transaction(Globals.Doc))
            //{
            //tr.Start("Create detailed Unit");


            // Get zone length from generic conveyor...

            Parameter zoneLength = instance.LookupParameter("Zone_Length");

            double zl = zoneLength.AsDouble();

            // Placing beds in order...
            List<DetailedBed> bedsTobeInserted = GetBedsLogic(startPoint, endPoint, (int)zl);
            List<FamilyInstance> placedBeds = new List<FamilyInstance>();
            foreach (var bed in bedsTobeInserted)
            {
                FamilyInstance inst = bed.PlaceBed(convWidth, unitId, elevation, (double)zl);
                XYZ pp = (inst.Location as LocationPoint).Point; 
                placedBeds.Add(inst);
                if (bed.HasDrive)
                {
                    placedBeds.Add(bed.PlaceDrive(isLeftHand, unitId, elevation));
                }
                placedBeds.AddRange(bed.PlaceSupports(inst, unitId, elevation, convWidth));

            }

            XYZ direction = (endPoint - startPoint).Normalize();
            PlaceGuideRail(oal, startPoint, direction, ref placedBeds, convWidth, elevation, ref error);

            //Rotate if needed

            //Delete generic family
            Globals.Doc.Delete(instance.Id);

        return placedBeds;
    }


        public List<DetailedBed> GetBedsLogic(XYZ startpoint, XYZ endpoint, int zoneLength)
        {
            List<DetailedBed> outBeds = new List<DetailedBed>();

            double entryExitLength = zoneLength + 0.5;

            XYZ direction = (endpoint - startpoint).Normalize();

                // Rules and formulas
            double oal = Math.Round(startpoint.DistanceTo(endpoint), 4);
            //double oal = (int) (startpoint.DistanceTo(endpoint) * 12) / 12.0;

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
            brakeBed.StartPoint = exitBed.StartPoint - (12) * direction;
            brakeBed.Direction = direction;

            outBeds.Add(entryBed);
            outBeds.Add(brakeBed);
            outBeds.Add(exitBed);

            // Remaining length after deducting enter, exit, brake...
            double remainingLength = oal - entryBed.Length - exitBed.Length - brakeBed.Length;

            if ((int) remainingLength / 12 == 0)
            {
                // Add the drive to brake bed...
                brakeBed.HasDrive = true;
                
                // Add CTF C351...
                double ctfLen = remainingLength % 12;
                DetailedBed ctf351 = new DetailedBed();
                ctf351.Length = ctfLen;
                ctf351.BedType = BedType.C351CTF;
                ctf351.StartPoint = entryBed.GetEndPoint();
                ctf351.Direction = direction;

                remainingLength -= ctf351.Length;

                outBeds.Add(ctf351);

            }
            else // Here we have at least one full intermediate bed, unless the case of two CTFs...
            {
                int full352Count = (int) remainingLength / 12;
                XYZ lastPoint = new XYZ();
                if (remainingLength % 12 > 0 && remainingLength % 12 < 2 && full352Count > 0)
                {
                    //This is the case when we need CTF C352 with length 6 ft beside 7 ft C351...
                    DetailedBed ctf351 = new DetailedBed();
                    ctf351.Length = (remainingLength - 6) % 12;
                    ctf351.BedType = BedType.C351CTF;
                    ctf351.StartPoint = entryBed.GetEndPoint();
                    ctf351.Direction = direction;


                    DetailedBed ctf352 = new DetailedBed();
                    ctf352.Length = 6;
                    ctf352.BedType = BedType.C352;
                    ctf352.StartPoint = ctf351.GetEndPoint();
                    ctf352.Direction = direction;


                    remainingLength -= (6 + ctf351.Length);

                    outBeds.Add(ctf351);
                    outBeds.Add(ctf352);

                    lastPoint = ctf352.GetEndPoint();

                    // Don't forget to deduct the full 352 by 1...
                    full352Count--;
                    if (full352Count == 0) brakeBed.HasDrive = true;
                }
                else // Here we have at least one full bed and a ctf 351 normally...
                {
                    double ctfLen = remainingLength % 12;
                    if (ctfLen != 0)
                    {
                        DetailedBed ctf351 = new DetailedBed();
                        ctf351.Length = ctfLen;
                        ctf351.BedType = BedType.C351CTF;
                        ctf351.StartPoint = entryBed.GetEndPoint();
                        ctf351.Direction = direction;


                        remainingLength -= ctf351.Length;

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

        public void PlaceGuideRail(double oal, XYZ startPoint, XYZ Direction, ref List<FamilyInstance> beds, int convWidth, double elevation, ref string error)
        {
            // Adding guiderails...
            FamilySymbol guideRailSymbol = FamilyHelper.getFamilySymbolwithoutTransaction(Constants.GuideRailFamilyName, Constants.GuideRailFamilyFileName, null, convWidth, ref error);
            int count = (int)(oal / 10);
            double length = 0;
            for (int i = 0; i < count; i++)
            {
                FamilyInstance inst = FamilyHelper.placePointFamilyWithSubTransaction(guideRailSymbol, startPoint + length*Direction, 10);
                if (inst != null)
                    inst.RotateFamilyToDirection(Globals.Doc, Direction, startPoint + length * Direction);
                inst.SetParameter(Constants.Conveyor_Elevation_In, elevation);
                beds.Add(inst);
                length += 10;
            }
            if(oal%10 > 0)
            {
                FamilyInstance inst = FamilyHelper.placePointFamilyWithSubTransaction(guideRailSymbol, startPoint + length * Direction, oal % 10);
                if (inst != null)
                    inst.RotateFamilyToDirection(Globals.Doc, Direction, startPoint + length * Direction);
                inst.SetParameter(Constants.Conveyor_Elevation_In, elevation);
                beds.Add(inst);
            }
                
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
        public new FamilyInstance ConvertBackToGeneric(List<FamilyInstance> detailedFamilies)
        {
            return null;
        }
    }
}
