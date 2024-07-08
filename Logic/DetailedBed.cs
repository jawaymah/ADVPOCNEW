using AdvansysPOC.Helpers;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;


namespace AdvansysPOC.Logic
{
    public class DetailedBed
    {
        public DetailedBed()
        {
            this.HasDrive = false;
        }
        //public DetailedBed NextBed { get; set; }
        //public DetailedBed PrevBed { get; set; }
        public XYZ StartPoint { get; set; }
        public XYZ Direction { get; set; }
        //public XYZ EndPoint { get; set; }
        public double Length { get; set; }
        public BedType BedType { get; set; }
        public bool HasDrive { get; set; }

        public XYZ GetEndPoint()
        {
            return StartPoint + Direction * Length;
            //to be implemented later...
            //return EndPoint.DistanceTo(StartPoint);
        }

        public FamilyInstance PlaceBed(int width)
        {
            string error = "";
            string familyName = Constants.CTFFamilyName;
            string fileName = Constants.CTFFamilyFileName;


            switch (BedType)
            {
                case BedType.None:
                    break;
                case BedType.Brake:
                    familyName = Constants.BrakeBedFamilyName;
                    fileName = Constants.BrakeBedFileName;
                    break;
                case BedType.C351CTF:
                    familyName = Constants.CTFFamilyName;
                    fileName = Constants.CTFFamilyFileName;
                    break;
                case BedType.C352:
                    familyName = Constants.IntermediateFamilyName;
                    fileName = Constants.IntermediateFamilyFileName;
                    break;
                case BedType.EntryBed:
                    familyName = Constants.EntranceBedFamilyName;
                    fileName = Constants.EntranceBedFileName;
                    break;
                case BedType.ExitBed:
                    familyName = Constants.ExitBedFamilyName;
                    fileName = Constants.ExitBedFileName;
                    break;
                case BedType.Drive:
                    familyName = Constants.DriveFamilyName;
                    fileName = Constants.DriveFamilyFileName;
                    break;
                default:
                    break;
            }

            FamilySymbol symbol = FamilyHelper.getFamilySymbolwithoutTransaction(familyName, fileName, null, width, ref error);
            FamilyInstance ins = FamilyHelper.placePointFamilyWithSubTransaction(symbol, StartPoint, Length);
            if (ins != null)
            {
                ins.RotateFamilyToDirection(Globals.Doc, Direction, StartPoint);
            }
            return ins;
        }

        //public FamilyInstance PlaceDrive(bool ConveyorHandLeft)
        //{
        //    string error = "";
        //    string familyName = Constants.DriveFamilyName;
        //    string fileName = Constants.DriveFamilyFileName;
        //    FamilySymbol symbol = FamilyHelper.getFamilySymbolwithoutTransaction(familyName, fileName, null, ref error);
        //    FamilyInstance ins = FamilyHelper.placePointFamilyWithSubTransaction(symbol, StartPoint, Length);
        //    if (ins != null)
        //    {
        //        ins.RotateFamilyToDirection(Globals.Doc, Direction, StartPoint, ConveyorHandLeft);
        //    }
        //    return ins;
        //}


        public List<FamilyInstance> PlaceSupports(FamilyInstance parentBed)
        {
            double conveyorIn = 2.5;
            if (parentBed != null)
            {
                var parameter = parentBed.LookupParameter(Constants.Conveyor_Elevation_In);
                if (parameter != null)
                {
                    conveyorIn = parameter.AsDouble();
                }
            }
            string error = "";
            string familyName = Constants.SupportFamilyName;
            string fileName = Constants.SupportFamilyFileName;
            List<FamilyInstance> supports = new List<FamilyInstance>();
            FamilySymbol symbol = FamilyHelper.getFamilySymbolwithoutTransaction(familyName, fileName, null,0,  ref error);
            FamilyInstance insStart = FamilyHelper.placePointFamilyWithSubTransaction(symbol, StartPoint, Length);
            if (insStart != null)
            {
                insStart.RotateFamilyToDirection(Globals.Doc, Direction, StartPoint);
                insStart.SetTypeParameter(Constants.Conveyor_Elevation_In, conveyorIn);
            }
            supports.Add(insStart);
            if (BedType == BedType.ExitBed)
            {
                FamilyInstance insEnd = FamilyHelper.placePointFamilyWithSubTransaction(symbol, GetEndPoint(), Length);
                if (insEnd != null)
                {
                    insEnd.RotateFamilyToDirection(Globals.Doc, Direction, GetEndPoint());
                    insEnd.SetTypeParameter(Constants.Conveyor_Elevation_In, conveyorIn);
                }
                supports.Add(insEnd);
            }

            return supports;

        }
    }
}