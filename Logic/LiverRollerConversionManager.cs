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

            // Get guardrail height...
            double grHeight = instance.LookupParameter("GR_Height").AsDouble();

            // Get speed...
            double speed = instance.LookupParameter("CLR_SPEED_FPM").AsDouble();


            int convWidth = (int)(12 * instance.Symbol.LookupParameter("Conveyor_OAW").AsDouble());
            double convWidthFeet = instance.Symbol.LookupParameter("Conveyor_OAW").AsDouble();
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
            double zl = 2;
            Parameter zoneLength = instance.LookupParameter("Zone_Length");
            if (zoneLength != null)
                zl = zoneLength.AsDouble();

            // check if the generic conveyor has Ceiling support...
            bool ceilingSupport = false;
            Parameter hasceilingSupport = instance.LookupParameter(Constants.HasHungerSupport);
            if (hasceilingSupport != null)
                ceilingSupport = hasceilingSupport.AsInteger() == 1;

            Parameter hasEntry = instance.LookupParameter("Child_Entry");
            bool hasEntryValue = hasEntry.AsInteger() == 0;
            
            Parameter hasExit = instance.LookupParameter("Child_Exit");
            bool hasExitValue = hasExit.AsInteger() == 0;


            // Placing beds in order...
            List<DetailedBed> bedsTobeInserted = GetBedsLogic(startPoint, endPoint, (int)zl, hasEntryValue, hasExitValue);
            List<FamilyInstance> placedBeds = new List<FamilyInstance>();
            foreach (var bed in bedsTobeInserted)
            {
                FamilyInstance inst = bed.PlaceBed(convWidth, unitId, elevation, (double)zl);
                XYZ pp = (inst.Location as LocationPoint).Point; 
                placedBeds.Add(inst);
                if (bed.HasDrive)
                {
                    placedBeds.Add(bed.PlaceDrive(isLeftHand, unitId, elevation, speed));
                }
                placedBeds.AddRange(bed.PlaceSupports(inst, unitId, elevation, convWidth, ceilingSupport, false));

            }
            if (!hasExitValue)
            {
                placedBeds.Add(bedsTobeInserted.First().PlaceExitSupport(placedBeds.First(), endPoint, elevation, ceilingSupport, unitId, convWidth));
            }


            XYZ direction = (endPoint - startPoint).Normalize();
            Globals.Doc.Regenerate();
            PlaceGuideRail(oal, grHeight, startPoint, direction, ref placedBeds, convWidth, elevation, ref error);
            PlaceControlDevices(oal, grHeight, startPoint, direction, ref placedBeds, convWidth, elevation, zl, ref error);
            //Rotate if needed

            //Delete generic family
            Globals.Doc.Delete(instance.Id);

        return placedBeds;
    }


        public List<DetailedBed> GetBedsLogic(XYZ startpoint, XYZ endpoint, int zoneLength, bool hasEntry, bool hasExit)
        {
            List<DetailedBed> outBeds = new List<DetailedBed>();

            double entryExitLength = zoneLength + 0.5;

            XYZ direction = (endpoint - startpoint).Normalize();

                // Rules and formulas
            double oal = Math.Round(startpoint.DistanceTo(endpoint), 4);
            //double oal = (int) (startpoint.DistanceTo(endpoint) * 12) / 12.0;

            //Entry...
            DetailedBed entryBed = new DetailedBed(direction);
            if (hasEntry)
            {
                entryBed.BedType = BedType.EntryBed;
                entryBed.StartPoint = startpoint;
                entryBed.Length = entryExitLength;
            }
            else
            {
                entryBed.StartPoint = startpoint;
            }

            //Exit...
            DetailedBed exitBed = new DetailedBed(direction);
            if (hasExit)
            {
                exitBed.BedType = BedType.ExitBed;
                exitBed.Length = entryExitLength;
                exitBed.StartPoint = endpoint - entryExitLength * direction;
            }
            else
            {
                exitBed.StartPoint = endpoint;
            }


            //Brake...
            DetailedBed brakeBed = new DetailedBed();
            brakeBed.Length = 12;
            brakeBed.BedType = BedType.Brake;
            brakeBed.StartPoint = exitBed.StartPoint - (12) * direction;
            brakeBed.Direction = direction;

            if (hasEntry) outBeds.Add(entryBed);
            outBeds.Add(brakeBed);
            if (hasExit) outBeds.Add(exitBed);

            double remainingLength = oal;

            // Remaining length after deducting enter, exit, brake...
            foreach (var bed in outBeds)
            {
                remainingLength -= bed.Length;
            }

            //double remainingLength = oal - entryBed.Length - exitBed.Length - brakeBed.Length;

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

        public void PlaceGuideRail(double oal, double grHeight,XYZ startPoint, XYZ Direction, ref List<FamilyInstance> beds, int convWidth, double elevation, ref string error)
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
                inst.SetParameter(Constants.GR_Height, grHeight);
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

        public List<FamilyInstance> PlaceControlDevices(double oal, double grHeight, XYZ startPoint, XYZ Direction, ref List<FamilyInstance> beds, int convWidth, double elevation, double zoneLength, ref string error)
        {
            List<FamilyInstance> devices = new List<FamilyInstance>();
            PlacePEM(startPoint, oal, Direction, ref beds, convWidth, elevation, ref error);
            XYZ motorLocation = PlaceMotor(startPoint, Direction, ref beds, convWidth, elevation, ref error);
            XYZ DISCLocation = PlaceDISC(motorLocation, Direction, ref beds, convWidth, elevation, ref error);
            PlaceVFD(DISCLocation, Direction, ref beds, convWidth, elevation, ref error);
            PlaceZIM(startPoint, oal, Direction, ref beds, convWidth, elevation, ref error);
            PlaceSolenoids(startPoint, oal, Direction, ref beds, convWidth, elevation, ref error);
            PlacePowerSupplies(startPoint, oal, Direction, ref beds, convWidth, elevation, zoneLength, ref error);
            return devices;
        }
        public void PlacePEM(XYZ startPoint, double oal, XYZ Direction, ref List<FamilyInstance> beds, int convWidth, double elevation, ref string error)
        {
            // Adding guiderails...
            FamilySymbol PEMSymbol = FamilyHelper.getFamilySymbolwithoutTransaction(Constants.PEMFamilyName, Constants.PEMFamilyFileName, null, convWidth, ref error);
            FamilyInstance ctf351 = beds.FirstOrDefault(b => b.Symbol.FamilyName == Constants.CTFFamilyName);
            if (ctf351 != null)
            {
                XYZ normalizedDirection = Direction.Normalize();
                double bedLength = ctf351.LookupParameter(Constants.Bed_Length).AsDouble();
                //XYZ bedStartLocation = startPoint.Add(normalizedDirection*(3.5));
                XYZ PEMLocation = (ctf351.Location as LocationPoint).Point.Add(normalizedDirection * (bedLength - 1));
                XYZ normal = normalizedDirection.CrossProduct(new XYZ(0, 0, 1));
                PEMLocation = PEMLocation.Add(normal * 0.5 * convWidth / 12);
                //XYZ PEMLocation = PEMLocation.Subtract(normalizedDirection).Add(normalizedDirection.CrossProduct(new XYZ(0, 0, 1) * convWidth / 2));
                FamilyInstance startPEM = FamilyHelper.placePointFamilyWithSubTransaction(PEMSymbol, PEMLocation, 0);
                beds.Add(startPEM);
                XYZ endPEMLocation = startPoint.Add(normalizedDirection * (oal - 1.5)).Add(normal * convWidth * 0.5 / 12);
                FamilyInstance endPEM = FamilyHelper.placePointFamilyWithSubTransaction(PEMSymbol, endPEMLocation, 0);
                beds.Add(endPEM);
            }
        }

        public XYZ PlaceMotor(XYZ startPoint, XYZ Direction, ref List<FamilyInstance> beds, int convWidth, double elevation, ref string error)
        {
            // Adding guiderails...
            FamilySymbol motorSymbol = FamilyHelper.getFamilySymbolwithoutTransaction(Constants.MotorFamilyName, Constants.MotorFamilyFileName, null, convWidth, ref error);
            FamilyInstance c370 = beds.FirstOrDefault(b => b.Symbol.FamilyName == Constants.DriveFamilyName);
            if (c370 != null)
            {
                XYZ normalizedDirection = Direction.Normalize();
                XYZ bedStartLocation = (c370.Location as LocationPoint).Point;
                XYZ motorLocation = bedStartLocation.Add(normalizedDirection * 6);
                FamilyInstance inst = FamilyHelper.placePointFamilyWithSubTransaction(motorSymbol, motorLocation, 0);
                beds.Add(inst);
                return motorLocation;
            }
            return null;
        }

        public XYZ PlaceDISC(XYZ motorLoction, XYZ Direction, ref List<FamilyInstance> beds, int convWidth, double elevation, ref string error)
        {
            FamilySymbol DISCSymbol = FamilyHelper.getFamilySymbolwithoutTransaction(Constants.DISCFamilyName, Constants.DISCFamilyFileName, null, convWidth, ref error);
            if (motorLoction != null && DISCSymbol != null)
            {
                XYZ normalizedDirection = Direction.Normalize();
                double offset = (convWidth * 0.5 / 12) + 0.3;
                XYZ DISCLocation = motorLoction.Add(normalizedDirection.CrossProduct(new XYZ(0, 0, 1)) * offset);
                FamilyInstance inst = FamilyHelper.placePointFamilyWithSubTransaction(DISCSymbol, DISCLocation, 0);
                beds.Add(inst);
                return DISCLocation;
            }
            return null;
        }


        public void PlaceVFD(XYZ DISCLocation, XYZ Direction, ref List<FamilyInstance> beds, int convWidth, double elevation, ref string error)
        {
            // Adding guiderails...
            FamilySymbol VFDSymbol = FamilyHelper.getFamilySymbolwithoutTransaction(Constants.VFDFamilyName, Constants.VFDFamilyFileName, null, convWidth, ref error);
            FamilyInstance c370 = beds.FirstOrDefault(b => b.Symbol.FamilyName == Constants.DriveFamilyName);
            if (c370 != null)
            {
                XYZ normalizedDirection = Direction.Normalize();
                XYZ bedStartLocation = (c370.Location as LocationPoint).Point;
                XYZ VFDLocation = DISCLocation.Add(normalizedDirection);
                VFDLocation = VFDLocation.Add(normalizedDirection * convWidth * 0.5 / 12);
                FamilyInstance inst = FamilyHelper.placePointFamilyWithSubTransaction(VFDSymbol, VFDLocation, 0);
                beds.Add(inst);
            }
        }

        public void PlaceZIM(XYZ startPoint, double oal, XYZ Direction, ref List<FamilyInstance> beds, int convWidth, double elevation, ref string error)
        {
            // Adding guiderails...
            FamilySymbol ZIMSymbol = FamilyHelper.getFamilySymbolwithoutTransaction(Constants.ZIMFamilyName, Constants.ZIMFamilyFileName, null, convWidth, ref error);
            if (ZIMSymbol != null)
            {
                XYZ normalizedDirection = Direction.Normalize();
                XYZ normal = normalizedDirection.CrossProduct(new XYZ(0, 0, 1));
                XYZ ZIMLocation = startPoint.Add(normalizedDirection * (oal - 2 - ((double)2 / 12)));
                double offset = (convWidth * 0.5 / 12) + 0.3;
                ZIMLocation = ZIMLocation.Add(normal * offset);
                FamilyInstance inst = FamilyHelper.placePointFamilyWithSubTransaction(ZIMSymbol, ZIMLocation, 0);
                beds.Add(inst);
            }
        }

        public void PlaceSolenoids(XYZ startPoint, double oal, XYZ Direction, ref List<FamilyInstance> beds, int convWidth, double elevation, ref string error)
        {
            FamilySymbol solSymbol = FamilyHelper.getFamilySymbolwithoutTransaction(Constants.SolenoidFamilyName, Constants.SolenoidFamilyFileName, null, convWidth, ref error);
            if (solSymbol != null)
            {
                XYZ normalizedDirection = Direction.Normalize();
                XYZ solLocation = startPoint.Add(normalizedDirection * (oal - 1.5));
                FamilyInstance inst = FamilyHelper.placePointFamilyWithSubTransaction(solSymbol, solLocation, 0);
                beds.Add(inst);
            }
        }

        public void PlacePowerSupplies(XYZ startPoint, double oal, XYZ Direction, ref List<FamilyInstance> beds, int convWidth, double elevation, double zoneLength, ref string error)
        {
            FamilySymbol powerSupplySymbol = FamilyHelper.getFamilySymbolwithoutTransaction(Constants.PowerSupplyFamilyName, Constants.PowerSupplyFamilyFileName, null, convWidth, ref error);
            if (powerSupplySymbol != null)
            {
                int zonesCount = (int)Math.Ceiling((double)oal / zoneLength);
                List<XYZ> locatons = new List<XYZ>();
                XYZ normalizedDirection = Direction.Normalize();
                if (zonesCount > Constants.MaxZonesPerPowerSupply)
                {
                    locatons.Add(startPoint.Add(normalizedDirection * 0.25 * oal));
                    locatons.Add(startPoint.Add(normalizedDirection * 0.75 * oal));
                }
                else
                {
                    locatons.Add(startPoint.Add(normalizedDirection * 0.5 * oal));
                }
                XYZ reverseNormal = normalizedDirection.CrossProduct(new XYZ(0, 0, -1));
                for (int i = 0; i < locatons.Count; i++)
                {
                    locatons[i] = locatons[i].Add(reverseNormal * convWidth * 0.5 / 12);
                    FamilyInstance inst = FamilyHelper.placePointFamilyWithSubTransaction(powerSupplySymbol, locatons[i], 0);
                    beds.Add(inst);
                }
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
